using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;

namespace CatchTheTarget
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private KinectSensor sensor = null;
        MainGame mainGame = null;
        

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            System.Console.WriteLine("window loaded");

            sensor = KinectSensor.GetDefault();
            if (sensor == null) {
                System.Console.WriteLine("Kinect Disconnected");
                return; }

            mainGame = new MainGame();
            mainGame.Init(sensor, GameScreen);

            sensor.Open();
        }

     /*   private void low_minDistance_Checked(object sender, RoutedEventArgs e)
        {
            if (mainGame != null)
            {
                mainGame.minDistance = 500;
                mainGame.CircleInit();
            }
        }

        private void normal_minDistance_Checked(object sender, RoutedEventArgs e)
        {
            if (mainGame != null)
            {
                mainGame.minDistance = 700;
                mainGame.CircleInit();
            }
        }

        private void High_minDistance_Checked(object sender, RoutedEventArgs e)
        {
            if (mainGame != null)
            {
                mainGame.minDistance = 900;
                mainGame.CircleInit();
            }
        }
     */

     /*   private void Low_maxDistance_Checked(object sender, RoutedEventArgs e)
        {
            if (mainGame != null)
            {
                mainGame.maxDistance = 1200;
                mainGame.CircleInit();
            }
        }

        private void normal_maxDistance_Checked(object sender, RoutedEventArgs e)
        {
            if (mainGame != null)
            {
                mainGame.maxDistance = 1600;
                mainGame.CircleInit();
            }
        }

        private void high_maxDistance_Checked(object sender, RoutedEventArgs e)
        {
            if (mainGame != null)
            {
                mainGame.maxDistance = 2000;
                mainGame.CircleInit();
            }
        }
     */

        private void Static_Checked(object sender, RoutedEventArgs e)
        {
            if(mainGame != null)
            {
                mainGame.IsMove = false;
                mainGame.CircleInit();
            }
            
        }

        private void MoveSlow_Checked(object sender, RoutedEventArgs e)
        {
            if (mainGame != null)
            {
                mainGame.IsMove = true;
                mainGame.baseSpeed = 0.5F;
                mainGame.CircleInit();
            }
        }

        private void MoveFast_Checked(object sender, RoutedEventArgs e)
        {
            if (mainGame != null)
            {
                mainGame.IsMove = true;
                mainGame.baseSpeed = 2;
                mainGame.CircleInit();
            }
        }

        private void MinDistance_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(mainGame != null)
            {
                mainGame.minDistance = (float)MinDistance_slider.Value;
                if (mainGame.IsInit)
                {
                    mainGame.CircleInit();
                }
                
            }
        }

        private void MaxDistance_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mainGame != null)
            {
                mainGame.maxDistance = (float)MaxDistance_slider.Value;
                if (mainGame.IsInit)
                {
                    mainGame.CircleInit();
                }
            }
        }
    }
}
