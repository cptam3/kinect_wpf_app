using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Drawing;

using Microsoft.Kinect;
using System.Windows.Media;
using System.Windows;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;

namespace CatchTheTarget
{
    class MainGame
    {
        private KinectSensor sensor;
        private System.Windows.Controls.Image GameScreen = null;

        private FrameDescription depthFrameDescription = null;
        private int bytesPerPixel = 4;
        private byte[] depthPixels = null;
        private WriteableBitmap depthImageBitmap = null;

        private Image<Bgr, byte> openCVImg = null;

        private ushort[] depthData = null;
        private byte[] bodyData = null;

        public float minDistance = 750;
        public float maxDistance = 1600;
        public float minDetectL = 10;
        public float maxDetectL = 100;

        private Random rand = new Random();

        private static int NumOfCircle = 9;       //default num is 9
        private static int maxNumOfCircle = 20;  //number of circle is [4,20]
        private CircleF[] circle = new CircleF[maxNumOfCircle];  //max num is 20
        private Rectangle[] CircleDetectionRegion = new Rectangle[maxNumOfCircle];

        private int[] circleDepth = new int[maxNumOfCircle];
        private int row = 0;
        private int column = 0;

        private int[] nearToCir = new int[maxNumOfCircle];

        private int Target = 0;

        public float baseSpeed = 0.5F;
        private float[] circleSpeed = new float[maxNumOfCircle];
        private int[] circleSpeedAngle = new int[maxNumOfCircle];
        private float[] circleMoveX = new float[maxNumOfCircle];
        private float[] circleMoveY = new float[maxNumOfCircle];

        private System.Drawing.Color c = System.Drawing.Color.DodgerBlue;
        private System.Drawing.Color Targetc = System.Drawing.Color.OrangeRed;
        private System.Drawing.Color LittleDangerc = System.Drawing.Color.Blue;
        private System.Drawing.Color LittleDangerTargetc = System.Drawing.Color.Red;
        private System.Drawing.Color Dangerc = System.Drawing.Color.DarkBlue;
        private System.Drawing.Color DangerTargetc = System.Drawing.Color.DarkRed;
        
        public bool IsInit = false;
        public bool IsMove = false;

        private int score = 0;
        public void Init(KinectSensor s, System.Windows.Controls.Image wpfImageForDisplay)
        {
            sensor = s;
            GameScreen = wpfImageForDisplay;
            createCir();
            BodyImageInit();
            DepthImageInit();

            
        }

        private void DepthImageInit()
        {
            DepthFrameReader depthFrameReader = sensor.DepthFrameSource.OpenReader();
            depthFrameReader.FrameArrived += DepthFrameReader_FrameArrived;

            depthFrameDescription = sensor.DepthFrameSource.FrameDescription;
            depthData = new ushort[depthFrameDescription.LengthInPixels];
           
            depthPixels = new byte[depthFrameDescription.LengthInPixels * bytesPerPixel];
            depthImageBitmap = new WriteableBitmap(
                                       depthFrameDescription.Width, // 512 
                                       depthFrameDescription.Height, // 424
                                       96, 96, PixelFormats.Bgr32, null);

            GameScreen.Source = depthImageBitmap;

        }

        private void DepthFrameReader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {

            using (DepthFrame depthFrame = e.FrameReference.AcquireFrame())
            {
                if (depthFrame == null) return;

                depthFrame.CopyFrameDataToArray(depthData);

                TargetDetection(depthData,bodyData);

            }
        }

        private void BodyImageInit()  
        {
            
            BodyIndexFrameReader bodyIndexFrameReader = sensor.BodyIndexFrameSource.OpenReader();
            bodyIndexFrameReader.FrameArrived += BodyIndexFrameReader_FrameArrived;

            bodyData = new byte[sensor.DepthFrameSource.FrameDescription.LengthInPixels];
        }

        private void BodyIndexFrameReader_FrameArrived(object sender, BodyIndexFrameArrivedEventArgs e)
        {
            using (BodyIndexFrame bodyIndexFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyIndexFrame == null) return;

                bodyIndexFrame.CopyFrameDataToArray(bodyData);

            }
        }

