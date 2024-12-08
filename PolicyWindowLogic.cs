// PolicyWindowLogic.cs
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Techolics_.PolicyManagement;

namespace Techolics_
{
    public class PolicyWindowLogic
    {
        public CISBenchmark GetBenchmarkValues() => benchmarkValues;
        public CISBenchmarkDocumentation GetBenchmarkDocumentation() => benchmarkDocumentation;

        private CISBenchmark benchmarkValues;
        private CISBenchmarkDocumentation benchmarkDocumentation;
        private PolicyExplorerWindow policyExplorer;
        private DataLoader dataLoader;
        private List<string> selectedProfiles;
        private string operation;

        public PolicyWindowLogic(
            PolicyExplorerWindow policyExplorer,
            List<string> selectedProfiles,
            string operation
        )
        {
            this.policyExplorer = policyExplorer;
            this.selectedProfiles = selectedProfiles;
            this.operation = operation;

            // Load data from XML files
            dataLoader = new DataLoader();
            benchmarkValues = dataLoader.LoadBenchmarkValues("data/CIS_Benchmark_Values.xaml");
            benchmarkDocumentation = dataLoader.LoadBenchmarkDocumentation(
                "data/CIS_Benchmark_Documentation.xaml"
            );

            // Populate the TreeView
            PopulatePolicyTreeView();

            // Load all policies into the DataGrid
            LoadAllPolicies();

            // Select the root item in the TreeView
            if (policyExplorer.policyTreeView.Items.Count > 0)
            {
                var rootItem = policyExplorer.policyTreeView.Items[0] as TreeViewItem;
                if (rootItem != null)
                {
                    rootItem.IsSelected = true;
                }
            }
        }

        // Method to populate the TreeView
        public void PopulatePolicyTreeView()
        {
            TreeViewItem rootItem = new TreeViewItem
            {
                Header = "Policy Explorer",
                Tag = "root",
                IsExpanded = true,
            };

            foreach (var section in benchmarkValues.Sections)
            {
                TreeViewItem sectionItem = new TreeViewItem
                {
                    Header = section.Title,
                    Tag = section.Id,
                };

                AddSectionsAndPolicies(sectionItem, section);

                rootItem.Items.Add(sectionItem);
            }

            policyExplorer.policyTreeView.Items.Add(rootItem);
        }

        // Recursive method to add sections and policies to the TreeView
        private void AddSectionsAndPolicies(TreeViewItem parentItem, Section section)
        {
            // Add sub-sections
            if (section.SubSections != null)
            {
                foreach (var subSection in section.SubSections)
                {
                    TreeViewItem subSectionItem = new TreeViewItem
                    {
                        Header = subSection.Title,
                        Tag = subSection.Id,
                    };

                    AddSectionsAndPolicies(subSectionItem, subSection);

                    parentItem.Items.Add(subSectionItem);
                }
            }

            // Add policies
            if (section.Policies != null)
            {
                foreach (var policy in section.Policies)
                {
                    TreeViewItem policyItem = new TreeViewItem
                    {
                        Header = policy.Title,
                        Tag = policy.Id,
                    };

                    parentItem.Items.Add(policyItem);
                }
            }
        }

        // Event handler for TreeView's SelectedItemChanged event
        public void PolicyTreeView_SelectedItemChanged(
            object sender,
            RoutedPropertyChangedEventArgs<object> e
        )
        {
            if (e.NewValue is TreeViewItem selectedItem)
            {
                // Update the navigation path
                UpdateNavigationPath(selectedItem);

                if (selectedItem.Tag is string key)
                {
                    if (key == "root")
                    {
                        // Display all policies
                        var items = GetAllPoliciesForSelectedProfiles();
                        // Clear existing items and add new ones
                        policyExplorer.Items.Clear();
                        foreach (var item in items)
                        {
                            policyExplorer.Items.Add(item);
                        }
                    }
                    else
                    {
                        var items = GetPoliciesBySectionOrPolicyId(key);

                        if (items != null && items.Count > 0)
                        {
                            // Clear existing items and add new ones
                            policyExplorer.Items.Clear();
                            foreach (var item in items)
                            {
                                policyExplorer.Items.Add(item);
                            }
                        }
                        else
                        {
                            policyExplorer.Items.Clear();
                        }
                    }
                }
            }
        }

