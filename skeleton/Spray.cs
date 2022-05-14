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
    class Spray
    {
        public BitmapImage spray;
        public Bullet[] bullet = new Bullet[2];
        public int availableBullet = 2;
        public Boolean IsFreezed = false;

        public Spray()
        {
            for(int i = 0; i < bullet.Length; i++)
            {
                bullet[i] = new Bullet();
            }
        }

        public void loadImage(int hand)
        {
            if(hand == 0)
            {
                if (IsFreezed)
                {
                    spray = new BitmapImage(
                    new Uri("img/freeze.png", UriKind.Relative));
                }
                if(IsFreezed == false)
                {
                    spray = new BitmapImage(
                    new Uri("img/spray_left.png", UriKind.Relative));
                }
               
            }
            if(hand == 1)
            {
                if (IsFreezed)
                {
                    spray = new BitmapImage(
                    new Uri("img/freeze.png", UriKind.Relative));
                }
                if (IsFreezed == false)
                {
                    spray = new BitmapImage(
                    new Uri("img/spray_right.png", UriKind.Relative));
                }
            }
            
        }
        public void ShootAvailableBullet(Point shootingPoint, Vector div)
        {
            for(int i=0; i<bullet.Length; i++)
            {
                if (bullet[i].isShooted == false)
                {
                    bullet[i].isShooted = true;
                    bullet[i].location.X = shootingPoint.X;
                    bullet[i].location.Y = shootingPoint.Y;
                    bullet[i].setDir(div);
                    availableBullet--;
                    break;
                }
            }
        }

        public void VirusIntersectBullet(Virus[] virus)
        {
            for(int i=0; i<virus.Length; i++)
            {
                if (virus[i].isGenerated)
                {
                    for(int j=0; j<bullet.Length; j++)
                    {
                        if (virus[i].detectionRegion.Contains(bullet[j].location))
                        {
                            virus[i].isShooted();
                            bullet[j].reset();
                            bullet[j].isShooted = false;
                            availableBullet++;
                        }
                    }
                }
            }
        }

        public void updateBullet(double drawingImgWidth, double drawingImgHeight)
        {
            for(int i = 0; i<bullet.Length; i++)
            {

                if (bullet[i].isShooted)
                {
                    if (bullet[i].location.X < 0 || bullet[i].location.X > drawingImgWidth || bullet[i].location.Y < 0 || bullet[i].location.Y > drawingImgHeight)
                    {
                        bullet[i].reset();
                        bullet[i].isShooted = false;
                        availableBullet++;
                    }
                    else
                    {
                        bullet[i].location += bullet[i].moving_dir * 40;
                    }
                }
            }
        }
        public void drawBullet(DrawingContext dc)
        {
            for (int i = 0; i < bullet.Length; i++)
            {
                if (bullet[i].isShooted)
                {
                    dc.DrawLine(new Pen(Brushes.Blue, bullet[i].size), bullet[i].location, bullet[i].location + bullet[i].moving_dir * bullet[i].size);
                }
            }
        }

    }

    class Bullet
    {
        public int size = 30;
        public Boolean isShooted = false;
        public Point location = new Point(-10, 500);
        public Vector moving_dir = new Vector(0, 0);
        public void reset()
        {
            location.X = -10;
            location.Y = 500;
        }
        public void setDir(Vector div)
        {
            moving_dir = div;
        }

    }
}
