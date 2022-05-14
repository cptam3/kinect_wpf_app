using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KillVirus
{
    class Virus
    {
        private BitmapImage virus;
        private BitmapImage iceCircle;

        private float size = 45;
        private float max_size = 90;
        public string color = "green";
        
        public Rect detectionRegion = new Rect(0, 0, 0, 0);
        private Rect right_laser = new Rect(0, 0, 0, 0);
        private Rect left_laser = new Rect(0, 0, 0, 0);
        private Rect up_laser = new Rect(0, 0, 0, 0);
        private Rect down_laser = new Rect(0, 0, 0, 0);
        private Rect iceArea = new Rect(0, 0, 0, 0);

        public int killed = 0;

        public Boolean isGenerated = false;
        private Boolean isHiting = false;

        public Virus()
        {
            virus = new BitmapImage(
               new Uri("img/virus_green.png", UriKind.Relative));
            iceCircle = new BitmapImage(
               new Uri("img/iceCircle.png", UriKind.Relative));
        }
        public void updateRegionSize()
        {
            detectionRegion.Height = size;
            detectionRegion.Width = size;
        }

        public void Loadimage()
        {
            if(color == "green")
            {
                virus = new BitmapImage(
                new Uri("img/virus_green.png", UriKind.Relative));
            }
            if (color == "red")
            {
                virus = new BitmapImage(
                new Uri("img/virus_red.png", UriKind.Relative));
            }
            if (color == "blue")
            {
                virus = new BitmapImage(
                new Uri("img/virus_blue.png", UriKind.Relative));
            }
        }

        public void drawVirus(DrawingContext dc)
        {
            dc.DrawImage(virus, detectionRegion);
        }

        public void updateState()
        {
            if(isGenerated)
            {
                if (size < 10)
                {
                    size = 45;
                    killed++;
                    isGenerated = false;   
                } else if(size >= 10  && size < max_size)
                {
                    size += 1;
                } else if(size >= max_size)
                {
                    size = max_size;
                }
            }   
        }

        public void isShooted()
        {
            size -= max_size / 2;
            if (size < 0)
            {
                size = 0;
            }
        }

        public void Emitlaser(DrawingContext dc, double width, Heart heart)
        {

            left_laser.Height = 5;
            left_laser.Width += 10;
            left_laser.X = detectionRegion.X + size / 2 - left_laser.Width - detectionRegion.Width/2;
            left_laser.Y = detectionRegion.Y + size / 2 - right_laser.Height / 2;
            if (left_laser.Width >= 300)
            {
                left_laser.X = detectionRegion.X + size / 2;
                left_laser.Width = 0;
            }
            dc.DrawRectangle(Brushes.Red, new Pen(Brushes.Red, 1), left_laser);
            
            right_laser.Height = 5;
            right_laser.Width += 10;
            right_laser.X = detectionRegion.X + size / 2 + detectionRegion.Width/2;
            right_laser.Y = detectionRegion.Y + size / 2;
            if (right_laser.Width >= 300)
            {
                right_laser.Width = 0;
            }
            dc.DrawRectangle(Brushes.Red, new Pen(Brushes.Red, 1), right_laser);

            up_laser.Width = 5;
            up_laser.Height += 10;
            up_laser.X = detectionRegion.X + size / 2 - up_laser.Width / 2;
            up_laser.Y = detectionRegion.Y + size / 2 - up_laser.Height - detectionRegion.Height/2;
            if(up_laser.Height >= 300)
            {
                up_laser.Y = detectionRegion.Y + size / 2;
                up_laser.Height = 0;
            }
            dc.DrawRectangle(Brushes.Red, new Pen(Brushes.Red, 1), up_laser);

            down_laser.Width = 5;
            down_laser.Height += 10;
            down_laser.X = detectionRegion.X + size / 2 - up_laser.Width / 2;
            down_laser.Y = detectionRegion.Y + size / 2 + detectionRegion.Height / 2;
            if (down_laser.Height >= 300)
            {
                down_laser.Y = detectionRegion.Y + size / 2;
                down_laser.Height = 0;
            }
            dc.DrawRectangle(Brushes.Red, new Pen(Brushes.Red, 0), down_laser);

            laserIntersectHeart(heart);
        }

        public void laserIntersectHeart(Heart heart)
        {
            
            if ( (up_laser.IntersectsWith(heart.detectionRegion) || down_laser.IntersectsWith(heart.detectionRegion) || left_laser.IntersectsWith(heart.detectionRegion) || right_laser.IntersectsWith(heart.detectionRegion) ) && isHiting == false){
                heart.hp--;
                isHiting = true;
            }
            if(up_laser.IntersectsWith(heart.detectionRegion) == false && down_laser.IntersectsWith(heart.detectionRegion) == false && left_laser.IntersectsWith(heart.detectionRegion) == false && right_laser.IntersectsWith(heart.detectionRegion) == false)
            {
                isHiting = false;
            }
        }

        public void SpreadIce(DrawingContext dc, Spray[] sprays, Point left, Point right)
        {
            iceArea.X = detectionRegion.X+size/2 - iceArea.Width/2;
            iceArea.Y = detectionRegion.Y+size/2 - iceArea.Height/2;
            iceArea.Height += 2;
            iceArea.Width += 2;
            if(iceArea.Height > 500)
            {
                iceArea.Height = 0;
                iceArea.Width = 0;
            }
            //dc.DrawRectangle(Brushes.Transparent, new Pen(Brushes.LightBlue, 5), iceArea);
            dc.DrawImage(iceCircle, iceArea);
            IceIntersectSpray(left, right, sprays);
        }


        public void IceIntersectSpray(Point left, Point right, Spray[] sprays)
        {
            if (iceArea.Contains(left))
            {
                sprays[0].IsFreezed = true;
                
            }
            if (iceArea.Contains(right))
            {
                sprays[1].IsFreezed = true;
            }
        }


        public void AbilityReset()
        {
            iceArea.Height = 0;
            iceArea.Width = 0;
            left_laser.Height = 0;
            left_laser.Width = 0;
            right_laser.Height = 0;
            right_laser.Width = 0;
        }
    }
}
