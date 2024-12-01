using System.Collections.Generic;
using System.Windows;

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
            this.Title = $"{operation} - Policy Explorer";

            logic = new PolicyWindowLogic(this, selectedProfiles, operation);

            // Hook up event handlers
            policyTreeView.SelectedItemChanged += logic.PolicyTreeView_SelectedItemChanged;
            myDataGrid.SelectionChanged += logic.MyDataGrid_SelectionChanged;

            // Control button visibility based on operation
            if (operation == "Audit")
            {
                ConfigButton.Visibility = Visibility.Collapsed;
                ConfigAllButton.Visibility = Visibility.Collapsed;
            }
            else if (operation == "Config")
            {
                AuditButton.Visibility = Visibility.Collapsed;
            }
        }

        private void AuditButton_Click(object sender, RoutedEventArgs e)
        {
            logic.StartAudit();
        }

        private void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
            logic.StartConfig(false); // Configure selected policies
        }

        private void ConfigAllButton_Click(object sender, RoutedEventArgs e)
        {
            logic.StartConfig(true); // Configure all policies in the DataGrid
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            // Implement edit functionality if needed
            MessageBox.Show("Edit functionality is not implemented yet.");
        }
    }
}
