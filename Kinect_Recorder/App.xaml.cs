using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace Kinect_Recorder
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void App_Startup(object sender, StartupEventArgs e)
        {
            string sodServer = null;
            int port = -1;
            string sensorID = null;
			string hub = null;
            
            if(e.Args.Length > 0)
                sodServer = e.Args[0];

            if (e.Args.Length > 1)
                int.TryParse(e.Args[1], out port);

            if (e.Args.Length > 2)
                sensorID = e.Args[2];

			if (e.Args.Length > 3)
				hub = e.Args[3];

            // Create main application window, starting minimized if specified
            //MainWindow mainWindow = new MainWindow(sodServer, port, sensorID);
            //mainWindow.Show();

            AppWindow app = new AppWindow(sodServer, port, sensorID, hub);
            app.Show();
        }
    }
}