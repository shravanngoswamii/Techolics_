using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Techolics_;
using Techolics_.PolicyManagement;
using Techolics_.Logging;
using System.Collections.Generic;

namespace Techolics_.Pages
{
    public partial class EditPolicyWindow : Window
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

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            string? newValue = GetNewValueFromControls();
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

        private string? GetNewValueFromControls()
        {
            var type = _policy.ValueType;
            if (string.Equals(type, "Integer", StringComparison.OrdinalIgnoreCase))
            {
                if (!int.TryParse(IntegerTextBox.Text.Trim(), out int val))
                {
                    MessageBox.Show("Please enter a valid integer value.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return null;
                }
                return val.ToString();
            }
            else if (string.Equals(type, "Boolean", StringComparison.OrdinalIgnoreCase))
            {
                return BooleanEnabledRadio.IsChecked == true ? "Enabled" : "Disabled";
            }
            else if (string.Equals(type, "String", StringComparison.OrdinalIgnoreCase))
            {
                var selected = new List<string>();
                foreach (var item in SelectedUsersListBox.Items)
                {
                    selected.Add(item.ToString()!);
                }

                if (selected.Count == 0)
                {
                    return "No one";
                }

                return string.Join(", ", selected);
            }
            else
            {
                var selected = new List<string>();
                foreach (var item in SelectedUsersListBox.Items)
                {
                    selected.Add(item.ToString()!);
                }

                return selected.Count == 0 ? "No one" : string.Join(", ", selected);
            }
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
