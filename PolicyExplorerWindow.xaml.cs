using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Techolics_.PolicyManagement;

namespace Techolics_
{
    public partial class PolicyExplorerWindow : Window
    {
        private PolicyWindowLogic logic;
        private List<string> selectedProfiles;
        private string operation;

        // Add a DispatcherTimer to periodically refresh the logs
        private DispatcherTimer logRefreshTimer;

        public PolicyExplorerWindow(List<string> selectedProfiles, string operation)
        {
            InitializeComponent();

            this.selectedProfiles = selectedProfiles;
            this.operation = operation;
            this.Title = "Policy Explorer"; // Set the title

            logic = new PolicyWindowLogic(this, selectedProfiles, operation);

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
                string logFileName = "Techolics_Log.txt"; // Use the same name as in Logger.cs
                string logFilePath = Path.Combine(logDirectory, logFileName);

                if (File.Exists(logFilePath))
                {
                    string[] allLines = File.ReadAllLines(logFilePath);

                    // Clean and format the logs
                    var formattedLines = new List<string>();
                    foreach (var line in allLines)
                    {
                        // Example of cleaning: Remove file paths and line numbers
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
            // Remove file path, member name, and line number for readability
            int fileIndex = logLine.IndexOf("| File:");
            if (fileIndex >= 0)
            {
                logLine = logLine.Substring(0, fileIndex).Trim();
            }

            // Optionally, format the log line further if needed
            return logLine;
        }

        private void Window_Closed(object? sender, EventArgs e)
        {
            // Stop the timer when the window is closed
            logRefreshTimer.Stop();
        }

        private void AuditButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = logic.GetSelectedItems();
            if (selectedItems.Count == 0)
            {
                MessageBox.Show(
                    "Please select at least one policy.",
                    "No Policy Selected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }
            logic.StartAudit(selectedItems);
        }

        private void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = logic.GetSelectedItems();
            if (selectedItems.Count == 0)
            {
                MessageBox.Show(
                    "Please select at least one policy.",
                    "No Policy Selected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }
            logic.StartConfig(selectedItems);
        }

        private void RevertButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = logic.GetSelectedItems();
            if (selectedItems.Count == 0)
            {
                MessageBox.Show(
                    "Please select at least one policy to revert.",
                    "No Policy Selected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }
            logic.StartRevert(selectedItems);
        }

        private void SelectAllCheckBox_Click(object sender, RoutedEventArgs e)
        {
            bool newValue = (SelectAllCheckBox.IsChecked == true);
            var items = myDataGrid.ItemsSource as IEnumerable<Item>;
            if (items != null)
            {
                foreach (var item in items)
                {
                    item.IsSelected = newValue;
                }
                myDataGrid.Items.Refresh();
            }
        }

        private void AuditAllButton_Click(object sender, RoutedEventArgs e)
        {
            var allItems = logic.GetAllItems();
            if (allItems.Count == 0)
            {
                MessageBox.Show(
                    "No policies available to audit.",
                    "No Policies",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }
            logic.StartAudit(allItems);
        }

        private void ConfigAllButton_Click(object sender, RoutedEventArgs e)
        {
            var allItems = logic.GetAllItems();
            if (allItems.Count == 0)
            {
                MessageBox.Show(
                    "No policies available for configuration.",
                    "No Policies",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }
            logic.StartConfig(allItems);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            // Implement edit functionality if needed
            MessageBox.Show("Edit functionality is not implemented yet.");
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
