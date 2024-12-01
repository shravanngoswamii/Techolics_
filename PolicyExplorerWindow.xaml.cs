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
        }

        private void AuditButton_Click(object sender, RoutedEventArgs e)
        {
            logic.StartAudit();
        }
    }
}