        // Method to load all policies into the DataGrid
        private void LoadAllPolicies()
        {
            // Get all policies for the selected profiles
            var items = GetAllPoliciesForSelectedProfiles();

            // Clear existing items and add new ones
            policyExplorer.Items.Clear();
            foreach (var item in items)
            {
                policyExplorer.Items.Add(item);
            }
        }

        // Method to get all policies for selected profiles
        private List<Item> GetAllPoliciesForSelectedProfiles()
        {
            var items = new List<Item>();

            // Get all policies in the benchmark
            var allPolicies = GetAllPoliciesInSections(benchmarkValues.Sections);

            // Filter policies by selected profiles
            var filteredPolicies = allPolicies
                .Where(policy => selectedProfiles.Contains(policy.Profile))
                .ToList();

            foreach (var policy in filteredPolicies)
            {
                items.Add(
                    new Item
                    {
                        ID = policy.Id,
                        Profile = policy.Profile,
                        Name = policy.Title,
                        Current = "N/A",
                        Status = "N/A",
                        Description = GetPolicyDescription(policy.Id),
                        DefaultValue = GetDefaultValueText(policy),
                        Policy = policy,
                    }
                );
            }

            return items;
        }

        // Recursive method to get all policies in sections
        private List<Policy> GetAllPoliciesInSections(List<Section> sections)
        {
            var policies = new List<Policy>();

            foreach (var section in sections)
            {
                if (section.Policies != null)
                    policies.AddRange(section.Policies);

                if (section.SubSections != null)
                    policies.AddRange(GetAllPoliciesInSections(section.SubSections));
            }

            return policies;
        }

        // Method to get policies by section ID or policy ID
        private List<Item> GetPoliciesBySectionOrPolicyId(string id)
        {
            var items = new List<Item>();

            // Search in Sections
            var section = FindSectionById(benchmarkValues.Sections, id);

            if (section != null)
            {
                // Collect policies in this section and its subsections
                var policies = GetAllPoliciesInSection(section);
                foreach (var policy in policies)
                {
                    items.Add(
                        new Item
                        {
                            ID = policy.Id,
                            Profile = policy.Profile,
                            Name = policy.Title,
                            Current = "N/A",
                            Status = "N/A",
                            Description = GetPolicyDescription(policy.Id),
                            DefaultValue = GetDefaultValueText(policy),
                            Policy = policy,
                        }
                    );
                }
            }
            else
            {
                // Search for a single policy
                var policy = FindPolicyById(benchmarkValues.Sections, id);
                if (policy != null)
                {
                    items.Add(
                        new Item
                        {
                            ID = policy.Id,
                            Profile = policy.Profile,
                            Name = policy.Title,
                            Current = "N/A",
                            Status = "N/A",
                            Description = GetPolicyDescription(policy.Id),
                            DefaultValue = GetDefaultValueText(policy),
                            Policy = policy,
                        }
                    );
                }
            }
            items = items.Where(item => selectedProfiles.Contains(item.Profile)).ToList();
            return items;
        }

        // Recursive method to find a section by ID
        private Section? FindSectionById(List<Section> sections, string id)
        {
            foreach (var section in sections)
            {
                if (section.Id == id)
                    return section;

                if (section.SubSections != null)
                {
                    var found = FindSectionById(section.SubSections, id);
                    if (found != null)
                        return found;
                }
            }
            return null;
        }