        public System.Drawing.Bitmap ImageGeneration(ushort[] depthData, byte[] bodyData)
        {
            float d_body = 0;
            int num_of_player_pixels = 0;

            for (int i = 0; i < depthData.Length; ++i)
            {
                ushort depth = depthData[i];
                
                    if (bodyData[i] != 255)
                    {
                        num_of_player_pixels++;
                        d_body += depthData[i];
                    }
                
            }

            if (num_of_player_pixels != 0)
            {
                d_body /= num_of_player_pixels;
            }

            float offset = 250;

            for (int i = 0; i < depthData.Length; ++i)
            {
                ushort depth = depthData[i];

                
                    if (bodyData[i] != 255) // player
                    {
                        if (depth < d_body - offset)
                        {
                            depthPixels[4 * i] = 255;
                            depthPixels[4 * i + 1] = 255;
                            depthPixels[4 * i + 2] = 255;
                        }
                        else
                        {
                            depthPixels[4 * i] = 0;
                            depthPixels[4 * i + 1] = 0;
                            depthPixels[4 * i + 2] = 0;

                        }

                    }
                    else
                    {
                        depthPixels[4 * i] = 0;
                        depthPixels[4 * i + 1] = 0;
                        depthPixels[4 * i + 2] = 0;
                    }
                
            }

            depthImageBitmap.WritePixels(
                        new Int32Rect(0, 0, depthFrameDescription.Width, depthFrameDescription.Height),
                depthPixels, depthFrameDescription.Width * 4, 0);



            BitmapSource bmpSrc = BitmapSource.Create(depthFrameDescription.Width, depthFrameDescription.Height, 96, 96, PixelFormats.Bgr32, null,
                     depthPixels, depthFrameDescription.Width * 4);
            // BitmapSource -> Bitmap 
            return BitMap_BitMapSource.ToBitmap(bmpSrc);
        }

        public void CircleInit()
        {
            setNumOfCir();
            setCirPos();
            setCirDepth();
            setTarget();
            setNearToCir();
            setCircleDetectionRegion();
            setCircleMovement();
        }

        private void createCir()   //initalize the cricles
        { 
            for(int i=0; i<NumOfCircle; i++)
            {
                circle[i] = new CircleF(new System.Drawing.PointF(0,0),0);
                CircleDetectionRegion[i] = new Rectangle(new System.Drawing.Point(0, 0), new System.Drawing.Size(0, 0));
            }
        }

        private void setNumOfCir()    //set the number of cirlces
        {
            NumOfCircle = rand.Next(4, maxNumOfCircle);
        }

        private void setCirPos()   //set the position of circles according to the number of circle being draw
        {
            row = 1;
            column = 1;
            while (NumOfCircle > ((row+1) * (column+1)))
            {
                column++;
                if(NumOfCircle > ((row+1) * (column+1)))
                {
                    row++;
                }
                else
                {
                    break;
                }
            }

            for(int y = 0; y<=row; y++)
            {
                for(int x = 0; x<=column; x++)
                {
                    /*  circle[(column+1) * y + x].Center = new System.Drawing.PointF(
                          rand.Next((int)(openCVImg.Width/(column+1) * x), (int)(openCVImg.Width/(column+1) * (x+1)) )
                          ,rand.Next((int)(openCVImg.Height/(row+1) * y), (int)(openCVImg.Height/(row+1) * (y+1)) )
                          );  */ //potential error: overlapped circles
                    circle[(column + 1) * y + x].Center = new System.Drawing.PointF(
                          openCVImg.Width / (column + 1) * x + openCVImg.Width / (2 * column + 2),
                          openCVImg.Height / (row + 1) * y + openCVImg.Height / (2 * row + 2)
                          );
                }
            }
        }

        private void setCirDepth()    // set circles' depth & size
        {
            for (int i = 0; i<NumOfCircle; i++)
            {
                circleDepth[i] = rand.Next((int)minDistance, (int)maxDistance);    //depends on the customised minmaxDistance
                circle[i].Radius = (float)((circleDepth[i] - minDistance) / (maxDistance - minDistance) * (10-30) + 30);   //range of radius is [10,30]
            }
        }

        private void setTarget()   //set the target
        {
            Target = rand.Next(NumOfCircle-1);
        }

        private void setCircleDetectionRegion()    //set the circles' detection region
        {
            for(int i = 0; i<NumOfCircle; i++)
            {
                CircleDetectionRegion[i].Location = new System.Drawing.Point((int)(circle[i].Center.X - circle[i].Radius), (int)(circle[i].Center.Y - circle[i].Radius));
                CircleDetectionRegion[i].Size = new System.Drawing.Size((int)circle[i].Radius * 2, (int)circle[i].Radius * 2);
            }
        }

