using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Techolics_
{
    public partial class PolicyExplorerWindow : Window
    {
        private PolicyWindowLogic logic;
        private List<string> selectedProfiles;
        private string operation;

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
    }
}
