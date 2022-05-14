using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Kinect;
using Microsoft.Kinect.VisualGestureBuilder;

namespace NotCorporateSlave
{
    class BodyFrameManager
    {
        private KinectSensor sensor;
        private DrawingGroup drawingGroup;
        private DrawingImage drawingImg;
        private static double drawingImgWidth = 1920, drawingImgHeight = 1080;
        private VisualGestureBuilderFrameSource vgbFrameSource;
        private VisualGestureBuilderDatabase vgbDb;
        private VisualGestureBuilderFrameReader vgbFrameReader;
        private BodyFrameReader bodyFrameReader;
        private TextBlock debug_1;
        private Random rand = new Random();

        public void Init(KinectSensor s, Image wpfImageForDisplay, TextBlock debug)
        {
            sensor = s;
            debug_1 = debug;
            drawingImgWidth = sensor.DepthFrameSource.FrameDescription.Width;
            drawingImgHeight = sensor.DepthFrameSource.FrameDescription.Height;

            layoutInit();
            GameInit();
            DrawingGroupInit(wpfImageForDisplay);
            BodyFrameReaderInit();
            vgbFrameInit();
            timerInit();

        }

        private void DrawingGroupInit(Image wpfImageForDisplay) //codes referenced from T8_PoseMatching
        {
            drawingGroup = new DrawingGroup();
            drawingImg = new DrawingImage(drawingGroup);
            wpfImageForDisplay.Source = drawingImg;

            // prevent drawing outside of our render area
            drawingGroup.ClipGeometry = new RectangleGeometry(
                                        new Rect(0.0, 0.0, drawingImgWidth, drawingImgHeight));
        }

        private void BodyFrameReaderInit() //codes referenced from T8_PoseMatching
        {
            bodyFrameReader = sensor.BodyFrameSource.OpenReader();
            bodyFrameReader.FrameArrived += BodyFrameReader_FrameArrived;

        }

        private void BodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame == null) return;
                Body body = GetClosestBody(bodyFrame);
                if (body != null)
                {
                    vgbFrameSource.TrackingId = body.TrackingId;
                }

