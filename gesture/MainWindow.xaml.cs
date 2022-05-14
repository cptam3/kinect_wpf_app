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
using Microsoft.Kinect.VisualGestureBuilder;

namespace NotCorporateSlave
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor sensor;
        private BodyFrameManager bodyFrameManager;
        private ColorFrameManager colorFrameManger;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            sensor = KinectSensor.GetDefault(); // get the default Kinect sensor 
            sensor.Open();

            bodyFrameManager = new BodyFrameManager();
            bodyFrameManager.Init(sensor, GameScreen, recognitionResult);

            colorFrameManger = new ColorFrameManager();
            colorFrameManger.Init(sensor, ColorScreen);
        }

        private void doc_rate_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bodyFrameManager != null)
            {
                bodyFrameManager.doc_prob = doc_rate.Value;
                bodyFrameManager.GameInit();
               

            }
        }

        private void Num_trail_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bodyFrameManager != null)
            {
                bodyFrameManager.numOfTrail = (int)Num_trail.Value;
                bodyFrameManager.GameInit();
               
            }
        }
    }
}
