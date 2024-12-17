// PolicyExplorerWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Linq;
using Microsoft.Win32;
using Techolics_.Logging;
using Techolics_.PolicyManagement;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using SystemButton = System.Windows.Controls.Button;
using WpfMessageBox = Wpf.Ui.Controls.MessageBox;
// Namespace Aliasing to Resolve Button Ambiguity
using WpfUiButton = Wpf.Ui.Controls.Button;

namespace Techolics_
{
    public partial class PolicyExplorerWindow : FluentWindow
    {
        private PolicyWindowLogic logic;
        private List<string> selectedProfiles;
        private string operation;

        // DispatcherTimer for log refreshing
        private DispatcherTimer logRefreshTimer;

        // ObservableCollection for DataGrid
        public ObservableCollection<Item> Items { get; set; } = new ObservableCollection<Item>();

        public PolicyExplorerWindow(List<string> selectedProfiles, string operation)
        {
            InitializeComponent();

            Loaded += (sender, args) =>
            {
                SystemThemeWatcher.Watch(
                    this, // Window instance
                    WindowBackdropType.Mica, // Background type
                    true // Automatically update accents
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
            var editWindow = new Techolics_.Pages.EditPolicyWindow(
                this,
                selectedItem,
                logic.GetBenchmarkValues(),
                logic.GetBenchmarkDocumentation()
            );
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
            var nameDialog = new InputDialogWindow(
                "Create GPO",
                "Enter GPO Name (leave blank for a random name):"
            );
            nameDialog.Owner = this;
            bool? nameResult = nameDialog.ShowDialog();
            if (nameResult != true)
            {
                // User canceled or closed the dialog
                return;
            }
            string gpoName = string.IsNullOrWhiteSpace(nameDialog.UserInput)
                ? $"GPO_{DateTime.Now:yyyyMMdd_HHmmss}"
                : nameDialog.UserInput.Trim();

            // Prompt user for GPO description
            var descDialog = new InputDialogWindow(
                "GPO Description",
                "Enter GPO Description (optional):"
            );
            descDialog.Owner = this;
            bool? descResult = descDialog.ShowDialog();
            string gpoDescription = descResult == true ? descDialog.UserInput.Trim() : string.Empty;

            // Prompt user to select where to save the GPO using OpenFolderDialog
            var folderDialog = new OpenFolderDialog
            {
                Title = "Select a directory to save the GPO",
            };

            if (folderDialog.ShowDialog() != true)
            {
                // User canceled folder selection
                return;
            }

            // Create GPO folder structure
            string gpoFolderPath = Path.Combine(folderDialog.FolderName, gpoName);
            Directory.CreateDirectory(gpoFolderPath);

            string machinePath = Path.Combine(gpoFolderPath, "MACHINE");
            Directory.CreateDirectory(machinePath);
            Directory.CreateDirectory(
                Path.Combine(machinePath, "Microsoft", "Windows NT", "SecEdit")
            );

            string userPath = Path.Combine(gpoFolderPath, "USER");
            Directory.CreateDirectory(userPath);

            var selectedItems = logic.GetSelectedItems();
            if (selectedItems == null || selectedItems.Count == 0)
            {
                var noPoliciesMsg = new WpfMessageBox
                {
                    Title = "No Policies Selected",
                    Content = "Please select at least one policy before creating a GPO.",
                    CloseButtonText = "OK",
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
                "Revision=1",
            };

            // No section header for registry policies
            // Each registry policy starts with "Computer"
            var registryEntries = new List<string>();

            foreach (var item in selectedItems)
            {
                var p = item.Policy;
                if (p == null)
                    continue;
                string? targetValue = string.IsNullOrWhiteSpace(item.CustomValue)
                    ? GetPolicyFinalValueForGPO(p, false)
                    : item.CustomValue;

                if (
                    p.Implementation?.Secedit != null
                    && !string.IsNullOrEmpty(p.Implementation.Secedit.TemplateSetting)
                )
                {
                    hasSeceditPolicies = true;
                    string section = p.Implementation.Secedit.Section;
                    if (string.IsNullOrEmpty(section))
                        section = "System Access";

                    if (
                        !seceditLines.Any(l =>
                            l.Equals($"[{section}]", StringComparison.OrdinalIgnoreCase)
                        )
                    )
                        seceditLines.Add($"[{section}]");

                    string convertedValue = PolicyValueConverter.ConvertForConfiguration(
                        targetValue ?? "No one",
                        p.ValueType
                    );
                    string settingLine = p.Implementation.Secedit.TemplateSetting.Replace(
                        "%Value%",
                        convertedValue
                    );
                    seceditLines.Add(settingLine);
                }
                else if (p.Implementation?.Registry != null)
                {
                    hasRegistryPolicies = true;
                    string convertedValue = PolicyValueConverter.ConvertForConfiguration(
                        targetValue ?? "",
                        p.ValueType
                    );

                    var regImpl = p.Implementation.Registry;
                    (string? hiveName, string? subKeyPath) = ParseRegistryKey(regImpl.Key);
                    if (!string.IsNullOrEmpty(subKeyPath))
                    {
                        // Each registry policy starts with "Computer"
                        registryEntries.Add("Computer");
                        registryEntries.Add(regImpl.Key);
                        registryEntries.Add(regImpl.ValueName);
                        registryEntries.Add($"{regImpl.ValueType}:{convertedValue}");

                        Logger.Instance.WriteLog(
                            $"Added Registry Policy:\nComputer\n{regImpl.Key}\n{regImpl.ValueName}\n{regImpl.ValueType}:{convertedValue}"
                        );
                    }
                }
            }

            // Write GptTmpl.inf if needed
            if (hasSeceditPolicies)
            {
                string seceditPath = Path.Combine(
                    machinePath,
                    "Microsoft",
                    "Windows NT",
                    "SecEdit"
                );
                string infPath = Path.Combine(seceditPath, "GptTmpl.inf");
                File.WriteAllLines(infPath, seceditLines);
                Logger.Instance.WriteLog($"Secedit policies written to {infPath}");
            }

            // If we have registry policies, run LGPO to create Registry.pol
            if (hasRegistryPolicies)
            {
                // Create the LGPO text file in the correct format
                string tempLgpoFile = Path.Combine(Path.GetTempPath(), $"tmp_{Guid.NewGuid()}.txt"); // Use .txt extension as per user example
                File.WriteAllLines(tempLgpoFile, registryEntries);
                Logger.Instance.WriteLog(
                    $"Registry policies written to temporary LGPO file: {tempLgpoFile}"
                );

                // Define the path to LGPO.exe
                string lgpoExePath = @"C:\Users\shrav\Downloads\LGPO_30\LGPO.exe"; // Update this path as needed

                if (!File.Exists(lgpoExePath))
                {
                    Logger.Instance.WriteLog("LGPO.exe not found. Please ensure it is available.");
                    var errorBox = new WpfMessageBox
                    {
                        Title = "LGPO.exe Not Found",
                        Content =
                            "LGPO.exe was not found at the specified path. Please ensure LGPO.exe is available.",
                        CloseButtonText = "OK",
                    };
                    await errorBox.ShowDialogAsync();
                }
                else
                {
                    // Define the path where Registry.pol should be placed within the GPO folder
                    string registryPolPath = Path.Combine(
                        machinePath,
                        "Microsoft",
                        "Windows",
                        "Group Policy",
                        "Machine"
                    );
                    Directory.CreateDirectory(registryPolPath); // Ensure the directory exists

                    string registryPolFilePath = Path.Combine(registryPolPath, "Registry.pol");

                    var startInfo = new ProcessStartInfo
                    {
                        FileName = lgpoExePath,
                        Arguments = $"/r \"{tempLgpoFile}\" /w \"{registryPolFilePath}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                    };

                    using (var proc = new Process { StartInfo = startInfo })
                    {
                        try
                        {
                            proc.Start();

                            string output = await proc.StandardOutput.ReadToEndAsync();
                            string error = await proc.StandardError.ReadToEndAsync();
                            proc.WaitForExit();

                            // Log the output and error streams
                            if (!string.IsNullOrEmpty(output))
                            {
                                Logger.Instance.WriteLog($"LGPO.exe Output: {output}");
                            }
                            if (!string.IsNullOrEmpty(error))
                            {
                                Logger.Instance.WriteLog($"LGPO.exe Error: {error}");
                            }

                            if (proc.ExitCode != 0)
                            {
                                Logger.Instance.WriteLog(
                                    $"LGPO.exe exited with code {proc.ExitCode}."
                                );
                                var errorBox = new WpfMessageBox
                                {
                                    Title = "LGPO.exe Error",
                                    Content =
                                        $"LGPO.exe encountered an error while processing Registry policies.\n\nExit Code: {proc.ExitCode}\n\nError: {error}\n\nPlease ensure that the LGPO text file is correctly formatted and that you have the necessary permissions to execute LGPO.exe.",
                                    CloseButtonText = "OK",
                                };
                                await errorBox.ShowDialogAsync();
                            }
                            else
                            {
                                Logger.Instance.WriteLog(
                                    "LGPO.exe executed successfully for Registry policies."
                                );
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Instance.WriteLog(
                                $"Exception while running LGPO.exe: {ex.Message}"
                            );
                            var exceptionBox = new WpfMessageBox
                            {
                                Title = "LGPO.exe Exception",
                                Content =
                                    $"An exception occurred while executing LGPO.exe: {ex.Message}",
                                CloseButtonText = "OK",
                            };
                            await exceptionBox.ShowDialogAsync();
                        }
                    }

                    // Clean up temporary LGPO file
                    if (File.Exists(tempLgpoFile))
                    {
                        try
                        {
                            File.Delete(tempLgpoFile);
                            Logger.Instance.WriteLog(
                                $"Temporary LGPO file deleted: {tempLgpoFile}"
                            );
                        }
                        catch (Exception ex)
                        {
                            Logger.Instance.WriteLog(
                                $"Failed to delete temporary LGPO file: {ex.Message}"
                            );
                        }
                    }
                }
            }

            // Store metadata
            if (!string.IsNullOrEmpty(gpoDescription))
            {
                File.WriteAllText(
                    Path.Combine(gpoFolderPath, "gpo_metadata.txt"),
                    $"Name: {gpoName}\nDescription: {gpoDescription}"
                );
                Logger.Instance.WriteLog(
                    $"GPO metadata written to {Path.Combine(gpoFolderPath, "gpo_metadata.txt")}"
                );
            }

            var successMsgBox = new WpfMessageBox
            {
                Title = "GPO Created",
                Content = $"GPO '{gpoName}' has been created successfully.",
                CloseButtonText = "OK",
            };
            await successMsgBox.ShowDialogAsync();
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
                if (
                    p.ValueConstraints?.RequiredValues != null
                    && p.ValueConstraints.RequiredValues.Count > 0
                )
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

        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar.Visibility = Visibility.Visible; // Show the progress bar

            try
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*",
                    Title = "Select PDF File to Import",
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string pdfFilePath = openFileDialog.FileName;

                    try
                    {
                        // Define the Python script and arguments
                        string pythonScript =
                            @"C:\Users\shrav\Desktop\pdf\Techolics_\data\script.py"; // Update this path
                        string arguments = $"\"{pythonScript}\" \"{pdfFilePath}\"";

                        var process = new System.Diagnostics.Process
                        {
                            StartInfo = new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = "python",
                                Arguments = arguments,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                UseShellExecute = false,
                                CreateNoWindow = true,
                            },
                        };

                        process.Start();

                        string output = await process.StandardOutput.ReadToEndAsync();
                        string error = await process.StandardError.ReadToEndAsync();
                        process.WaitForExit();

                        if (process.ExitCode != 0)
                        {
                            var messageBox = new WpfMessageBox
                            {
                                Title = "Error",
                                Content = $"Error processing PDF: {error}",
                                CloseButtonText = "OK",
                            };
                            await messageBox.ShowDialogAsync();
                            return;
                        }

                        string generatedXmlPath =
                            @"C:\Users\shrav\Desktop\pdf\Techolics_\data\policies.xml"; // Update this path
                        LoadGeneratedXml(generatedXmlPath);

                        var successMessageBox = new WpfMessageBox
                        {
                            Title = "Success",
                            Content = "PDF imported successfully!",
                            CloseButtonText = "OK",
                        };
                        await successMessageBox.ShowDialogAsync();
                    }
                    catch (Exception ex)
                    {
                        var errorBox = new WpfMessageBox
                        {
                            Title = "Error",
                            Content = $"An error occurred: {ex.Message}",
                            CloseButtonText = "OK",
                        };
                        await errorBox.ShowDialogAsync();
                    }
                }
            }
            finally
            {
                ProgressBar.Visibility = Visibility.Collapsed; // Hide the progress bar
            }
        }

        private void LoadGeneratedXml(string xmlPath)
        {
            // Logic to parse the XML and load it into the UI
            LoadGeneratedXmlFromPolicy(xmlPath);
        }

        private void LoadGeneratedXmlFromPolicy(string xmlPath)
        {
            // Parse the XML file
            var doc = XDocument.Load(xmlPath);

            // Example: Update TreeView or DataGrid based on the parsed XML
            foreach (var policy in doc.Descendants("Policy"))
            {
                var item = new Item
                {
                    ID = policy.Attribute("id")?.Value ?? "Unknown",
                    Name = policy.Element("Documentation")?.Element("Title")?.Value ?? "No Title",
                    Current = "Not Configured",
                    Status = "N/A",
                    ValueType = policy.Element("ValueType")?.Value ?? "String", // Ensure ValueType is set
                    // Populate other fields as needed
                };

                Items.Add(item); // Add to ObservableCollection
            }

            myDataGrid.Items.Refresh(); // Refresh the DataGrid
            Logger.Instance.WriteLog($"Loaded {Items.Count} policies from XML.");
        }

        private void ExportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Logic for exporting data
            // Implement as needed
        }

        private void myDataGrid_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            // Implement selection changed logic here if needed
        }

