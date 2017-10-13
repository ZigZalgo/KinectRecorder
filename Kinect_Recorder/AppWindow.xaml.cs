using Microsoft.Kinect;
using Kinect_Recorder.Helpers;
using Kinect_Recorder.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Kinect_Recorder
{
    /// <summary>
    /// Interaction logic for AppWindow.xaml
    /// </summary>
    public partial class AppWindow : Window
    {

        #region Initialization and Disposing

        public AppWindow(string sodServer, int port, string sensorID, string hub)
        {
            InitializeComponent();
            AppendLog("Application started");
            InitializeKinect();
            AwwYeahMath.Instance.RunMathTest();
        }

        public bool IsKinectRuntimeInstalled
        {
            get
            {
                bool isInstalled;
                try
                {
                    TestForKinectTypeLoadException();
                    isInstalled = true;
                }
                catch (TypeInitializationException)
                {
                    isInstalled = false;
                }
                return isInstalled;
            }
        }

        private void TestForKinectTypeLoadException()
        {
            KinectSensor kinectCheck = KinectSensor.GetDefault();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        #endregion


        #region Logs

        /// <summary>
        /// Stores all log information.
        /// </summary>
        private StringBuilder logger;

        /// <summary>
        /// Manages logs and displays them.
        /// </summary>
        /// <param name="msg"></param>
        public void AppendLog(string msg)
        {
            // Initialize logger
            if (logger == null)
                logger = new StringBuilder();

            // Create detailed message
            string detailedMsg = "[" + DateTime.Now.ToString() + "] " + msg + "\n";

            // Append message to log with timestamp
            logger.Insert(0, detailedMsg);

            // Display in the richtextbox
            logs_richtextbox.Document.Blocks.Clear();
            logs_richtextbox.Document.Blocks.Add(new Paragraph(new Run(logger.ToString())));
            Console.WriteLine(detailedMsg);
        }

        #endregion


        #region Kinect

        /// <summary>
        /// Responsible for rendering the depth data
        /// </summary>
        private DepthRenderer depthRenderer;

        /// <summary>
        /// Responsible for rendering the skeleton data
        /// </summary>
        private SkeletonRenderer skeletonRenderer;



        /// <summary>
        /// Initialize kinect
        /// </summary>
        public void InitializeKinect()
        {
            if (!IsKinectRuntimeInstalled)
            {
                MessageBoxResult result = MessageBox.Show("Microsoft Kinect Runtime 1.8 is required.\nClick \"OK\" to download Microsoft Kinect Runtime 1.8 from Microsoft's website.",
                    "Kinect Runtime required",
                    MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    System.Diagnostics.Process.Start("https://www.microsoft.com/en-ca/download/details.aspx?id=44559");
                }
            }
            Properties.Settings.Default.DefaultDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\cache\\";
            UpdateUIKinectConnection(false);
            InitializeKinectRendering();
            KinectManager.Instance.KinectSensor.IsAvailableChanged += KinectSensor_IsAvailableChanged;
        }

        /// <summary>
        /// Initialize the kinect rendering
        /// </summary>
        private void InitializeKinectRendering()
        {
            // Open the reader for the body frames
            KinectManager.Instance.DepthReader.FrameArrived += DepthReader_FrameArrived;
            KinectManager.Instance.BodyReader.FrameArrived += BodyReader_FrameArrived;

            depthRenderer = new DepthRenderer(depthImage, KinectManager.Instance.KinectSensor);
            skeletonRenderer = new SkeletonRenderer(skeletonImage, KinectManager.Instance.KinectSensor);
        }



        /// <summary>
        /// Update Kinect connection state UI.
        /// </summary>
        /// <param name="state">Connection state of the Kinect.</param>
        public void UpdateUIKinectConnection(bool state)
        {
            string imageURL = "";
            string kinectStatusText = "";

            // Connected
            if (state)
            {
                imageURL = @"/Kinect_Recorder;component/Resources/green_circle.png";
                kinectStatusText = "Connected";
                kinect_view_feed_button.IsEnabled = true;
            }

            // Disconnected
            else
            {
                if(FileRecording.recording)
                    Stop_Recording();
                Kinect_Feed.IsEnabled = false;
                Kinect_Feed.Visibility = Visibility.Hidden;
                Recording_Canvas.IsEnabled = false;
                Recording_Canvas.Visibility = Visibility.Hidden;
                View_Feed_Canvas.IsEnabled = true;
                View_Feed_Canvas.Visibility = Visibility.Visible;
                kinect_view_feed_button.IsEnabled = false;
                imageURL = @"/Kinect_Recorder;component/Resources/red_circle.png";
                kinectStatusText = "Disconnected";
            }

            // Load and set the new image state indicator
            kinect_connection_state_image.Source = new BitmapImage(new Uri(imageURL, UriKind.RelativeOrAbsolute));

            // Change label text to reflect new status
            kinect_connection_state_label.Content = kinectStatusText;
        }

        /// <summary>
        /// Open the Kinect feed.
        /// </summary>
        private void kinect_view_feed_button_Click(object sender, RoutedEventArgs e)
        {
            Kinect_Feed.IsEnabled = true;
            Kinect_Feed.Visibility = Visibility.Visible;
            Recording_Canvas.IsEnabled = true;
            Recording_Canvas.Visibility = Visibility.Visible;
            View_Feed_Canvas.IsEnabled = false;
            View_Feed_Canvas.Visibility = Visibility.Hidden;
            string imageURL = @"/Kinect_Recorder;component/Resources/green_circle.png";
            Recording_Image.Source = new BitmapImage(new Uri(imageURL, UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// Get triggered when the kinnect connection changes
        /// </summary>
        private void KinectSensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            UpdateUIKinectConnection(e.IsAvailable);
            AppendLog("Kinect status changed: " + (e.IsAvailable ? "connected" : "disconnected"));
        }



        /// <summary>
        /// Display the skeleton data when a frame is received
        /// </summary>
        private void BodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            BodyFrameReference frameReference = e.FrameReference;
            BodyFrame frame = frameReference.AcquireFrame();

            if (frame != null)
                Dispatcher.Invoke(() => { skeletonRenderer.Reader_FrameArrived(e); frame.Dispose(); });
        }

        /// <summary>
        /// Display the depth data when a frame is received
        /// </summary>
        private void DepthReader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            DepthFrameReference frameReference = e.FrameReference;
            DepthFrame frame = frameReference.AcquireFrame();

            if (frame != null)
                Dispatcher.Invoke(() => { depthRenderer.RenderDepthFrame(e); frame.Dispose(); });
        }

        private void Record_Clicked(object sender, RoutedEventArgs e)
        {
            string imageURL = "";
            //Null Coalesce that bool? so gently bbe
            if (Record_Button.IsChecked ?? false)
            {
                FileRecording.recording = true;
                Record_Button.Content = "Stop Rec";
                imageURL = @"/Kinect_Recorder;component/Resources/red_circle.png";

            }
            else
            {
                Stop_Recording();
                imageURL = @"/Kinect_Recorder;component/Resources/green_circle.png";
            }
            // Load and set the new image state indicator
            Recording_Image.Source = new BitmapImage(new Uri(imageURL, UriKind.RelativeOrAbsolute));
        }

        private void Stop_Recording()
        {
            FileRecording.recording = false;
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Joints"; // Default file name
            dlg.DefaultExt = ".JSON"; // Default file extension
            dlg.Filter = "JSon documents (.JSON)|*.JSON"; // Filter files by extension
                                                          // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();
            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                FileRecording.endFile(filename);
            }

            Record_Button.Content = "Rec";
            FileRecording.GenNewTempStreamWriter();
        }

        private void Change_Directory_Button_Click(object sender, RoutedEventArgs e)
        {
            //OpenFileDialog
        }

        #endregion

    }
}