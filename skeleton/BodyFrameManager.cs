using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Kinect;

namespace KillVirus
{
    public class BodyFrameManager
    {
        private KinectSensor sensor;

        private Body[] bodies;

        private Boolean mapToColorSpace = true;

        private DrawingGroup drawingGroup;
        private DrawingImage drawingImg;
        private static double drawingImgWidth = 1920, drawingImgHeight = 1080;

        private Boolean IsStart = false;

        private static int row = 10;
        private static int column = 10;
        double x_interval = drawingImgWidth / column;
        double y_interval = drawingImgHeight / row;

        private Heart heart = new Heart();

        private Virus[] virus = new Virus[100]; //10*10 grid == 10*y+x

        private Spray[] sprays = new Spray[2]; //one for left, one for right


        private Vector left_hand_div = new Vector(0, 0);
        private Vector right_hand_div = new Vector(0, 0);
        Vector left_EW = new Vector(0, 0);
        Vector right_EW = new Vector(0, 0);

        private int gameLevel = 0;
        private int NumOfKilledVirus = 0;
        private int GeneratePerSec = 1;

        private Boolean IsWin = false;
        private Boolean IsLose = false;


        private static System.Windows.Threading.DispatcherTimer VirusGenTimer = new System.Windows.Threading.DispatcherTimer();
        private Boolean left_t_activated = false;
        private Boolean right_t_activated = false;

        private Point pt_lefthand = new Point(0, 0);
        private Point pt_righthand = new Point(0, 0);


        System.Timers.Timer L_Shoot_t = new System.Timers.Timer(100);
        System.Timers.Timer R_Shoot_t = new System.Timers.Timer(100);

        private JointType[] bones = {
                                // Torso
                   /* JointType.Head, JointType.Neck, */
                    JointType.Neck, JointType.SpineShoulder,
                    JointType.SpineShoulder, JointType.SpineMid,
                    JointType.SpineMid, JointType.SpineBase,
                    JointType.SpineShoulder, JointType.ShoulderRight,
                    JointType.SpineShoulder, JointType.ShoulderLeft,
                    JointType.SpineBase, JointType.HipRight,
                    JointType.SpineBase, JointType.HipLeft,

                    // Right Arm
                    JointType.ShoulderRight, JointType.ElbowRight,
                    JointType.ElbowRight, JointType.WristRight,
                    JointType.WristRight, JointType.HandRight,
                   /* JointType.HandRight, JointType.HandTipRight,
                    JointType.WristRight, JointType.ThumbRight, */

                    // Left Arm
                    JointType.ShoulderLeft, JointType.ElbowLeft,
                    JointType.ElbowLeft, JointType.WristLeft,
                    JointType.WristLeft, JointType.HandLeft,
                    /*JointType.HandLeft, JointType.HandTipLeft,
                    JointType.WristLeft, JointType.ThumbLeft,*/

                    // Right Leg
                    JointType.HipRight, JointType.KneeRight,
                    JointType.KneeRight, JointType.AnkleRight,
                    JointType.AnkleRight, JointType.FootRight,
                
                    // Left Leg
                    JointType.HipLeft, JointType.KneeLeft,
                    JointType.KneeLeft, JointType.AnkleLeft,
                    JointType.AnkleLeft, JointType.FootLeft,
        };

        private double jointSize = 5;
        private double boneThickness = 10;

        private Boolean showHandStates = false;

        public void ShowHandStates(Boolean show = true)
        {
            showHandStates = show;
        }

        public void Init(KinectSensor s, Image wpfImageForDisplay, Boolean toColorSpace = true)
        {
            sensor = s;

            if (toColorSpace) // map the skeleton to the color space
            {
                drawingImgWidth = sensor.ColorFrameSource.FrameDescription.Width;
                drawingImgHeight = sensor.ColorFrameSource.FrameDescription.Height;
            }
            else // map the skeleton to the depth space 
            {
                drawingImgWidth = sensor.DepthFrameSource.FrameDescription.Width;
                drawingImgHeight = sensor.DepthFrameSource.FrameDescription.Height;
            }
            DrawingGroupInit(wpfImageForDisplay);

            mapToColorSpace = toColorSpace;

            BodyFrameReaderInit();
            virusInit();
            spraysInit();
            timerInit();
            ShowHandStates();
        }


