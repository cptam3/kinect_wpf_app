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
    class Players
    {
        public int health = 5;
        public bool isExhausted = false;
        public int onTrail = 0;
        private BitmapImage img;
        public Rect pos;
        public bool isRun = false; 
        public HealthBar healthBar;
        public Players(double x, double y, double w,double health_x, double health_y, double health_w)
        {
            health = 5;
            pos = new Rect(x, y, w, w*1.8);
            pos.X -= pos.Width / 2;
            pos.Y -= pos.Height;
            img = new BitmapImage(
                new Uri("img/boy_stand.png", UriKind.Relative));

            healthBar = new HealthBar(health, health_x, health_y, health_w);
        }

        public void draw(DrawingContext dc)
        {
            dc.DrawImage(img, pos);
        }

        public void move(int mov_div,int numOfTrail, List<Trails> trail_list)
        {
            onTrail += mov_div;
            if (onTrail < 0)
            {
                onTrail = 0;
            }
            if (onTrail > numOfTrail - 1)
            {
                onTrail = numOfTrail - 1;
            }
            Rect nextTrail = trail_list[onTrail].region;
            pos.X = nextTrail.X + nextTrail.Width / 2 - pos.Width / 2;
        }

        public void changeState(bool isRunning)
        {
            isRun = isRunning;
            if(isRunning)
            {
                img = new BitmapImage(
                 new Uri("img/boy_run.png", UriKind.Relative));
            } else
            {
                img = new BitmapImage(
                new Uri("img/boy_stand.png", UriKind.Relative));
            }
        }

        public bool Document_Player(Rect doc)
        {
            if (pos.IntersectsWith(doc))
            {
                health--;
                if (health > 0)
                {
                    healthBar.reduceHeart();
                } else
                {
                    health = 0;
                    isExhausted = true;
                }
                return true;
            }

            return false;
        }

        public bool Entertainment_Player(Rect Entertainment)
        {
            if (pos.IntersectsWith(Entertainment))
            {
                return true;
            }
            return false;
        }

        public bool CCTV_Player(Rect CCTV)
        {
            if (pos.IntersectsWith(CCTV))
            {
                return true;
            }
            return false;
        }
    }
    class HealthBar
    {
        private BitmapImage img;
        private List<Rect> pos;
        private double heart_w;
        public HealthBar(int hp, double x, double y, double w)
        {
            heart_w = w;
            int gap = 5;
            pos = new List<Rect>();
            for (int i = 0; i < hp; i++)
            {
                pos.Add(new Rect(x, y + i * (heart_w + gap), heart_w, heart_w));
            }

            img = new BitmapImage(
                new Uri("img/heart.png", UriKind.Relative));
        }
        public void draw(DrawingContext dc)
        {
            for (int i = 0; i < pos.Count; i++)
            {
                dc.DrawImage(img, pos[i]);
            }
        }

        public void reduceHeart()
        {
            pos.RemoveAt(pos.Count - 1);
        }
    }
}
