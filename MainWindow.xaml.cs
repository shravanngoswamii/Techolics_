using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

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
                using (
                    var searcher = new ManagementObjectSearcher(
                        "SELECT Version FROM Win32_OperatingSystem"
                    )
                )
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
                using (
                    var searcher = new ManagementObjectSearcher(
                        "SELECT Caption FROM Win32_OperatingSystem"
                    )
                )
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

        private void AuditButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItemsListBox.Items.Count == 0)
            {
                MessageBox.Show(
                    "Please select at least one profile.",
                    "No Profile Selected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (!IsUserAdministrator())
            {
                AdminPromptWindow adminPrompt = new AdminPromptWindow();
                adminPrompt.ShowDialog();
                return;
            }

            PolicyExplorerWindow policyExplorerWindow = new PolicyExplorerWindow(
                GetSelectedProfiles(),
                "Audit"
            );
            policyExplorerWindow.Show();
            this.Close();
        }

        private void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItemsListBox.Items.Count == 0)
            {
                MessageBox.Show(
                    "Please select at least one profile.",
                    "No Profile Selected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (!IsUserAdministrator())
            {
                AdminPromptWindow adminPrompt = new AdminPromptWindow();
                adminPrompt.ShowDialog();
                return;
            }

            PolicyExplorerWindow policyExplorerWindow = new PolicyExplorerWindow(
                GetSelectedProfiles(),
                "Config"
            );
            policyExplorerWindow.Show();
            this.Close();
        }

        private List<string> GetSelectedProfiles()
        {
            var profiles = new List<string>();
            foreach (ListBoxItem item in SelectedItemsListBox.Items)
            {
                if (item.Content != null)
                {
                    profiles.Add(item.Content.ToString()!);
                }
            }
            return profiles;
        }


        // Event Handlers for Buttons
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (AvailableItemsListBox.SelectedItem is ListBoxItem selectedItem)
            {
                AvailableItemsListBox.Items.Remove(selectedItem);
                SelectedItemsListBox.Items.Add(selectedItem);
            }
            else
            {
                MessageBox.Show(
                    "Please select an item to add.",
                    "No Selection",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }

        private void AddAllButton_Click(object sender, RoutedEventArgs e)
        {
            var itemsToMove = new List<ListBoxItem>();

            foreach (var item in AvailableItemsListBox.Items)
            {
                if (item is ListBoxItem listBoxItem)
                {
                    itemsToMove.Add(listBoxItem);
                }
            }

            foreach (var item in itemsToMove)
            {
                AvailableItemsListBox.Items.Remove(item);
                SelectedItemsListBox.Items.Add(item);
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItemsListBox.SelectedItem is ListBoxItem selectedItem)
            {
                SelectedItemsListBox.Items.Remove(selectedItem);
                AvailableItemsListBox.Items.Add(selectedItem);
            }
            else
            {
                MessageBox.Show(
                    "Please select an item to remove.",
                    "No Selection",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }

        private void RemoveAllButton_Click(object sender, RoutedEventArgs e)
        {
            var itemsToMove = new List<ListBoxItem>();

            foreach (var item in SelectedItemsListBox.Items)
            {
                if (item is ListBoxItem listBoxItem)
                {
                    itemsToMove.Add(listBoxItem);
                }
            }

            foreach (var item in itemsToMove)
            {
                SelectedItemsListBox.Items.Remove(item);
                AvailableItemsListBox.Items.Add(item);
            }
        }
    }
}
