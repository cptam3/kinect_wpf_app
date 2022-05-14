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
    class ProgressBar
    {
        public Rect region;
        public Rect progress_bar;
        public int maxDist = 2000;
        public double max_w;
        public double progress = 0;

        public Player_bar player_bar;
        public Boss_bar boss_bar;
        public bool isCaught = false;
        public bool isReach = false;

        public ProgressBar(double x,double y, double w, double h)
        {
            int gap = 10;
            max_w = w - gap * 2;
            region = new Rect(x, y, w, h);
            progress_bar = new Rect(x + gap, y + gap, progress*max_w, h - gap * 2);

            player_bar = new Player_bar(x + gap + progress_bar.Width / 2, y + gap + progress_bar.Height / 2);
            boss_bar = new Boss_bar(x + gap + progress_bar.Width/2, y + gap + progress_bar.Height / 2);
        }

        public void update_player_run()
        {
            player_bar.run_dist += player_bar.run_speed;

            if (player_bar.run_dist >= maxDist)
            {
                player_bar.run_dist = maxDist;
                isReach = true;
            }
            progress = player_bar.run_dist / (double)maxDist;
            progress_bar.Width = progress * max_w;
            player_bar.pos.X = progress_bar.X + progress_bar.Width;
        }

        public void update_boss_run()
        {
            boss_bar.run_dist += boss_bar.run_speed;
            if (boss_bar.run_dist >= maxDist)
            {
                boss_bar.run_dist = maxDist;
            }
            boss_bar.pos.X = progress_bar.X + boss_bar.run_dist / (double)maxDist * max_w - boss_bar.pos.Width/2;
        }

        public void boss_player()
        {
            if (player_bar.run_dist <= boss_bar.run_dist)
            {
                isCaught = true;
                player_bar.run_speed = 0;
                boss_bar.run_speed = 0;
            }
        }

        public void draw(DrawingContext dc)
        {
            dc.DrawRectangle(null, new Pen(Brushes.Blue, 2), region);
            dc.DrawRectangle(Brushes.Red, null, progress_bar);

            player_bar.draw(dc);
            boss_bar.draw(dc);
        }
    }

    class Player_bar
    {
        private BitmapImage img;
        public Rect pos;
        public double run_speed = 1.25;
        public double run_dist = 20;

        public Player_bar(double x, double y)
        {
            pos = new Rect(x, y, 15, 25);
            pos.X -= pos.Width / 2;
            pos.Y -= pos.Height / 2;
            img = new BitmapImage(
                new Uri("img/boy_run_flip.png", UriKind.Relative));
        }

        public void draw(DrawingContext dc)
        {
            dc.DrawImage(img, pos);
        }
    }
    class Boss_bar
    {
        private BitmapImage img;
        public Rect pos;
        public double run_speed = 1;
        public double run_dist = 0;

        public Boss_bar(double x, double y)
        {
            pos = new Rect(x, y, 50, 50);
            pos.X -= pos.Width / 2*2;
            pos.Y -= pos.Height / 2;
            img = new BitmapImage(
                new Uri("img/boss_angry.png", UriKind.Relative));
        }
        public void draw(DrawingContext dc)
        {
            dc.DrawImage(img, pos);
        }

    }
}