        public Point MapCameraPointToScreenSpace(Body body, JointType jointType) //codes referenced from T8_PoseMatching
        {
            Point screenPt = new Point(0, 0);
            if (mapToColorSpace) // to color space 
            {
                ColorSpacePoint pt = sensor.CoordinateMapper.MapCameraPointToColorSpace(
                body.Joints[jointType].Position);
                screenPt.X = pt.X;
                screenPt.Y = pt.Y;
            }
            else // to depth space
            {
                DepthSpacePoint pt = sensor.CoordinateMapper.MapCameraPointToDepthSpace(
                    body.Joints[jointType].Position);
                screenPt.X = pt.X;
                screenPt.Y = pt.Y;
            }
            return screenPt;
        }

        private void BodyFrameReaderInit() //codes referenced from T8_PoseMatching
        {
            BodyFrameReader bodyFrameReader = sensor.BodyFrameSource.OpenReader();
            bodyFrameReader.FrameArrived += BodyFrameReader_FrameArrived; 

            // BodyCount: maximum number of bodies that can be tracked at one time
            bodies = new Body[sensor.BodyFrameSource.BodyCount];
        }

        private void BodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame == null) return;

                bodyFrame.GetAndRefreshBodyData(bodies);

                if(IsStart == false)
                {
                    IsStart = true;
                }

                updateLv();
                
