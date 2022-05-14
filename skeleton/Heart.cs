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
    class Heart
    {
        public BitmapImage heart;
        public Rect detectionRegion = new Rect(0, 0, 0, 0);
        public int hp = 5;
        public int size = 30;
        
        public Heart()
        {
            heart = new BitmapImage(
               new Uri("img/heart.png", UriKind.Relative));
        }

        public void HeartInit(DrawingContext dc,Point pt)
        {
            int final_size = size * hp;
            pt.X -= final_size / 2;
            pt.Y -= final_size / 2;
            detectionRegion.X = pt.X;
            detectionRegion.Y = pt.Y;
            detectionRegion.Width = final_size;
            detectionRegion.Height = final_size;
            dc.DrawImage(heart, detectionRegion);
            //dc.DrawRectangle(Brushes.Red, new Pen(Brushes.Red, 5), detectionRegion);
        }

        public bool VirusIntersectHeart(Virus[] virus)
        {
            for(int i = 0; i < virus.Length; i++)
            {
                if (virus[i].isGenerated)
                {
                    if (virus[i].detectionRegion.IntersectsWith(detectionRegion))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        public void reset()
        {
            detectionRegion.X = -500;
            detectionRegion.Y = -500;
        }
    }
}
