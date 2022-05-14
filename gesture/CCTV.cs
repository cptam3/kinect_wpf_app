using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NotCorporateSlave
{
    class CCTV
    {
        private BitmapImage img;
        public Rect pos;
        public Rect region;
        public double region_max_height;
        private List<Trails> trail_infor;
        public int numOftrail;
        public int onTrail;
        public int max_countdown = 100;
        public int countDown;
        public bool isActivated;
        public bool isWatch;

        private Random rand = new Random();

        public CCTV(List<Trails> trail_list, int numOfTrails, double w,double height)
        {
            trail_infor = trail_list;
            numOftrail = numOfTrails;
            onTrail = rand.Next(0, numOftrail);
            pos = new Rect(trail_infor[onTrail].region.X, trail_infor[onTrail].region.Y, w, w);
            region = new Rect(trail_infor[onTrail].region.X, trail_infor[onTrail].region.Y+pos.Height, w, 0);
            region_max_height = height-pos.Height;
            
            countDown = max_countdown;
            isActivated = false;
            isWatch = false;

            img = new BitmapImage(
                new Uri("img/cctv.png", UriKind.Relative));
        }
        public void reset()
        {
            isActivated = false;
            countDown = max_countdown;

            onTrail = rand.Next(0, numOftrail);
            pos.X = trail_infor[onTrail].region.X;
            pos.Y = trail_infor[onTrail].region.Y;
            region.X = trail_infor[onTrail].region.X;
            region.Y = trail_infor[onTrail].region.Y+pos.Height;
            region.Height = 0;

        }
        public void update()
        {
            if (isActivated)
            {
                countDown--;
                region.Height = ((double)max_countdown - (double)countDown) / (double)max_countdown * (double)region_max_height;
                if (countDown <= 0)
                {
                    reset();
                }
            }            
        }

        public void draw(DrawingContext dc)
        {
            if (isActivated)
            {
                dc.DrawImage(img, pos);
                dc.PushOpacity(0.5);
                dc.DrawRectangle(Brushes.Red, null, region);
                dc.Pop();
            }  
        }

    }
}
