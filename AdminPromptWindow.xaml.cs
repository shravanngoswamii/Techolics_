using System;
using System.Diagnostics;
using System.Windows;

namespace Techolics_
{
    public partial class AdminPromptWindow : Window
    {
        public AdminPromptWindow()
        {
            InitializeComponent();
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            // Restart the application with admin privileges
            var exeName = Process.GetCurrentProcess().MainModule?.FileName;
            if (exeName == null)
            {
                MessageBox.Show("Failed to retrieve the executable name.");
                return;
            }
            var startInfo = new ProcessStartInfo(exeName)
            {
                UseShellExecute = true,
                Verb = "runas"
            };
            try
            {
                Process.Start(startInfo);
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to restart with admin privileges: " + ex.Message);
            }
        }


        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
