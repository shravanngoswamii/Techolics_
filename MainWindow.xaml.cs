using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Security.Principal;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;

namespace Techolics_
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();

            // Display system details
            UsernameTextBlock.Text = Environment.UserName;
            WindowsVersionTextBlock.Text = GetWindowsVersion();
            EditionTextBlock.Text = GetWindowsEdition();
            DomainTextBlock.Text = GetDomainInfo();

            // Check admin privileges
            if (IsUserAdministrator())
            {
                AdminStatusEllipse.Fill = System.Windows.Media.Brushes.Green;
            }
            else
            {
                AdminStatusEllipse.Fill = System.Windows.Media.Brushes.Red;
                AdminStatusEllipse.MouseLeftButtonDown += AdminStatusEllipse_MouseLeftButtonDown;
                AdminStatusEllipse.Cursor = Cursors.Hand;
                AdminStatusEllipse.ToolTip = "Click to restart with admin privileges";
            }

            // Initialize timer for current time
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();

            // Handle "Select All" checkbox events
            SelectAllCheckBox.Checked += SelectAllCheckBox_Checked;
            SelectAllCheckBox.Unchecked += SelectAllCheckBox_Unchecked;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            CurrentTimeTextBlock.Text = DateTime.Now.ToString("hh:mm:ss tt");
        }

        private string GetWindowsVersion()
        {
            string version = "Unknown";
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Version FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject os in searcher.Get())
                    {
                        version = os["Version"]?.ToString() ?? "Unknown";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving Windows version: " + ex.Message);
            }
            return version;
        }

        private string GetWindowsEdition()
        {
            string edition = "Unknown";
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject os in searcher.Get())
                    {
                        edition = os["Caption"]?.ToString() ?? "Unknown";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving Windows edition: " + ex.Message);
            }
            return edition;
        }

        private string GetDomainInfo()
        {
            if (Environment.MachineName == Environment.UserDomainName)
            {
                return "Standalone";
            }
            else
            {
                return Environment.UserDomainName;
            }
        }

        private bool IsUserAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void AdminStatusEllipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Show the admin prompt
            AdminPromptWindow adminPrompt = new AdminPromptWindow();
            adminPrompt.ShowDialog();
        }

        private void SelectAllCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            L1CheckBox.IsChecked = true;
            L1BLCheckBox.IsChecked = true;
            L2CheckBox.IsChecked = true;
            L2BLCheckBox.IsChecked = true;
            BLCheckBox.IsChecked = true;
        }

        private void SelectAllCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            L1CheckBox.IsChecked = false;
            L1BLCheckBox.IsChecked = false;
            L2CheckBox.IsChecked = false;
            L2BLCheckBox.IsChecked = false;
            BLCheckBox.IsChecked = false;
        }

        private List<string> GetSelectedProfiles()
        {
            var profiles = new List<string>();
            if (L1CheckBox.IsChecked == true) profiles.Add("L1");
            if (L1BLCheckBox.IsChecked == true) profiles.Add("L1+BL");
            if (L2CheckBox.IsChecked == true) profiles.Add("L2");
            if (L2BLCheckBox.IsChecked == true) profiles.Add("L2+BL");
            if (BLCheckBox.IsChecked == true) profiles.Add("BL");
            return profiles;
        }

        private void AuditButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedProfiles = GetSelectedProfiles();
            if (selectedProfiles.Count == 0)
            {
                MessageBox.Show("Please select at least one profile.", "No Profile Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!IsUserAdministrator())
            {
                // Show the admin prompt
                AdminPromptWindow adminPrompt = new AdminPromptWindow();
                adminPrompt.ShowDialog();
                return;
            }

            PolicyExplorerWindow policyExplorerWindow = new PolicyExplorerWindow(selectedProfiles, "Audit");
            policyExplorerWindow.Show();
            this.Close();
        }

        private void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedProfiles = GetSelectedProfiles();
            if (selectedProfiles.Count == 0)
            {
                MessageBox.Show("Please select at least one profile.", "No Profile Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!IsUserAdministrator())
            {
                // Show the admin prompt
                AdminPromptWindow adminPrompt = new AdminPromptWindow();
                adminPrompt.ShowDialog();
                return;
            }

            PolicyExplorerWindow policyExplorerWindow = new PolicyExplorerWindow(selectedProfiles, "Config");
            policyExplorerWindow.Show();
            this.Close();
        }
    }
}