        private void setNearToCir()    // reset the all index to 0
        {
            for(int i = 0; i<maxNumOfCircle; i++)
            {
                nearToCir[i] = 0;
            }
        }

        private void setCircleMovement()
        {
            for(int i = 0; i<NumOfCircle; i++)
            {
                circleSpeed[i] = (float)(baseSpeed + rand.NextDouble());
                circleSpeedAngle[i] = rand.Next(360);
                circleMoveX[i] = (float)(circleSpeed[i] * Math.Cos((double)(circleSpeedAngle[i] * Math.PI / 180)));
                circleMoveY[i] = -(float)(circleSpeed[i] * Math.Sin((double)(circleSpeedAngle[i] * Math.PI / 180)));
            }
        }


        private void TargetDetection(ushort[] depthData, byte[] bodyData)
        {
            System.Drawing.Bitmap bmp = ImageGeneration(depthData, bodyData);

            openCVImg = bmp.ToImage<Bgr, byte>();
            Image<Gray, byte> grayImg = openCVImg.Convert<Gray, byte>();

            if(IsInit == false){   //initalize the circles once at the beginning
                CircleInit();
                IsInit = true;
            }

            setNearToCir();
            if (IsMove)
            {
                UpdateCirclePos();
            }

            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(grayImg, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                for (int i = 0; i < contours.Size; i++)
                {
                    double area = CvInvoke.ContourArea(contours[i]);
                    if (area > minDetectL* minDetectL && area < maxDetectL* maxDetectL)
                    {

                        System.Drawing.Rectangle DetectRegion = CvInvoke.BoundingRectangle(contours[i]);
                        RotatedRect rotatedRect = CvInvoke.MinAreaRect(contours[i]);
                        openCVImg.Draw(rotatedRect, new Bgr(System.Drawing.Color.Red), 2);

                        double depth = CalculateAverageDepth(grayImg, contours[i]);
                       /* String s = String.Format("{0:0}", depth/100);
                        openCVImg.Draw(s, new System.Drawing.Point(DetectRegion.X, DetectRegion.Y), new FontFace(), 0.5,
                            new Bgr(System.Drawing.Color.Yellow)); */

                        DetectItem(DetectRegion, circle, depth);
                            
                    }
                }
            }

            drawCirlces();
            drawUI();

            bmp = openCVImg.ToBitmap<Bgr, byte>();                                       
            GameScreen.Source = BitMap_BitMapSource.ToBitmapSource(bmp);

        }

        private void drawCirlces()
        {
            for(int i = 0; i<NumOfCircle; i++)
            {
                //openCVImg.Draw(CircleDetectionRegion[i], new Bgr(c), 0);  //for debug use
                if (i == Target)
                {
                    if(nearToCir[i] == 0)
                    {
                        openCVImg.Draw(circle[i], new Bgr(Targetc), 0);
                       /* String s = String.Format("{0:0}", circleDepth[i]/100);
                        openCVImg.Draw(s, new System.Drawing.Point((int)circle[i].Center.X, (int)circle[i].Center.Y), new FontFace(), 0.4,
                            new Bgr(System.Drawing.Color.Yellow)); */

                    }
                    else if(nearToCir[i] == 1)
                    {
                        openCVImg.Draw(circle[i], new Bgr(DangerTargetc), 0);
                       /* String s = String.Format("{0:0}", circleDepth[i]/100);
                        openCVImg.Draw(s, new System.Drawing.Point((int)circle[i].Center.X, (int)circle[i].Center.Y), new FontFace(), 0.4,
                            new Bgr(System.Drawing.Color.Yellow)); */
                    }
                    else
                    {
                        openCVImg.Draw(circle[i], new Bgr(LittleDangerTargetc), 0);
                       /* String s = String.Format("{0:0}", circleDepth[i] / 100);
                        openCVImg.Draw(s, new System.Drawing.Point((int)circle[i].Center.X, (int)circle[i].Center.Y), new FontFace(), 0.4,
                            new Bgr(System.Drawing.Color.Yellow)); */
                    }
                    
                }
                else
                {
                    if(nearToCir[i] == 0) {
                        openCVImg.Draw(circle[i], new Bgr(c), 0);
                       /* String s = String.Format("{0:0}", circleDepth[i]/100);
                        openCVImg.Draw(s, new System.Drawing.Point((int)circle[i].Center.X, (int)circle[i].Center.Y), new FontFace(), 0.4,
                            new Bgr(System.Drawing.Color.Yellow));  */
                    }
                    else if(nearToCir[i] == 1)
                    {
                        openCVImg.Draw(circle[i], new Bgr(Dangerc), 0);
                       /* String s = String.Format("{0:0}", circleDepth[i]/100);
                        openCVImg.Draw(s, new System.Drawing.Point((int)circle[i].Center.X, (int)circle[i].Center.Y), new FontFace(), 0.4,
                            new Bgr(System.Drawing.Color.Yellow)); */
                    }
                    else
                    {
                        openCVImg.Draw(circle[i], new Bgr(LittleDangerc), 0);
                       /* String s = String.Format("{0:0}", circleDepth[i] / 100);
                        openCVImg.Draw(s, new System.Drawing.Point((int)circle[i].Center.X, (int)circle[i].Center.Y), new FontFace(), 0.4,
                            new Bgr(System.Drawing.Color.Yellow)); */
                    }
                    
                }
            }
        }

