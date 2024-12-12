// PolicyExplorerWindow.xaml.cs
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Techolics_.Logging;
using Techolics_.PolicyManagement;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using WpfMessageBox = Wpf.Ui.Controls.MessageBox;
namespace Techolics_
{
    public partial class PolicyExplorerWindow : FluentWindow
    {
        private PolicyWindowLogic logic;
        private List<string> selectedProfiles;
        private string operation;

        // Add a DispatcherTimer to periodically refresh the logs
        private DispatcherTimer logRefreshTimer;

        // Use ObservableCollection for dynamic updates
        public ObservableCollection<Item> Items { get; set; } = new ObservableCollection<Item>();

        public PolicyExplorerWindow(List<string> selectedProfiles, string operation)
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

            this.selectedProfiles = selectedProfiles;
            this.operation = operation;
            this.Title = "Policy Explorer"; // Set the title

            logic = new PolicyWindowLogic(this, selectedProfiles, operation);

            // Set the DataGrid's ItemsSource to the ObservableCollection
            myDataGrid.ItemsSource = Items;

            // Hook up event handlers
            policyTreeView.SelectedItemChanged += logic.PolicyTreeView_SelectedItemChanged;
            myDataGrid.SelectionChanged += logic.MyDataGrid_SelectionChanged;

            // Control button visibility based on operation
            if (operation == "Audit")
            {
                ConfigButton.Visibility = Visibility.Collapsed;
                ConfigAllButton.Visibility = Visibility.Collapsed;
                RevertButton.Visibility = Visibility.Collapsed;
            }
            else if (operation == "Config")
            {
                // All buttons are visible
            }

            // Initialize and start the log refresh timer
            logRefreshTimer = new DispatcherTimer();
            logRefreshTimer.Interval = TimeSpan.FromSeconds(1);
            logRefreshTimer.Tick += LogRefreshTimer_Tick;
            logRefreshTimer.Start();

            this.Closed += Window_Closed;
        }

        private void LogRefreshTimer_Tick(object? sender, EventArgs e)
        {
            RefreshLogs();
        }

        private void RefreshLogs()
        {
            try
            {
                string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                string logFileName = "Techolics_Log.txt";
                string logFilePath = Path.Combine(logDirectory, logFileName);

                if (File.Exists(logFilePath))
                {
                    string[] allLines = File.ReadAllLines(logFilePath);
                    var formattedLines = new List<string>();
                    foreach (var line in allLines)
                    {
                        string cleanedLine = CleanLogLine(line);
                        formattedLines.Add(cleanedLine);
                    }

                    logsTextBox.Text = string.Join(Environment.NewLine, formattedLines);
                    logsTextBox.ScrollToEnd();
                }
                else
                {
                    logsTextBox.Text = "No logs available.";
                }
            }
            catch (Exception ex)
            {
                logsTextBox.Text = $"Failed to read logs: {ex.Message}";
            }
        }

        private string CleanLogLine(string logLine)
        {
            int fileIndex = logLine.IndexOf("| File:");
            if (fileIndex >= 0)
            {
                logLine = logLine.Substring(0, fileIndex).Trim();
            }
            return logLine;
        }

        private void Window_Closed(object? sender, EventArgs e)
        {
            logRefreshTimer.Stop();
        }

