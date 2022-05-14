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
    class Trails
    {
        private BitmapImage img;
        public Rect region;
        public int index;

        public Trails(int i,double x, double y, double w, double h)
        {
            index = i;

            region = new Rect(x, y, w, h);
            region.X -= region.Width / 2;
            
            img = new BitmapImage(
                new Uri("img/trail.png", UriKind.Relative));
        }

        public void draw(DrawingContext dc)
        {
            dc.DrawImage(img, region);
        }
    }
}
