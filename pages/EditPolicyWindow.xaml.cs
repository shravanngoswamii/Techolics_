using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Techolics_;
using Techolics_.PolicyManagement;
using Techolics_.Logging;
using System.Collections.Generic;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using WpfMessageBox = Wpf.Ui.Controls.MessageBox;
namespace Techolics_.Pages
{
    public partial class EditPolicyWindow : FluentWindow
    {
        private Item _policyItem;
        private Policy _policy;
        private CISBenchmark _benchmarkValues;
        private CISBenchmarkDocumentation _benchmarkDocumentation;
        private PolicyExplorerWindow _policyExplorerWindow; // reference to main window

        private List<string> _allSidUsers = new List<string>();

        public EditPolicyWindow(PolicyExplorerWindow explorer, Item policyItem, CISBenchmark benchmarkValues, CISBenchmarkDocumentation benchmarkDocumentation)
        {
            InitializeComponent();
            _policyItem = policyItem;
            _policy = policyItem.Policy!;
            _benchmarkValues = benchmarkValues;
            _benchmarkDocumentation = benchmarkDocumentation;
            _policyExplorerWindow = explorer;

            PolicyNameTextBlock.Text = _policy.Title;
            DefaultValueTextBlock.Text = _policyItem.DefaultValue;

            if (_policy.ValueConstraints?.RequiredValues != null && _policy.ValueConstraints.RequiredValues.Count > 0)
            {
                CISRecommendedTextBlock.Text = _policy.ValueConstraints.RequiredValues[0].Value;
            }
            else
            {
                CISRecommendedTextBlock.Text = "N/A";
            }

            SetupInputControls();

            if (string.Equals(_policy.ValueType, "String", StringComparison.OrdinalIgnoreCase))
            {
                _allSidUsers = GetAllSidFriendlyNames();
                foreach (var user in _allSidUsers)
                {
                    AvailableUsersListBox.Items.Add(user);
                }

                if (!string.IsNullOrEmpty(_policyItem.Current)
                    && !_policyItem.Current.Equals("Not Configured", StringComparison.OrdinalIgnoreCase)
                    && !_policyItem.Current.Equals("No one", StringComparison.OrdinalIgnoreCase))
                {
                    var currentParts = _policyItem.Current.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                          .Select(x => x.Trim()).ToList();
                    foreach (var part in currentParts)
                    {
                        if (AvailableUsersListBox.Items.Contains(part))
                        {
                            AvailableUsersListBox.Items.Remove(part);
                            SelectedUsersListBox.Items.Add(part);
                        }
                    }
                }
            }
            else if (string.Equals(_policy.ValueType, "Boolean", StringComparison.OrdinalIgnoreCase))
            {
                if (_policyItem.Current.Equals("Enabled", StringComparison.OrdinalIgnoreCase))
                {
                    BooleanEnabledRadio.IsChecked = true;
                }
                else
                {
                    BooleanDisabledRadio.IsChecked = true;
                }
            }
            else if (string.Equals(_policy.ValueType, "Integer", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(_policyItem.Current, out int val))
                {
                    IntegerTextBox.Text = val.ToString();
                }
            }
        }

        private void SetupInputControls()
        {
            IntegerPanel.Visibility = Visibility.Collapsed;
            BooleanPanel.Visibility = Visibility.Collapsed;
            StringPanel.Visibility = Visibility.Collapsed;

            var type = _policy.ValueType;
            if (string.Equals(type, "Integer", StringComparison.OrdinalIgnoreCase))
            {
                IntegerPanel.Visibility = Visibility.Visible;
            }
            else if (string.Equals(type, "Boolean", StringComparison.OrdinalIgnoreCase))
            {
                BooleanPanel.Visibility = Visibility.Visible;
            }
            else if (string.Equals(type, "String", StringComparison.OrdinalIgnoreCase))
            {
                StringPanel.Visibility = Visibility.Visible;
            }
            else
            {
                StringPanel.Visibility = Visibility.Visible;
            }
        }

        private List<string> GetAllSidFriendlyNames()
        {
            return PolicyValueConverter.GetAllFriendlyNames();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private async void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            string? newValue = await GetNewValueFromControls();
            if (newValue == null)
            {
                return;
            }

            // Store user's chosen value in Current
            _policyItem.Current = newValue;

            var configurator = new PolicyConfigurator(_policyExplorerWindow, _benchmarkValues, _benchmarkDocumentation);
            try
            {
                // Configure using the user-provided value in item.Current
                configurator.ConfigurePolicies(new List<Item> { _policyItem }, isRevert: false, fromEditWindow: true);
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteLog($"Error configuring policy {_policyItem.ID}: {ex.Message}");
            }

            var auditor = new PolicyAuditor(_policyExplorerWindow, _benchmarkValues, _benchmarkDocumentation);
            auditor.AuditPolicies(new List<Item> { _policyItem });

            DialogResult = true;
            this.Close();
        }


        private async Task<string?> GetNewValueFromControls()
        {
            var type = _policy.ValueType;

            if (string.Equals(type, "Integer", StringComparison.OrdinalIgnoreCase))
            {
                if (!int.TryParse(IntegerTextBox.Text.Trim(), out int val))
                {
                    await ShowMessageBoxAsync("Invalid Input", "Please enter a valid integer value.");
                    return null;
                }
                return val.ToString();
            }
            else if (string.Equals(type, "Boolean", StringComparison.OrdinalIgnoreCase))
            {
                // Return "Enabled" or "Disabled" based on the radio button state
                return BooleanEnabledRadio.IsChecked == true ? "Enabled" : "Disabled";
            }
            else if (string.Equals(type, "String", StringComparison.OrdinalIgnoreCase))
            {
                return GetSelectedItemsAsString() ?? "No one";
            }

            // Default case (fallback)
            return GetSelectedItemsAsString() ?? "No one";
        }

        // Helper method to retrieve selected items from a ListBox
        private string? GetSelectedItemsAsString()
        {
            var selected = SelectedUsersListBox.Items.Cast<object>()
                .Select(item => item.ToString())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .ToList();

            return selected.Count > 0 ? string.Join(", ", selected) : null;
        }

        // Helper method to show a message box
        private async Task ShowMessageBoxAsync(string title, string content)
        {
            var messageBox = new WpfMessageBox
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK",
            };
            await messageBox.ShowDialogAsync();
        }


        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = AvailableUsersListBox.SelectedItems.Cast<object>().ToList();
            foreach (var item in selectedItems)
            {
                AvailableUsersListBox.Items.Remove(item);
                SelectedUsersListBox.Items.Add(item);
            }
        }

        private void RemoveUserButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = SelectedUsersListBox.SelectedItems.Cast<object>().ToList();
            foreach (var item in selectedItems)
            {
                SelectedUsersListBox.Items.Remove(item);
                AvailableUsersListBox.Items.Add(item);
            }
        }
    }
}