        private async void AuditButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = logic.GetSelectedItems();
            if (selectedItems.Count == 0)
            {
                var messageBox = new WpfMessageBox
                {
                    Title = "No Profile Selected",
                    Content = "Please select at least one policy.",
                    CloseButtonText = "Close",
                };
                await messageBox.ShowDialogAsync();
                return;
            }
            logic.StartAudit(selectedItems);
        }

        private async void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = logic.GetSelectedItems();
            if (selectedItems.Count == 0)
            {
                var messageBox = new WpfMessageBox
                {
                    Title = "No Policy Selected",
                    Content = "Please select at least one policy.",
                    CloseButtonText = "Close",
                };
                await messageBox.ShowDialogAsync();
                return;
            }
            // Pass fromEditWindow: false
            logic.StartConfig(selectedItems, fromEditWindow: false);
        }

        private async void RevertButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = logic.GetSelectedItems();
            if (selectedItems.Count == 0)
            {
                var messageBox = new WpfMessageBox
                {
                    Title = "No Profile Selected",
                    Content = "Please select at least one policy to revert.",
                    CloseButtonText = "Close",
                };
                await messageBox.ShowDialogAsync();
                return;
            }
            logic.StartRevert(selectedItems);
        }

        private async void SelectAllCheckBox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool newValue = (SelectAllCheckBox.IsChecked == true);
                foreach (var item in Items)
                {
                    item.IsSelected = newValue;
                }
                Logger.Instance.WriteLog($"Select All clicked. New value: {newValue}");
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteLog($"Error in SelectAllCheckBox_Click: {ex.Message}");
                var messageBox = new WpfMessageBox
                {
                    Title = "Error",
                    Content = "An error occurred while selecting all policies.",
                    CloseButtonText = "OK",
                };
                await messageBox.ShowDialogAsync();
                return;
            }
        }

        private async void AuditAllButton_Click(object sender, RoutedEventArgs e)
        {
            var allItems = logic.GetAllItems();
            if (allItems.Count == 0)
            {
                var messageBox = new WpfMessageBox
                {
                    Title = "No Policies",
                    Content = "No policies available to audit.",
                    CloseButtonText = "Close",
                };
                await messageBox.ShowDialogAsync();
                return;
            }
            logic.StartAudit(allItems);
        }

        private async void ConfigAllButton_Click(object sender, RoutedEventArgs e)
        {
            var allItems = logic.GetAllItems();
            if (allItems.Count == 0)
            {
                var messageBox = new WpfMessageBox
                {
                    Title = "No Policies",
                    Content = "No policies available for configuration.",
                    CloseButtonText = "OK",
                };
                await messageBox.ShowDialogAsync();
                return;
            }
            // Pass fromEditWindow: false
            logic.StartConfig(allItems, fromEditWindow: false);
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = logic.GetSelectedItems();

            if (selectedItems.Count == 0)
            {
                var messageBox = new WpfMessageBox
                {
                    Title = "No Policy Selected",
                    Content = "Please select a policy to edit.",
                    CloseButtonText = "OK",
                };
                await messageBox.ShowDialogAsync();
                return;
            }

            if (selectedItems.Count > 1)
            {
                var messageBox = new WpfMessageBox
                {
                    Title = "Multiple Policies Selected",
                    Content = "Please select only one policy at a time to edit.",
                    CloseButtonText = "OK",
                };
                await messageBox.ShowDialogAsync();
                return;
            }

            var selectedItem = selectedItems[0];

            // Pass 'this' as the PolicyExplorerWindow reference
            var editWindow = new Techolics_.Pages.EditPolicyWindow(this, selectedItem, logic.GetBenchmarkValues(), logic.GetBenchmarkDocumentation());
            editWindow.Owner = this;
            bool? result = editWindow.ShowDialog();

            if (result == true)
            {
                myDataGrid.Items.Refresh();
            }
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Implement back navigation logic here
            try
            {
                // Show MainWindow
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();

                // Close the current window
                this.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error navigating back: {ex.Message}");
            }

        }
        private async void CreateGPOButton_Click(object sender, RoutedEventArgs e)
        {
            // Prompt user for GPO name
            var nameDialog = new InputDialogWindow("Create GPO", "Enter GPO Name (leave blank for a random name):");
            nameDialog.Owner = this;
            bool? nameResult = nameDialog.ShowDialog();
            if (nameResult != true)
            {
                // User canceled or closed the dialog
                return;
            }
            string gpoName = string.IsNullOrWhiteSpace(nameDialog.UserInput) ? $"GPO_{DateTime.Now:yyyyMMdd_HHmmss}" : nameDialog.UserInput.Trim();

            // Prompt user for GPO description
            var descDialog = new InputDialogWindow("GPO Description", "Enter GPO Description (optional):");
            descDialog.Owner = this;
            bool? descResult = descDialog.ShowDialog();
            string gpoDescription = descResult == true ? descDialog.UserInput.Trim() : string.Empty;

            // Prompt user to select where to save the GPO using CommonOpenFileDialog
            var folderDialog = new CommonOpenFileDialog
            {
                Title = "Select a directory to save the GPO",
                IsFolderPicker = true,
                EnsurePathExists = true
            };

            if (folderDialog.ShowDialog() != CommonFileDialogResult.Ok)
            {
                // User canceled folder selection
                return;
            }

            string gpoFolderPath = Path.Combine(folderDialog.FileName, gpoName);
            Directory.CreateDirectory(gpoFolderPath);

            string machinePath = Path.Combine(gpoFolderPath, "MACHINE");
            Directory.CreateDirectory(machinePath);
            Directory.CreateDirectory(Path.Combine(machinePath, "Microsoft", "Windows NT", "SecEdit"));

            string userPath = Path.Combine(gpoFolderPath, "USER");
            Directory.CreateDirectory(userPath);

            var selectedItems = logic.GetSelectedItems();
            if (selectedItems == null || selectedItems.Count == 0)
            {
                var noPoliciesMsg = new Wpf.Ui.Controls.MessageBox
                {
                    Title = "No Policies Selected",
                    Content = "Please select at least one policy before creating a GPO.",
                    CloseButtonText = "OK"
                };
                await noPoliciesMsg.ShowDialogAsync();
                return;
            }

            bool hasSeceditPolicies = false;
            bool hasRegistryPolicies = false;

            var seceditLines = new List<string>
            {
                "[Unicode]",
                "Unicode=yes",
                "[Version]",
                "signature=\"$CHICAGO$\"",
                "Revision=1"
            };

            var registryEntries = new List<string> { "COMPUTER" };

            foreach (var item in selectedItems)
            {
                var p = item.Policy;
                if (p == null) continue;
                string? targetValue = GetPolicyFinalValueForGPO(p, false);

                if (p.Implementation?.Secedit != null && !string.IsNullOrEmpty(p.Implementation.Secedit.TemplateSetting))
                {
                    hasSeceditPolicies = true;
                    string section = p.Implementation.Secedit.Section;
                    if (string.IsNullOrEmpty(section))
                        section = "System Access";

                    if (!seceditLines.Any(l => l.Equals($"[{section}]", StringComparison.OrdinalIgnoreCase)))
                        seceditLines.Add($"[{section}]");

                    string convertedValue = PolicyValueConverter.ConvertForConfiguration(targetValue ?? "No one", p.ValueType);
                    string settingLine = p.Implementation.Secedit.TemplateSetting.Replace("%Value%", convertedValue);
                    seceditLines.Add(settingLine);
                }
                else if (p.Implementation?.Registry != null)
                {
                    hasRegistryPolicies = true;
                    string convertedValue = PolicyValueConverter.ConvertForConfiguration(targetValue ?? "", p.ValueType);

                    var regImpl = p.Implementation.Registry;
                    (string? hiveName, string? subKeyPath) = ParseRegistryKey(regImpl.Key);
                    if (!string.IsNullOrEmpty(subKeyPath))
                    {
                        registryEntries.Add(subKeyPath);
                        string lgpoValueLine = GetLGPORegistryValueLine(regImpl.ValueName, convertedValue, p.ValueType);
                        registryEntries.Add(lgpoValueLine);
                        registryEntries.Add(""); // blank line to separate keys
                    }
                }
            }

            // Write GptTmpl.inf if needed
            if (hasSeceditPolicies)
            {
                string seceditPath = Path.Combine(machinePath, "Microsoft", "Windows NT", "SecEdit");
                string infPath = Path.Combine(seceditPath, "GptTmpl.inf");
                File.WriteAllLines(infPath, seceditLines);
            }

            // If we have registry policies, run LGPO to create Registry.pol
            if (hasRegistryPolicies)
            {
                string tempLgpoFile = Path.GetTempFileName();
                File.WriteAllLines(tempLgpoFile, registryEntries);

                // Adjust LGPO.exe path as necessary
                string lgpoExePath = "LGPO.exe";
                if (!File.Exists(lgpoExePath))
                {
                    Logger.Instance.WriteLog("LGPO.exe not found. Please ensure it is available.");
                }
                else
                {
                    var startInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = lgpoExePath,
                        Arguments = $"/parse /m \"{tempLgpoFile}\" /path \"{gpoFolderPath}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (var proc = System.Diagnostics.Process.Start(startInfo))
                    {
                        proc?.WaitForExit();
                    }
                }

                if (File.Exists(tempLgpoFile))
                {
                    File.Delete(tempLgpoFile);
                }
            }

            // Store metadata
            if (!string.IsNullOrEmpty(gpoDescription))
            {
                File.WriteAllText(Path.Combine(gpoFolderPath, "gpo_metadata.txt"), $"Name: {gpoName}\nDescription: {gpoDescription}");
            }

            var successMsg = new Wpf.Ui.Controls.MessageBox
            {
                Title = "GPO Created",
                Content = $"GPO '{gpoName}' has been created successfully.",
                CloseButtonText = "OK"
            };
            await successMsg.ShowDialogAsync();
        }

        private string? GetPolicyFinalValueForGPO(Policy p, bool isRevert)
        {
            if (isRevert)
            {
                if (p.DefaultValue != null)
                {
                    if (!string.IsNullOrEmpty(p.DefaultValue.Value))
                        return p.DefaultValue.Value;
                    bool standalone = Environment.MachineName == Environment.UserDomainName;
                    return standalone ? p.DefaultValue.Standalone : p.DefaultValue.Domain;
                }
                return null;
            }
            else
            {
                if (p.ValueConstraints?.RequiredValues != null && p.ValueConstraints.RequiredValues.Count > 0)
                {
                    return p.ValueConstraints.RequiredValues[0].Value;
                }

                if (p.DefaultValue != null)
                {
                    if (!string.IsNullOrEmpty(p.DefaultValue.Value))
                        return p.DefaultValue.Value;
                    bool standalone = Environment.MachineName == Environment.UserDomainName;
                    return standalone ? p.DefaultValue.Standalone : p.DefaultValue.Domain;
                }
            }

            return null;
        }

        private (string? hiveName, string? subKeyPath) ParseRegistryKey(string key)
        {
            int firstBackslashIndex = key.IndexOf('\\');
            if (firstBackslashIndex <= 0)
            {
                return (null, null);
            }

            string hiveName = key.Substring(0, firstBackslashIndex);
            string subKeyPath = key.Substring(firstBackslashIndex + 1);
            return (hiveName, subKeyPath);
        }

        private string GetLGPORegistryValueLine(string valueName, string value, string valueType)
        {
            if (string.Equals(valueType, "Boolean", StringComparison.OrdinalIgnoreCase))
            {
                int v = int.Parse(value);
                return $"\"{valueName}\"=dword:{v.ToString("x8")}";
            }
            else if (string.Equals(valueType, "Integer", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(value, out int intVal))
                {
                    return $"\"{valueName}\"=dword:{intVal.ToString("x8")}";
                }
                else
                {
                    return $"\"{valueName}\"=\"{value}\"";
                }
            }
            else if (string.Equals(valueType, "String", StringComparison.OrdinalIgnoreCase))
            {
                return $"\"{valueName}\"=\"{value}\"";
            }

            return $"\"{valueName}\"=\"{value}\"";
        }
        private void myDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}