                using (DrawingContext dc = drawingGroup.Open())
                {
                    // draw a transparent background to set the render size
                    dc.DrawRectangle(Brushes.Transparent, null,
                            new Rect(0.0, 0.0, drawingImgWidth, drawingImgHeight));              
                    drawLayout(dc);
                    draw(dc);
                    CheckWinLose();
                }
            }

        }

        private Body GetClosestBody(BodyFrame bodyFrame)    //referenced from T9_GesturesRecognition
        {
            Body[] bodies = new Body[6];
            bodyFrame.GetAndRefreshBodyData(bodies);

            Body closestBody = null;
            foreach (Body b in bodies)
            {
                if (b.IsTracked)
                {
                    if (closestBody == null) closestBody = b;
                    else
                    {
                        Joint newHeadJoint = b.Joints[JointType.Head];
                        Joint oldHeadJoint = closestBody.Joints[JointType.Head];
                        if (newHeadJoint.TrackingState == TrackingState.Tracked &&
                        newHeadJoint.Position.Z < oldHeadJoint.Position.Z)
                        {
                            closestBody = b;
                        }
                    }
                }
            }
            return closestBody;
        }

        private void vgbFrameInit()      //referenced from T9_GesturesRecognition
        {
            vgbFrameSource = new VisualGestureBuilderFrameSource(sensor, 0);
            vgbDb = new VisualGestureBuilderDatabase(@".\Gestures\projectGestures.gbd");
            vgbFrameSource.AddGestures(vgbDb.AvailableGestures);

            vgbFrameReader = vgbFrameSource.OpenReader();
            vgbFrameReader.FrameArrived += VgbFrameReader_FrameArrived;
        }

        private bool isLeft = false;
        private bool isRight = false;
        private bool isRun = false;
        private bool leftActivated = false;
        private bool rightActivated = false;
        private void VgbFrameReader_FrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e)
        {
            using (VisualGestureBuilderFrame vgbFrame = e.FrameReference.AcquireFrame())
            {
                if (vgbFrame == null) return;
                
                IReadOnlyDictionary<Gesture, DiscreteGestureResult> results =
                vgbFrame.DiscreteGestureResults;

                ProcessRecognizedResults(results);
                player.changeState(isRun);
                if (isRun)
                {
                    if(isStart == false)
                    {
                        isStart = true;
                    }
                    Progress_bar.update_player_run();
                    updateItem();
                }
                if (isStart)
                {
                    update_cctv();
                    Progress_bar.update_boss_run();
                    Progress_bar.boss_player();

                    if (isLeft && leftActivated == false)
                    {
                        player.move(-1, numOfTrail, trailList);
                        leftActivated = true;
                    }
                    if (isRight && rightActivated == false)
                    {
                        player.move(1, numOfTrail, trailList);
                        rightActivated = true;
                    }
                    if (isLeft == false)
                    {
                        leftActivated = false;
                    }
                    if (isRight == false)
                    {
                        rightActivated = false;
                    }
                    
                }
            }

        }

        private void resetGestures()
        {
            isLeft = false;
            isRight = false;
            isRun = false;
        }

        private void ProcessRecognizedResults(IReadOnlyDictionary<Gesture, DiscreteGestureResult> results)
        {

            if (results != null)
            {
                // Check if any of the gestures is recognized 
                resetGestures();

                foreach (Gesture gesture in results.Keys)
                {
                    DiscreteGestureResult result = results[gesture];
                    if (result.Detected)
                    {
                        //debug_1.Text = gesture.Name + " gesture recognized; confidence: " + result.Confidence;
                        //recognized = true;

                        if (gesture.Name.Equals("hand_halfup_Left"))
                        {
                            isLeft = true;
                        }
                        if (gesture.Name.Equals("hand_halfup_Right"))
                        {
                            isRight = true;
                        }
                        if (gesture.Name.Equals("run"))
                        {
                            isRun = true;
                        }
                    }
                }
                //if (!recognized) debug_1.Text = "No gesture recognized";
            }
        }

        private Rect ProgressBar_box;
        private Rect Main_box;
        private Rect HealthBar_box;
        private Rect Score_box;
        private void layoutInit()
        {
            double gap = 10;
            ProgressBar_box = new Rect(gap,gap,drawingImgWidth-gap*2,50);
            Score_box = new Rect(gap, ProgressBar_box.Height + gap * 2, 30, 30);
            HealthBar_box = new Rect(gap, ProgressBar_box.Height + Score_box.Height + gap*3, 30, drawingImgHeight - ProgressBar_box.Height - gap * 3);
            Main_box = new Rect(HealthBar_box.Width + gap * 2, ProgressBar_box.Height + gap * 2, drawingImgWidth - HealthBar_box.Width - gap * 3, drawingImgHeight - ProgressBar_box.Height - gap * 3);
        }

        private void drawLayout(DrawingContext dc)
        {
            //dc.DrawRectangle(Brushes.Red, null, ProgressBar_box);
            //dc.DrawRectangle(Brushes.Blue, null, HealthBar_box);
            dc.DrawRectangle(Brushes.Gray, null, Main_box);
            dc.DrawRectangle(Brushes.Black, null, Score_box);
        }

        private bool isStart;
        public int numOfTrail = 4;
        private const int max_trail = 8;
        private const int min_trail = 3;
        private double My_score = 0;
        private double trail_w;
        private List<Trails> trailList;
        private Players player;
        private ProgressBar Progress_bar;
        private List<Document> documents;
        private List<Entertainment> entertainments;
        private CCTV cctv;
        public void GameInit()
        {
            isStart = false;
            My_score = 0;

            trailList = new List<Trails>();
            trail_w = ((double)max_trail - (double)numOfTrail) / ((double)max_trail - (double)min_trail) * 20 + 30;
            for(int i = 0; i < numOfTrail; i++)
            {
                double x = Main_box.X + Main_box.Width * (i+1) / (numOfTrail + 1);
                double y = Main_box.Y;
                trailList.Add(new Trails(i, x, y, trail_w, Main_box.Height));
            }

            player = new Players(trailList[0].region.X + trailList[0].region.Width / 2, Main_box.Y + Main_box.Height, trailList[0].region.Width, HealthBar_box.X, HealthBar_box.Y, HealthBar_box.Width);

            Progress_bar = new ProgressBar(ProgressBar_box.X, ProgressBar_box.Y, ProgressBar_box.Width, ProgressBar_box.Height);

            documents = new List<Document>();
            entertainments = new List<Entertainment>();
            cctv = new CCTV(trailList, numOfTrail, trail_w, Main_box.Height);

            interval = (int)(((double)max_trail - (double)numOfTrail) / ((double)max_trail - (double)min_trail) * 600 + 400);
            spawnItem_t.Interval = new TimeSpan(0, 0, 0, 0, interval);

            max_score = 1 / ((double)interval / 1000) * 65 * (1 - doc_prob);
        }

        private static System.Windows.Threading.DispatcherTimer spawnItem_t = new System.Windows.Threading.DispatcherTimer();
        private int interval;
        private static System.Windows.Threading.DispatcherTimer spawnCCTV_t = new System.Windows.Threading.DispatcherTimer();
        public void timerInit()
        {
            

            spawnItem_t.Interval = new TimeSpan(0, 0, 0, 0, interval);
            spawnItem_t.Tick += spawnItem_ehr;
            spawnItem_t.Start();

            spawnCCTV_t.Interval = new TimeSpan(0, 0, 0, 8);
            spawnCCTV_t.Tick += spawnCCTV_ehr;
            spawnCCTV_t.Start();
        }

        
        public double doc_prob = 0.4;
        private void spawnItem_ehr(object sender, EventArgs e)
        {
            if (isStart&&isRun)
            {
                double index;
                index = rand.NextDouble();
                if (index < doc_prob)
                {
                    documents.Add(new Document(trailList, numOfTrail, trail_w));
                }
                else
                {  
                    entertainments.Add(new Entertainment(trailList, numOfTrail, trail_w));
                }
                
            }
            
        }
        private void spawnCCTV_ehr(object sender, EventArgs e)
        {
            if (isStart)
            {
                if(cctv.isActivated == false)
                {
                    cctv.isActivated = true;
                }
            }
        }
        private void draw(DrawingContext dc)
        {
            for(int i = 0; i < trailList.Count; i++)
            {
                trailList[i].draw(dc);
            }

            player.draw(dc);
            Progress_bar.draw(dc);
            player.healthBar.draw(dc);

            for(int i = 0; i < documents.Count; i++)
            {
                documents[i].draw(dc);
            }
            for(int i = 0; i < entertainments.Count; i++)
            {
                entertainments[i].draw(dc);
            }

            cctv.draw(dc);
            //doc = documents.Count+entertainments.Count;
            showText(dc);
        }

        
        private void updateItem()
        {
            for(int i = 0; i < documents.Count; i++)
            {
                documents[i].update(Main_box.Y + Main_box.Height);
                if (player.Document_Player(documents[i].pos))
                {
                    documents[i].isOut = true;
                }
            }
            for (int i = 0; i < documents.Count; i++)
            {
                if (documents[i].isOut)
                {
                    documents.RemoveAt(i);
                    break;
                }
            }

            for (int i = 0; i < entertainments.Count; i++)
            {
                entertainments[i].update(Main_box.Y + Main_box.Height);
                if (player.Entertainment_Player(entertainments[i].pos))
                {
                    My_score++;
                    entertainments[i].isOut = true;
                }
            }
            for (int i = 0; i < entertainments.Count; i++)
            {
                if (entertainments[i].isOut)
                {
                    entertainments.RemoveAt(i);
                    break;
                }
            }
        }

        private void update_cctv()
        {
            cctv.update();
            if (player.CCTV_Player(cctv.region))
            {
                //cctv.reset();
                cctv.isWatch = true;
            }
        }

        private void CheckWinLose()
        {
            if (Progress_bar.isReach || Progress_bar.isCaught || player.isExhausted || cctv.isWatch)
            {
                if (Progress_bar.isReach)
                {
                    messageBox("You successfully escape from your Boss");
                }
                if (Progress_bar.isCaught)
                {
                    messageBox("You are catched by your Boss");
                }
                if (player.isExhausted)
                {
                    messageBox("You become the corporate slave");
                }
                if (cctv.isWatch)
                {
                    messageBox("You are discovered by the CCTV");
                }
                spawnItem_t.Stop();
                spawnCCTV_t.Stop();
            }

        }
        double max_score;
        private void messageBox(string s)
        {
            String grade;
            if (Progress_bar.isReach)
            {
                My_score += max_score*0.3;
            }
            if (My_score < max_score * 0.1)
            {
                grade = "E";
            } else if (My_score < max_score * 0.2)
            {
                grade = "D";
            } else if(My_score < max_score * 0.4)
            {
                grade = "C";
            } else if(My_score < max_score * 0.65)
            {
                grade = "B";
            } else if(My_score < max_score* 0.9)
            {
                grade = "A";
            } else
            {
                grade = "S";
                if (Progress_bar.isReach)
                {
                    grade = "S+";
                }
            }
            MessageBoxResult option = MessageBox.Show("Grade: " +grade + "\n" + "Would you like to retart the game?", s, MessageBoxButton.YesNo);
            switch (option)
            {
                case MessageBoxResult.Yes:
                    System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                    break;

                case MessageBoxResult.No:
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                    break;
            }
        }

        
        //private int doc = 0;
        private void DrawText(DrawingContext dc, String text, Point position, double size,         //T8_PoseMatching.T8_PositionBasedMatching
        Typeface typeFace, Brush color)
        {
            FormattedText formattedText = new FormattedText(text,
                System.Globalization.CultureInfo.GetCultureInfo("en-us"),
            FlowDirection.LeftToRight, typeFace, size, color);

            dc.DrawText(formattedText, position);
        }
        private void showText(DrawingContext dc)
        {
            String score = "" + My_score;
            DrawText(dc, score, new Point(Score_box.X,Score_box.Y), 15, new Typeface("Calibri"), Brushes.White);

            String cctv_count = "" + cctv.countDown;
            if (cctv.isActivated)
            {
                DrawText(dc, cctv_count, new Point(cctv.pos.X+cctv.pos.Width, cctv.pos.Y), 15, new Typeface("Calibri"), Brushes.Black);
            }

            String debug = "Run:" + isRun + " " + "Left:" + isLeft + " " + "Right:" + isRight + "max score:" + max_score;
            DrawText(dc, debug, new Point(0, drawingImgHeight - 15), 15, new Typeface("Calibri"), Brushes.Black);

            String Run_start = "Run to start!";
            if(isStart == false)
            {
                DrawText(dc, Run_start, new Point(drawingImgWidth/2-100, drawingImgHeight/2), 50, new Typeface("Calibri"), Brushes.Black);
            }
            
        }
    }
}
