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
    class Document
    {
        private BitmapImage img;
        public Rect pos;
        public double speed = 5;
        private Random rand = new Random();
        public int onTrail = 0;
        public bool isOut = false;

        public Document(List<Trails> trail_list,int numOfTrails, double w)
        {
            onTrail = rand.Next(0, numOfTrails);
            pos = new Rect(trail_list[onTrail].region.X + trail_list[onTrail].region.Width/2, trail_list[onTrail].region.Y, w, w);
            pos.X -= pos.Width / 2;
            img = new BitmapImage(
                new Uri("img/document.png", UriKind.Relative));
        }

        public void update(double border)
        {
            pos.Y += speed;
            if (pos.Y >= border - pos.Height)
            {
                pos.Y = border - pos.Height;
                isOut = true;
            }
        }

        public void draw(DrawingContext dc)
        {
            if(isOut == false)
            {
                dc.DrawImage(img, pos);
            }
        }
    }
}