        // Recursive method to find a policy by ID
        private Policy? FindPolicyById(List<Section> sections, string id)
        {
            foreach (var section in sections)
            {
                if (section.Policies != null)
                {
                    foreach (var policy in section.Policies)
                    {
                        if (policy.Id == id)
                            return policy;
                    }
                }

                if (section.SubSections != null)
                {
                    var found = FindPolicyById(section.SubSections, id);
                    if (found != null)
                        return found;
                }
            }
            return null;
        }

        // Recursive method to get all policies in a section
        private List<Policy> GetAllPoliciesInSection(Section section)
        {
            var policies = new List<Policy>();

            if (section.Policies != null)
                policies.AddRange(section.Policies);

            if (section.SubSections != null)
            {
                foreach (var subSection in section.SubSections)
                {
                    policies.AddRange(GetAllPoliciesInSection(subSection));
                }
            }

            return policies;
        }

        // Method to get the policy description
        private string GetPolicyDescription(string policyId)
        {
            var docPolicy = benchmarkDocumentation.Policies.FirstOrDefault(p => p.Id == policyId);
            return docPolicy?.Documentation?.Description?.Text ?? "No description available.";
        }

        // Method to get the default value text for a policy
        private string GetDefaultValueText(Policy policy)
        {
            if (policy.DefaultValue != null)
            {
                if (!string.IsNullOrEmpty(policy.DefaultValue.Value))
                {
                    return $"Global: {policy.DefaultValue.Value}";
                }
                else
                {
                    string domainValue = policy.DefaultValue.Domain ?? "N/A";
                    string standaloneValue = policy.DefaultValue.Standalone ?? "N/A";
                    return $"Domain: {domainValue}, Standalone: {standaloneValue}";
                }
            }
            return "N/A";
        }

        // Event handler for DataGrid's SelectionChanged event (Optional)
        public void MyDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Clear the details section
            policyExplorer.detailsTextBlock.Text = "";

            // Iterate over all selected items
            foreach (var selectedItem in policyExplorer.myDataGrid.SelectedItems)
            {
                if (selectedItem is Item item)
                {
                    var docPolicy = benchmarkDocumentation.Policies.FirstOrDefault(p =>
                        p.Id == item.ID
                    );

                    if (docPolicy != null && docPolicy.Documentation != null)
                    {
                        var doc = docPolicy.Documentation;

                        // Use a StringBuilder for efficient string concatenation
                        var sb = new System.Text.StringBuilder();

                        sb.AppendLine($"ID: {item.ID}");
                        sb.AppendLine($"Name: {item.Name}");
                        sb.AppendLine($"Profile: {item.Profile}");
                        sb.AppendLine($"Default Value: {item.DefaultValue}");
                        sb.AppendLine($"Current: {item.Current}");
                        sb.AppendLine($"Status: {item.Status}");
                        sb.AppendLine();

                        if (doc.Title != null)
                        {
                            sb.AppendLine($"Title: {doc.Title.Text.Trim()}");
                            sb.AppendLine();
                        }

                        if (doc.ProfileApplicability != null)
                        {
                            sb.AppendLine(
                                $"Profile Applicability: {doc.ProfileApplicability.Text.Trim()}"
                            );
                            sb.AppendLine();
                        }

                        if (doc.Description != null)
                        {
                            sb.AppendLine($"Description: {doc.Description.Text.Trim()}");
                            sb.AppendLine();
                        }

                        if (doc.Rationale != null)
                        {
                            sb.AppendLine($"Rationale: {doc.Rationale.Text.Trim()}");
                            sb.AppendLine();
                        }

                        if (doc.Impact != null)
                        {
                            sb.AppendLine($"Impact: {doc.Impact.Text.Trim()}");
                            sb.AppendLine();
                        }

                        if (doc.Audit != null)
                        {
                            sb.AppendLine($"Audit: {doc.Audit.Text.Trim()}");
                            sb.AppendLine();
                        }

                        if (doc.Remediation != null)
                        {
                            sb.AppendLine($"Remediation: {doc.Remediation.Text.Trim()}");
                            if (
                                doc.Remediation.CodeBlock != null
                                && doc.Remediation.CodeBlock.Lines != null
                            )
                            {
                                sb.AppendLine("Code Block:");
                                foreach (var line in doc.Remediation.CodeBlock.Lines)
                                {
                                    sb.AppendLine(line.Trim());
                                }
                            }
                            sb.AppendLine();
                        }

                        if (doc.DefaultValue != null)
                        {
                            sb.AppendLine(
                                $"Default Value (Documentation): {doc.DefaultValue.Text.Trim()}"
                            );
                            sb.AppendLine();
                        }

                        if (doc.References != null && doc.References.ReferenceList != null)
                        {
                            sb.AppendLine("References:");
                            foreach (var reference in doc.References.ReferenceList)
                            {
                                sb.AppendLine($"- {reference.Text.Trim()} ({reference.Url})");
                            }
                            sb.AppendLine();
                        }

                        policyExplorer.detailsTextBlock.Text += sb.ToString();
                    }
                    else
                    {
                        policyExplorer.detailsTextBlock.Text +=
                            $"ID: {item.ID}\n"
                            + $"Name: {item.Name}\n"
                            + $"Profile: {item.Profile}\n"
                            + $"Default Value: {item.DefaultValue}\n"
                            + $"Current: {item.Current}\n"
                            + $"Status: {item.Status}\n"
                            + $"Description: Not available\n\n";
                    }
                }
            }