        private void drawUI()
        {
            openCVImg.Draw("Score " + score
                , new System.Drawing.Point(openCVImg.Width/2-40, 25), new FontFace(), 0.7, new Bgr(System.Drawing.Color.LightBlue));

           // debug();
        }

        private void DetectItem(System.Drawing.Rectangle DetectRegion, CircleF[] circle, double depth)
        {

            for (int i = 0; i<NumOfCircle; i++)
            {
                
                //DetectRegion.IntersectsWith
                if (DetectRegion.IntersectsWith(CircleDetectionRegion[i]))     //if the player touches the circles
                {
                    if (depth > circleDepth[i] - 400 && depth < circleDepth[i] + 400)
                    {
                        nearToCir[i] = 2;

                        if (depth > circleDepth[i] - 200 && depth < circleDepth[i] + 200)
                        {
                            nearToCir[i] = 1;

                            if (depth > circleDepth[i] - 100 && depth < circleDepth[i])     //if the player reach the depth of that circle
                            {
                                if (i == Target)  //get score if touching the target
                                {
                                    score++;
                                }
                                else
                                {
                                    if (score > 0)
                                    {
                                        score--;
                                    }

                                }
                                CircleInit();   //reset circles
                                break;          //call out once in each frame
                            }
                        }
                    }
                    
                }
                
            }
        }

        //Reference Code: T5_BodilyInteraction
        private float CalculateAverageDepth(Image<Gray, byte> binaryImg, IInputArray contour)
        {
            float avg_depth = 0;
            int count = 0;
            System.Drawing.Rectangle aabb = CvInvoke.BoundingRectangle(contour);
            for (int col = aabb.Left; col < aabb.Right; col++)
                for (int row = aabb.Top; row < aabb.Bottom; row++)
                {
                    byte pixel = binaryImg.Data[row, col, 0]; // get corresponding pixel 
                    if (pixel == 255) // white
                    {
                        avg_depth += depthData[row * depthFrameDescription.Width + col];
                        count++;
                    }
                }
            if (count != 0) return avg_depth / count;
            else return 0;
        }

        private void UpdateCirclePos()
        {
            for(int i = 0; i < NumOfCircle; i++)
            {
                float newX = circle[i].Center.X + circleMoveX[i];
                float newY = circle[i].Center.Y + circleMoveY[i];

                if (newX > openCVImg.Width || newX < 0)
                {
                    circleMoveX[i] = -circleMoveX[i];
                    newX += circleMoveX[i];
                }
                if(newY > openCVImg.Height || newY < 0)
                {
                    circleMoveY[i] = -circleMoveY[i];
                    newY += circleMoveY[i];
                }

                circle[i].Center = new System.Drawing.PointF(newX, newY);
                CircleDetectionRegion[i].Location = new System.Drawing.Point((int)(newX - circle[i].Radius), (int)(newY - circle[i].Radius));
            }
        }

            
               

        private void debug()
        {
            openCVImg.Draw("column is:" + column + " row is: " + row + " numOfCir is:" + NumOfCircle + " target:" + Target +
                " score is:" + score
                , new System.Drawing.Point(0, 50), new FontFace(), 0.4, new Bgr(System.Drawing.Color.Yellow));
        }
    }
    
}
