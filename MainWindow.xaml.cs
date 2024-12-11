// MainWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Techolics_.Logging;
using Wpf.Ui.Controls;
using Wpf.Ui.Appearance;

using WpfMessageBox = Wpf.Ui.Controls.MessageBox;
using Wpf.Ui.Extensions;
namespace Techolics_
{
    public partial class MainWindow : FluentWindow
    {
        private DispatcherTimer timer;

        // Master list to preserve original order
        private List<Profile> AllProfiles = new List<Profile>
        {
            new Profile { Name = "L1", Description = "Level 1 (L1) - Corporate/Enterprise Environment (general use)" },
            new Profile { Name = "L1+BL", Description = "Level 1 (L1) + BitLocker (BL)" },
            new Profile { Name = "L2", Description = "Level 2 (L2) - High Security/Sensitive Data Environment (limited functionality)" },
            new Profile { Name = "L2+BL", Description = "Level 2 (L2) + BitLocker (BL)" },
            new Profile { Name = "BL", Description = "BitLocker (BL) - optional add-on for when BitLocker is deployed" }
        };

        // ObservableCollections for data binding
        public ObservableCollection<Profile> AvailableProfiles { get; set; }
        public ObservableCollection<Profile> SelectedProfiles { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Loaded += (sender, args) =>
            {
                SystemThemeWatcher.Watch(
                    this,                                    // Window instance
                    WindowBackdropType.Mica,                // Background type
                    true                                     // Automatically update accents
                );
            };

            // Clean up the watcher when the window is closing
            Closing += (sender, args) =>
            {
                if (new System.Windows.Interop.WindowInteropHelper(this).Handle != IntPtr.Zero)
                {
                    SystemThemeWatcher.UnWatch(this);
                }
            };


            // Initialize ObservableCollections
            AvailableProfiles = new ObservableCollection<Profile>(AllProfiles);
            SelectedProfiles = new ObservableCollection<Profile>();

            // Set DataContext for data binding
            this.DataContext = this;

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

            // Initialize button states
            UpdateToggleButtons();
        }
        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // Unwatch the theme changes to clean up resources
            SystemThemeWatcher.UnWatch(this as System.Windows.Window);
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
                System.Windows.MessageBox.Show("Error retrieving Windows version: " + ex.Message);
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
                System.Windows.MessageBox.Show("Error retrieving Windows edition: " + ex.Message);
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

        private async void AuditButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsUserAdministrator())
            {
                AdminPromptWindow adminPrompt = new AdminPromptWindow();
                SystemThemeWatcher.UnWatch(this as System.Windows.Window);
                adminPrompt.ShowDialog();
                return;
            }

            if (SelectedProfiles.Count == 0)
            {
                var messageBox = new Wpf.Ui.Controls.MessageBox
                {
                    Title = "No Profile Selected",
                    Content = "Please select at least one profile.",
                    CloseButtonText = "Close",

                };
                await messageBox.ShowDialogAsync();


                return;
            }

            //if (!IsUserAdministrator())
            //{
            //    AdminPromptWindow adminPrompt = new AdminPromptWindow();
            //    SystemThemeWatcher.UnWatch(this as System.Windows.Window);
            //    adminPrompt.ShowDialog();
            //    return;
            //}
            SystemThemeWatcher.Watch(this as System.Windows.Window);

