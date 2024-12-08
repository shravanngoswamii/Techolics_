// PolicyExplorerWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Techolics_.Logging;
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

        // Use ObservableCollection for dynamic updates
        public ObservableCollection<Item> Items { get; set; } = new ObservableCollection<Item>();

        public PolicyExplorerWindow(List<string> selectedProfiles, string operation)
        {
            InitializeComponent();

            this.selectedProfiles = selectedProfiles;
            this.operation = operation;
            this.Title = "Policy Explorer"; // Set the title

            logic = new PolicyWindowLogic(this, selectedProfiles, operation);

            // Set the DataGrid's ItemsSource to the ObservableCollection
            myDataGrid.ItemsSource = Items;

            // Hook up event handlers
            policyTreeView.SelectedItemChanged += logic.PolicyTreeView_SelectedItemChanged;

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

        private void AuditButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = logic.GetSelectedItems();
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Please select at least one policy.", "No Policy Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            logic.StartAudit(selectedItems);
        }

        private void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = logic.GetSelectedItems();
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Please select at least one policy.", "No Policy Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Pass fromEditWindow: false
            logic.StartConfig(selectedItems, fromEditWindow: false);
        }

        private void RevertButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = logic.GetSelectedItems();
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Please select at least one policy to revert.", "No Policy Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            logic.StartRevert(selectedItems);
        }

        private void SelectAllCheckBox_Click(object sender, RoutedEventArgs e)
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
                MessageBox.Show("An error occurred while selecting all policies.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AuditAllButton_Click(object sender, RoutedEventArgs e)
        {
            var allItems = logic.GetAllItems();
            if (allItems.Count == 0)
            {
                MessageBox.Show("No policies available to audit.", "No Policies", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            logic.StartAudit(allItems);
        }

        private void ConfigAllButton_Click(object sender, RoutedEventArgs e)
        {
            var allItems = logic.GetAllItems();
            if (allItems.Count == 0)
            {
                MessageBox.Show("No policies available for configuration.", "No Policies", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Pass fromEditWindow: false
            logic.StartConfig(allItems, fromEditWindow: false);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = logic.GetSelectedItems();

            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Please select a policy to edit.", "No Policy Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (selectedItems.Count > 1)
            {
                MessageBox.Show("Please select only one policy at a time to edit.", "Multiple Policies Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
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
    }
}