        #region Customize GPO Functionality

        private void CustomizeGPOButton_Click(object sender, RoutedEventArgs e)
        {
            // Toggle visibility of the CustomValue column
            if (CustomValueColumn.Visibility == Visibility.Collapsed)
            {
                CustomValueColumn.Visibility = Visibility.Visible;
                CustomizeGPOButton.Content = "Hide Customizations";
                Logger.Instance.WriteLog("CustomValue column is now visible for customization.");
            }
            else
            {
                CustomValueColumn.Visibility = Visibility.Collapsed;
                CustomizeGPOButton.Content = "Customize GPO";
                Logger.Instance.WriteLog("CustomValue column is now hidden.");
            }
        }

        private async void EditCustomValueButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is SystemButton editButton && editButton.Tag is Item policyItem)
            {
                var editWindow = new Techolics_.Pages.EditPolicyWindow(
                    this,
                    policyItem,
                    logic.GetBenchmarkValues(),
                    logic.GetBenchmarkDocumentation()
                );
                editWindow.Owner = this;
                bool? result = await Task.Run(() => editWindow.ShowDialog());

                if (result == true)
                {
                    myDataGrid.Items.Refresh();
                    Logger.Instance.WriteLog($"Custom value updated for policy: {policyItem.Name}");
                }
            }
        }

        // Ensure only numeric input for Integer TextBox using EventSetter
        private void IntegerTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only digits
            e.Handled = !IsTextNumeric(e.Text);
        }

        private bool IsTextNumeric(string text)
        {
            return Regex.IsMatch(text, @"^\d+$");
        }

        #endregion
    }
}