            PolicyExplorerWindow policyExplorerWindow = new PolicyExplorerWindow(
                GetSelectedProfileNames(),
                "Audit"
            );
            policyExplorerWindow.Show();
            this.Close();

        }

        private async void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedProfiles.Count == 0)
            {
                var messageBox = new Wpf.Ui.Controls.MessageBox
                {
                    Title = "No Profile Selected",
                    Content = "Please select at least one profile.",
                    CloseButtonText = "Close",

                };
                await messageBox.ShowDialogAsync();
                return;
            }

            if (!IsUserAdministrator())
            {
                AdminPromptWindow adminPrompt = new AdminPromptWindow();
                adminPrompt.ShowDialog();
                return;
            }

            PolicyExplorerWindow policyExplorerWindow = new PolicyExplorerWindow(
                GetSelectedProfileNames(),
                "Config"
            );
            policyExplorerWindow.Show();
            this.Close();

        }

        private List<string> GetSelectedProfileNames()
        {
            return SelectedProfiles.Select(p => p.Name).ToList();
        }

        #region Button Event Handlers

        // Toggle Add/Remove Button
        private void ToggleAddRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            // Determine which list has a selected item
            if (AvailableItemsListBox.SelectedItem is Profile selectedAvailableProfile)
            {
                // Add the selected profile
                AvailableProfiles.Remove(selectedAvailableProfile);
                SelectedProfiles.Add(selectedAvailableProfile);
                Logger.Instance.WriteLog($"Added profile: {selectedAvailableProfile.Name}");

                // Update button states
                UpdateToggleButtons();
            }
            else if (SelectedItemsListBox.SelectedItem is Profile selectedSelectedProfile)
            {
                // Remove the selected profile
                SelectedProfiles.Remove(selectedSelectedProfile);
                InsertProfileBackToAvailable(selectedSelectedProfile);
                Logger.Instance.WriteLog($"Removed profile: {selectedSelectedProfile.Name}");

                // Update button states
                UpdateToggleButtons();
            }
            else
            {
                System.Windows.MessageBox.Show(
                    "Please select an item to add or remove.",
                    "No Selection",
                    System.Windows.MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }

        // Toggle Add All / Remove All Button
        private void ToggleAddAllRemoveAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (AddAllRemoveAllText.Text == "Add All")
            {
                // Add all profiles
                var profilesToAdd = AvailableProfiles.ToList();
                foreach (var profile in profilesToAdd)
                {
                    AvailableProfiles.Remove(profile);
                    SelectedProfiles.Add(profile);
                    Logger.Instance.WriteLog($"Added profile: {profile.Name}");
                }

                // Update button text and icon
                AddAllRemoveAllText.Text = "Remove All";
                try
                {
                    AddAllRemoveAllIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/remove.png"));
                }
                catch (Exception ex)
                {
                    Logger.Instance.WriteLog($"Error loading remove all icon: {ex.Message}");
                }
            }
            else
            {
                // Remove all profiles
                var profilesToRemove = SelectedProfiles.ToList();
                foreach (var profile in profilesToRemove)
                {
                    SelectedProfiles.Remove(profile);
                    InsertProfileBackToAvailable(profile);
                    Logger.Instance.WriteLog($"Removed profile: {profile.Name}");
                }

                // Update button text and icon
                AddAllRemoveAllText.Text = "Add All";
                try
                {
                    AddAllRemoveAllIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/add.png"));
                }
                catch (Exception ex)
                {
                    Logger.Instance.WriteLog($"Error loading add all icon: {ex.Message}");
                }
            }

            // Update the Add/Remove button based on current selections
            UpdateToggleButtons();
        }

        #endregion

        #region Selection Changed Handlers

        private void AvailableItemsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateToggleButtons();
        }

        private void SelectedItemsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateToggleButtons();
        }

        #endregion

        #region Helper Methods

        private void UpdateToggleButtons()
        {
            // Update Add/Remove Button
            if (AvailableItemsListBox.SelectedItem != null)
            {
                AddRemoveText.Text = "Add";
                try
                {
                    AddRemoveIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/add.png"));
                }
                catch (Exception ex)
                {
                    Logger.Instance.WriteLog($"Error loading add icon: {ex.Message}");
                }
                ToggleAddRemoveButton.IsEnabled = true;
            }
            else if (SelectedItemsListBox.SelectedItem != null)
            {
                AddRemoveText.Text = "Remove";
                try
                {
                    AddRemoveIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/remove.png"));
                }
                catch (Exception ex)
                {
                    Logger.Instance.WriteLog($"Error loading remove icon: {ex.Message}");
                }
                ToggleAddRemoveButton.IsEnabled = true;
            }
            else
            {
                AddRemoveText.Text = "Add";
                try
                {
                    AddRemoveIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/add.png"));
                }
                catch (Exception ex)
                {
                    Logger.Instance.WriteLog($"Error loading add icon: {ex.Message}");
                }
                ToggleAddRemoveButton.IsEnabled = false;
            }

            // Update Add All / Remove All Button
            if (AvailableProfiles.Count > 0)
            {
                AddAllRemoveAllText.Text = "Add All";
                try
                {
                    AddAllRemoveAllIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/add.png"));
                }
                catch (Exception ex)
                {
                    Logger.Instance.WriteLog($"Error loading add all icon: {ex.Message}");
                }
            }
            else
            {
                AddAllRemoveAllText.Text = "Remove All";
                try
                {
                    AddAllRemoveAllIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/remove.png"));
                }
                catch (Exception ex)
                {
                    Logger.Instance.WriteLog($"Error loading remove all icon: {ex.Message}");
                }
            }
        }

        private void InsertProfileBackToAvailable(Profile profile)
        {
            // Find the index of the profile in the master list
            int originalIndex = AllProfiles.IndexOf(profile);
            if (originalIndex == -1)
            {
                // If not found, add to the end
                AvailableProfiles.Add(profile);
                return;
            }

            // Find the first profile in AvailableProfiles with a higher original index
            for (int i = 0; i < AvailableProfiles.Count; i++)
            {
                if (AllProfiles.IndexOf(AvailableProfiles[i]) > originalIndex)
                {
                    AvailableProfiles.Insert(i, profile);
                    return;
                }
            }

            // If no profile has a higher index, add to the end
            AvailableProfiles.Add(profile);
        }

        #endregion

        // Internal Profile class
        public class Profile
        {
            public string Name { get; set; }
            public string Description { get; set; }

            public Profile()
            {
                Name = string.Empty;
                Description = string.Empty;
            }

            public Profile(string name, string description)
            {
                Name = name;
                Description = description;
            }
        }

    }
}