            // Show a default message if nothing is selected
            if (string.IsNullOrWhiteSpace(policyExplorer.detailsTextBlock.Text))
            {
                policyExplorer.detailsTextBlock.Text = "Select rows to view details.";
            }
        }

        // Method to update the navigation path
        private void UpdateNavigationPath(TreeViewItem selectedItem)
        {
            // Build the navigation path by walking up the TreeViewItem hierarchy
            string path = selectedItem.Header?.ToString() ?? "Unknown";
            var parent = GetParentTreeViewItem(selectedItem);

            while (parent != null)
            {
                path = $"{parent.Header} > {path}";
                parent = GetParentTreeViewItem(parent);
            }

            // Update the navigationTextBlock
            policyExplorer.navigationTextBlock.Text = path;
        }

        // Helper method to get parent TreeViewItem
        private TreeViewItem? GetParentTreeViewItem(DependencyObject item)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(item);

            while (parent != null && !(parent is TreeViewItem))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as TreeViewItem;
        }

        // Method to get all items from DataGrid
        public List<Item> GetAllItems()
        {
            var items = policyExplorer.Items;
            return items.ToList();
        }

        // Method to get selected items from DataGrid
        public List<Item> GetSelectedItems()
        {
            return policyExplorer.Items.Where(item => item.IsSelected).ToList();
        }

        /// <summary>
        /// Starts the audit process on selected items.
        /// </summary>
        public void StartAudit(List<Item> selectedItems)
        {
            var auditor = new PolicyAuditor(
                policyExplorer,
                benchmarkValues,
                benchmarkDocumentation
            );
            auditor.AuditPolicies(selectedItems);
        }

        /// <summary>
        /// Starts the configuration process on selected items.
        /// </summary>
        /// <param name="fromEditWindow">Indicates if the configuration is initiated from the Edit Policy window.</param>
        public void StartConfig(List<Item> selectedItems, bool fromEditWindow = false)
        {
            var configurator = new PolicyConfigurator(
                policyExplorer,
                benchmarkValues,
                benchmarkDocumentation
            );
            configurator.ConfigurePolicies(selectedItems, isRevert: false, fromEditWindow: fromEditWindow);
        }

        /// <summary>
        /// Starts the revert process on selected items.
        /// </summary>
        public void StartRevert(List<Item> selectedItems)
        {
            var configurator = new PolicyConfigurator(
                policyExplorer,
                benchmarkValues,
                benchmarkDocumentation
            );
            configurator.ConfigurePolicies(selectedItems, isRevert: true, fromEditWindow: false);
        }
    }
}