                using (DrawingContext dc = drawingGroup.Open())
                {
                    // draw a transparent background to set the render size
                    dc.DrawRectangle(Brushes.Transparent, null,
                            new Rect(0.0, 0.0, drawingImgWidth, drawingImgHeight));

                    
                    drawBullets(dc);
                    updateBullets();

                    foreach (Body body in bodies)
                    {
                        if (body.IsTracked)
                        {
                            // draw a skeleton
                            DrawSkeleton(body, dc);
                            
                            if (IsLose == false)
                            {
                                heart.HeartInit(dc, MapCameraPointToScreenSpace(body, JointType.SpineMid));
                                IsLose = heart.VirusIntersectHeart(virus);
                            }
                            else
                            {
                                heart.reset();
                            }

                            drawVirus(dc, body);
                            handMovingDir(body);
                            checkFreeze();
                            drawSprays(body, dc);
                            shoot(body, dc);

                            showText(dc, body);
                        }
                    }

                    if(heart.hp <= 0)
                    {
                        IsLose = true;
                    }
                    CheckIf_endGame();
                }
            }
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

        private void DrawSkeleton(Body body, DrawingContext dc) //codes referenced from T8_PoseMatching
        {
            for (int i = 0; i < bones.Length; i += 2)
            {
                DrawBone(body, dc, bones[i], bones[i + 1]);
            }

            foreach (JointType bone in bones)
            {
                Point pt = MapCameraPointToScreenSpace(body, bone);
                dc.DrawEllipse(Brushes.LightGreen, null, pt, jointSize, jointSize);
            }

            if (showHandStates)
            {
                // Visualize the hand tracking states             
                VisualizeHandState(body, dc, JointType.HandLeft, body.HandLeftState, -1);
                VisualizeHandState(body, dc, JointType.HandRight, body.HandRightState, 1);
            }
        }

        private void DrawBone(Body body, DrawingContext dc, JointType j0, JointType j1) //codes referenced from T8_PoseMatching
        {
            Point pt0 = MapCameraPointToScreenSpace(body, j0);
            Point pt1 = MapCameraPointToScreenSpace(body, j1);

            dc.DrawLine(new Pen(Brushes.DarkGreen, boneThickness), pt0, pt1);
        }

        private void VisualizeHandState(Body body, DrawingContext dc, JointType jointType, HandState handState,int handPos) //codes referenced from T8_PoseMatching
        {
            SolidColorBrush green = new SolidColorBrush(Color.FromArgb(100, 0, 255, 0));
            SolidColorBrush red = new SolidColorBrush(Color.FromArgb(100, 255, 0, 0));
            SolidColorBrush blue = new SolidColorBrush(Color.FromArgb(100, 0, 0, 255));
            double radius = 80;
            Point hand_pt = MapCameraPointToScreenSpace(body, jointType);
            switch (handState)
            {
                case HandState.Closed:
                   // dc.DrawEllipse(red, null, hand_pt, radius, radius);
                    break;
                case HandState.Open:
                    dc.DrawEllipse(blue, null, hand_pt, radius, radius);
                    // dc.DrawEllipse(green, null, hand_pt, radius, radius);
                    break;
                case HandState.Lasso:
                    //dc.DrawEllipse(blue, null, hand_pt, radius, radius);
                    break;
            }
        }

        // calculate the rotation angle (wrt the positive x-axis; in degree) given a vector dir 
        private double Dir2Angle(Vector dir)   //codes referenced from T7_AugmentedHuman
        {
            return Math.Atan2(dir.Y, dir.X) / Math.PI * 180;
        }

        private void DrawRotatedImage(DrawingContext dc, ImageSource imgSrc, Point imgCenter, double imgW, double imgH, double angle)  //codes referenced from T7_AugmentedHuman
        {
            // top-left position
            TranslateTransform translation = new TranslateTransform(imgCenter.X - imgW / 2, imgCenter.Y - imgH / 2);
            dc.PushTransform(translation); // apply translation 
            {
                // rotate with respect to image center 
                RotateTransform rotation = new RotateTransform(angle, imgW / 2, imgH / 2);
                dc.PushTransform(rotation); // apply rotation 
                Rect r = new Rect(0, 0, imgW, imgH);
                // no translation in r_gun (X = 0, Y = 0)
                dc.DrawImage(imgSrc, r);
                dc.Pop(); // reset rotation 
            }
            dc.Pop(); // reset translation
        }

        private void virusInit()
        {
            for (int y = 0; y < row; y++)
            {
                for (int x = 0; x < column; x++)
                {
                    virus[y * 10 + x] = new Virus();
                    virus[y * 10 + x].updateRegionSize();
                    virus[y * 10 + x].detectionRegion.X = x_interval * x + x_interval / 2 - virus[y * 10 + x].detectionRegion.Width/2;
                    virus[y * 10 + x].detectionRegion.Y = y_interval * y + y_interval / 2 - virus[y * 10 + x].detectionRegion.Height/2;
                }
            }
        }
        private void spraysInit()
        {
            for (int i = 0; i < sprays.Length; i++)
            {
                sprays[i] = new Spray();
                sprays[i].loadImage(i);
            }
        }

        private void handMovingDir(Body body)
        {
            pt_lefthand = MapCameraPointToScreenSpace(body, JointType.HandLeft);
            Point left_pt1 = MapCameraPointToScreenSpace(body, JointType.ElbowLeft);
            Point left_pt2 = MapCameraPointToScreenSpace(body, JointType.WristLeft);
            Vector left_WE = new Vector(left_pt2.X - left_pt1.X, left_pt2.Y - left_pt1.Y);
            left_hand_div.X = left_WE.X;
            left_hand_div.Y = left_WE.Y;
            left_hand_div.Normalize();
            left_EW.X = left_pt1.X - left_pt2.X;
            left_EW.Y = left_pt1.Y - left_pt2.Y;

            pt_righthand = MapCameraPointToScreenSpace(body, JointType.HandRight);
            Point right_pt1 = MapCameraPointToScreenSpace(body, JointType.ElbowRight);
            Point right_pt2 = MapCameraPointToScreenSpace(body, JointType.WristRight);
            Vector right_WE = new Vector(right_pt2.X - right_pt1.X, right_pt2.Y - right_pt1.Y);
            right_hand_div.X = right_WE.X;
            right_hand_div.Y = right_WE.Y;
            right_hand_div.Normalize();
            right_EW.X = right_pt1.X - right_pt2.X;
            right_EW.Y = right_pt1.Y - right_pt2.Y;

        }

        private void drawSprays(Body body, DrawingContext dc)
        {
            Point pt_lefthand = MapCameraPointToScreenSpace(body, JointType.HandLeft);
            sprays[0].loadImage(0);
            double left_ang = Dir2Angle(left_EW);
            if (sprays[0].spray != null)
            {
                //dc.DrawLine(new Pen(Brushes.Black, 50), pt_lefthand, pt_lefthand + left_hand_div*500);
                DrawRotatedImage(dc, sprays[0].spray, pt_lefthand, sprays[0].spray.Width, sprays[0].spray.Height, left_ang);
            }

            Point pt_righthand = MapCameraPointToScreenSpace(body, JointType.HandRight);
            sprays[1].loadImage(1);
            double right_ang = Dir2Angle(right_EW) - 180;
            if (sprays[1].spray != null)
            {
                //dc.DrawLine(new Pen(Brushes.Black, 50), pt_righthand, pt_righthand + right_hand_div * 500);
                DrawRotatedImage(dc, sprays[1].spray, pt_righthand, sprays[1].spray.Width, sprays[1].spray.Height, right_ang);
            }
        }

        private Boolean isShooting_left = false;
        private Boolean isShooting_right = false;

        private void shoot(Body body, DrawingContext dc)
        {
            if(sprays[0].IsFreezed == false)
            {
                //spray 0
                
                if (body.HandLeftState == HandState.Open && isShooting_left == false)
                {
                    L_Shoot_t.Enabled = true;
                    isShooting_left = true;
                }
                if (body.HandLeftState != HandState.Open)
                {
                    L_Shoot_t.Enabled = false;
                    isShooting_left = false;
                }
            }
            
            if(sprays[1].IsFreezed == false)
            {
                //spray 1
                
                if (body.HandRightState == HandState.Open && isShooting_right == false)
                {
                    R_Shoot_t.Enabled = true;
                    isShooting_right = true;
                }
                if (body.HandRightState != HandState.Open)
                {
                    R_Shoot_t.Enabled = false;
                    isShooting_right = false;
                }
            }
            
        }

        private void updateBullets()
        {
            sprays[0].VirusIntersectBullet(virus);
            sprays[1].VirusIntersectBullet(virus);

            sprays[0].updateBullet(drawingImgWidth, drawingImgHeight);
            sprays[1].updateBullet(drawingImgWidth, drawingImgHeight);
        }

        private void drawBullets(DrawingContext dc)
        {
            sprays[0].drawBullet(dc);
            sprays[1].drawBullet(dc);
        }

        private void updateLv()
        {
            NumOfKilledVirus = 0;
            for(int i = 0; i < virus.Length; i++)
            {
                NumOfKilledVirus += virus[i].killed;
            }

            if(NumOfKilledVirus < 5)
            {
                gameLevel = 0;
                GeneratePerSec = 1;
            } else if (NumOfKilledVirus >= 5 && NumOfKilledVirus < 15)
            {
                gameLevel = 1;
                GeneratePerSec = 2;
            } else if(NumOfKilledVirus >= 15 && NumOfKilledVirus < 30)
            {
                gameLevel = 2;
                GeneratePerSec = 3;
            } else if(NumOfKilledVirus >= 30 && NumOfKilledVirus < 60)
            {
                gameLevel = 3;
                GeneratePerSec = 5;
            } else if(NumOfKilledVirus >= 60 && NumOfKilledVirus < 100)
            {
                gameLevel = 4;
                GeneratePerSec = 7;
            } else if(NumOfKilledVirus >= 100)
            {
                IsWin = true;
            }
        }

        private void drawVirus(DrawingContext dc, Body body)
        {
            for (int y = 0; y < row; y++)
            {
                for (int x = 0; x < column; x++)
                {
                    if(virus[10 * y + x].isGenerated)
                    {
                        //dc.DrawRectangle(Brushes.Green, new Pen(Brushes.Green, 10), virus[10 * y + x].detectionRegion);
                        virus[10 * y + x].Loadimage();
                        virus[10 * y + x].drawVirus(dc);
                        ability(dc, virus[10 * y + x], MapCameraPointToScreenSpace(body, JointType.HandLeft), MapCameraPointToScreenSpace(body, JointType.HandRight));
                        virus[10 * y + x].updateState();
                        virus[10 * y + x].updateRegionSize();
                        virus[y * 10 + x].detectionRegion.X = x_interval * x + x_interval / 2 - virus[y * 10 + x].detectionRegion.Width/2;
                        virus[y * 10 + x].detectionRegion.Y = y_interval * y + y_interval / 2 - virus[y * 10 + x].detectionRegion.Height/2;
                       // DrawText(dc, virus[10 * y + x].color, new Point(x_interval * x + x_interval / 2 - virus[y * 10 + x].detectionRegion.Width / 2, y_interval * y + y_interval / 2 - virus[y * 10 + x].detectionRegion.Height / 2), 30, new Typeface("Calibri"), Brushes.Black);
                    }
                   
                }
            }          
        }

        private void DrawText(DrawingContext dc, String text, Point position, double size,         //T8_PoseMatching.T8_PositionBasedMatching
        Typeface typeFace, Brush color)
        {
            FormattedText formattedText = new FormattedText(text,
                System.Globalization.CultureInfo.GetCultureInfo("en-us"),
            FlowDirection.LeftToRight, typeFace, size, color);

            dc.DrawText(formattedText, position);
        }

        private void showText(DrawingContext dc,Body body)
        {
            Point pt_head = MapCameraPointToScreenSpace(body, JointType.Head);
            pt_head.X -= pt_head.X*0.08;
            String level = "Level " + gameLevel;
            DrawText(dc, level, pt_head, 64, new Typeface("Calibri"), Brushes.Black);

            //String KillNum = "Number of Virus killed " + NumOfKilledVirus;
            //DrawText(dc, KillNum, new Point(drawingImgWidth / 2, 64), 64, new Typeface("Calibri"), Brushes.Black);

            Point pt_lefthand = MapCameraPointToScreenSpace(body, JointType.HandLeft);
            Point pt_righthand = MapCameraPointToScreenSpace(body, JointType.HandRight);
            String Left_bulletLeft = sprays[0].availableBullet +"/"+ sprays[0].bullet.Length;
            String Right_bulletLeft = sprays[1].availableBullet + "/" + sprays[1].bullet.Length;
            DrawText(dc, Left_bulletLeft, new Point(pt_lefthand.X + pt_lefthand.X*0.1, pt_lefthand.Y), 64, new Typeface("Calibri"), Brushes.Black);
            DrawText(dc, Right_bulletLeft, new Point(pt_righthand.X - pt_righthand.X*0.1, pt_righthand.Y), 64, new Typeface("Calibri"), Brushes.Black);
        }

        private void virusGeneration()   //respawn a customized no. of virus in customized order per second
        {
            int generatedVirus = 0;
            for (int iter = 0; iter < row / 2; iter++)
            {
                if (generatedVirus >= GeneratePerSec)
                {
                    break;
                }

                for (int x = iter; x < 9 - iter; x++)
                {
                    if (generatedVirus >= GeneratePerSec)
                    {
                        break;
                    }
                    if (virus[iter * 10 + x].isGenerated == false)
                    {
                        virus[iter * 10 + x].color = ColorSelector();
                        virus[iter * 10 + x].AbilityReset();
                        virus[iter * 10 + x].isGenerated = true;
                        generatedVirus++;
                    }        
                }

                for(int y = iter; y < 9 - iter; y++)
                {
                    if (generatedVirus >= GeneratePerSec)
                    {
                        break;
                    }
                    if (virus[y * 10 + 9 - iter].isGenerated == false)
                    {
                        virus[y * 10 + 9 - iter].color = ColorSelector();
                        virus[y * 10 + 9 - iter].AbilityReset();
                        virus[y * 10 + 9 - iter].isGenerated = true;
                        generatedVirus++;
                    }
                }

                for(int x = 9 - iter; x > iter; x--)
                {
                    if (generatedVirus >= GeneratePerSec)
                    {
                        break;
                    }
                    if (virus[(9 - iter) * 10 + x].isGenerated == false)
                    {
                        virus[(9 - iter) * 10 + x].color = ColorSelector();
                        virus[(9 - iter) * 10 + x].AbilityReset();
                        virus[(9 - iter) * 10 + x].isGenerated = true;
                        generatedVirus++;
                    }
                }

                for(int y = 9 - iter; y > iter; y--)
                {
                    if (generatedVirus >= GeneratePerSec)
                    {
                        break;
                    }
                    if (virus[y * 10 + iter].isGenerated == false)
                    {
                        virus[y * 10 + iter].color = ColorSelector();
                        virus[y * 10 + iter].AbilityReset();
                        virus[y * 10 + iter].isGenerated = true;
                        generatedVirus++;
                    }
                }
            }
        }

        private void timerInit()
        {
            VirusGenTimer.Interval = new TimeSpan(0, 0, 0, 3);
            VirusGenTimer.Tick += VirusGenTimer_Tick;
            VirusGenTimer.Start();

            
            L_Shoot_t.Elapsed += new System.Timers.ElapsedEventHandler(L_Shoot);
            L_Shoot_t.AutoReset = true;
            L_Shoot_t.Enabled = false;

            
            R_Shoot_t.Elapsed += new System.Timers.ElapsedEventHandler(R_Shoot);
            R_Shoot_t.AutoReset = true;
            R_Shoot_t.Enabled = false;
        }

        private void VirusGenTimer_Tick(object sender, EventArgs e)
        {
            if (virus != null && IsStart)
            {
                virusGeneration();
            }
        }

        private void L_Shoot(object source, System.Timers.ElapsedEventArgs e)
        {
            if (sprays[0].IsFreezed == false)
            {
                sprays[0].ShootAvailableBullet(pt_lefthand, left_hand_div);
            }
            
        }
        private void R_Shoot(object source, System.Timers.ElapsedEventArgs e)
        {
            if(sprays[1].IsFreezed == false)
            {
               sprays[1].ShootAvailableBullet(pt_righthand, right_hand_div);
            }
            
        }

        private void ability(DrawingContext dc, Virus virus, Point left, Point right)
        {
            if(virus.color == "red")
            {
                virus.Emitlaser(dc, drawingImgWidth, heart);
                    
            }
            if(virus.color == "blue")
            {   
                virus.SpreadIce(dc, sprays, left, right);
            }
        }

        private void checkFreeze()
        { 
            if (sprays[0].IsFreezed && left_t_activated == false)
            {
                left_t_activated = true;
                System.Timers.Timer Left_t = new System.Timers.Timer(3000);
                Left_t.Elapsed += new System.Timers.ElapsedEventHandler(left_endFreeze);
                Left_t.AutoReset = false;
                Left_t.Enabled = true;
            }
            if (sprays[1].IsFreezed && right_t_activated == false) 
            {
                right_t_activated = true;
                System.Timers.Timer right_t = new System.Timers.Timer(3000);
                right_t.Elapsed += new System.Timers.ElapsedEventHandler(right_endFreeze);
                right_t.AutoReset = false;
                right_t.Enabled = true;
            }

        }
        private void left_endFreeze(object source, System.Timers.ElapsedEventArgs e)
        {
            sprays[0].IsFreezed = false;
            left_t_activated = false;
        }

        private void right_endFreeze(object source, System.Timers.ElapsedEventArgs e)
        {
            sprays[1].IsFreezed = false;
            right_t_activated = false;
        }

        private void messageBox(string s)
        {
            MessageBoxResult option = MessageBox.Show(s+"\n"+"Would you like to retart the game?", "Message", MessageBoxButton.YesNo);
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

        private Random rand = new Random();
        private string ColorSelector()
        {
            double prob = rand.NextDouble();
            if (prob < 0.6)
            {
                return "green";
            }
            if(prob >= 0.6 && prob < 0.8)
            {
                return "red";
            }

            return "blue";
        }

        private void CheckIf_endGame()
        {
            if (IsLose || IsWin)
            {
                if (IsLose)
                {
                    messageBox("You Lose.");
                }
                else if (IsWin)
                {
                    messageBox("You win.");
                }
                VirusGenTimer.Stop();
                L_Shoot_t.Close();
                R_Shoot_t.Close();
            }
        }

    }
}
