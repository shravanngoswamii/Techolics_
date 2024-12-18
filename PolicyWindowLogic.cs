// PolicyWindowLogic.cs
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Techolics_.Logging;
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
            benchmarkValues = dataLoader.LoadBenchmarkValues("CIS_Benchmark_Values.xml");
            benchmarkDocumentation = dataLoader.LoadBenchmarkDocumentation(
                "CIS_Benchmark_Documentation.xml"
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
            Logger.Instance.WriteLog($"Searching for documentation with Policy ID: {policyId}");
            var docPolicy = benchmarkDocumentation.Policies.FirstOrDefault(p => p.Id == policyId);
            if (docPolicy == null)
            {
                Logger.Instance.WriteLog($"No documentation found for Policy ID: {policyId}");
                return "No description available.";
            }
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
            // Clear existing details
            var container = (StackPanel)policyExplorer.FindName("DetailsContainer");
            container.Children.Clear();

            // Check if anything is selected
            if (policyExplorer.myDataGrid.SelectedItems.Count == 0)
            {
                // No selection, show default message
                container.Children.Add(
                    new TextBlock
                    {
                        Text = "Select rows to view details.",
                        FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                        FontSize = 14,
                        FontStyle = FontStyles.Italic,
                        HorizontalAlignment = HorizontalAlignment.Center,
                    }
                );
                return;
            }

            // Iterate over all selected items
            foreach (var selectedItem in policyExplorer.myDataGrid.SelectedItems)
            {
                if (selectedItem is Item item)
                {
                    var docPolicy = benchmarkDocumentation.Policies.FirstOrDefault(p =>
                        p.Id == item.ID
                    );

                    // Always show basic info
                    AddDetailBlock("ID", item.ID);
                    AddDetailBlock("Name", item.Name);
                    AddDetailBlock("Profile", item.Profile);
                    AddDetailBlock("Default Value", item.DefaultValue);
                    AddDetailBlock("Current", item.Current);
                    AddDetailBlock("Status", item.Status);

                    if (docPolicy != null && docPolicy.Documentation != null)
                    {
                        var doc = docPolicy.Documentation;

                        if (doc.Title != null)
                            AddDetailBlock("Title", doc.Title.Text.Trim());

                        if (doc.ProfileApplicability != null)
                            AddDetailBlock(
                                "Profile Applicability",
                                doc.ProfileApplicability.Text.Trim()
                            );

                        if (doc.Description != null)
                            AddDetailBlock("Description", doc.Description.Text.Trim());

                        if (doc.Rationale != null)
                            AddDetailBlock("Rationale", doc.Rationale.Text.Trim());

                        if (doc.Impact != null)
                            AddDetailBlock("Impact", doc.Impact.Text.Trim());

                        if (doc.Audit != null)
                            AddDetailBlock("Audit", doc.Audit.Text.Trim());

                        if (doc.Remediation != null)
                        {
                            AddDetailBlock("Remediation", doc.Remediation.Text.Trim());

                            if (doc.Remediation.CodeBlock?.Lines != null)
                            {
                                var codeLines = string.Join(
                                    Environment.NewLine,
                                    doc.Remediation.CodeBlock.Lines.Select(l => l.Trim())
                                );
                                AddDetailBlock(
                                    "Remediation Code Block",
                                    codeLines,
                                    isCopiable: true
                                );
                            }
                        }

                        if (doc.DefaultValue != null)
                            AddDetailBlock(
                                "Default Value (Documentation)",
                                doc.DefaultValue.Text.Trim()
                            );

                        if (doc.References?.ReferenceList != null)
                        {
                            // References as a custom block with hyperlinks
                            var referencesStackPanel = new StackPanel();
                            foreach (var reference in doc.References.ReferenceList)
                            {
                                var hyperlink = new Hyperlink
                                {
                                    NavigateUri = new Uri(reference.Url),
                                };
                                hyperlink.Inlines.Add(new Run(reference.Text.Trim()));
                                hyperlink.RequestNavigate += (s, ev) =>
                                {
                                    System.Diagnostics.Process.Start(
                                        new System.Diagnostics.ProcessStartInfo
                                        {
                                            FileName = ev.Uri.AbsoluteUri,
                                            UseShellExecute = true,
                                        }
                                    );
                                };

                                referencesStackPanel.Children.Add(
                                    new TextBlock(hyperlink)
                                    {
                                        Margin = new Thickness(0, 5, 0, 0),
                                        FontSize = 12,
                                    }
                                );
                            }

                            AddCustomBlock("References", referencesStackPanel);
                        }
                    }
                    else
                    {
                        // If no documentation is available
                        AddDetailBlock("Description", "Not available");
                    }
                }
            }

            // If container is empty (no details), show default message
            if (container.Children.Count == 0)
            {
                container.Children.Add(
                    new TextBlock
                    {
                        Text = "Select rows to view details.",
                        FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                        FontSize = 14,
                        FontStyle = FontStyles.Italic,
                        HorizontalAlignment = HorizontalAlignment.Center,
                    }
                );
            }
        }

        private void AddDetailBlock(string title, string content, bool isCopiable = false)
        {
            if (string.IsNullOrWhiteSpace(content))
                return;
            content = content.Trim();

            // Create a Border for the detail block with subtle styling
            var detailBlock = new Border
            {
                Margin = new Thickness(0, 0, 0, 0),
                Padding = new Thickness(10),
                CornerRadius = new CornerRadius(5),
                BorderThickness = new Thickness(1),
            };

            var outerStack = new StackPanel();

            // Top panel with title and optional copy button
            var titlePanel = new DockPanel
            {
                LastChildFill = false
            };

            // Title text
            var titleText = new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            };
            DockPanel.SetDock(titleText, Dock.Left);
            titlePanel.Children.Add(titleText);

            // If copiable, add a Copy button on the right
            if (isCopiable)
            {
                var copyButton = new Button
                {
                    Content = "Copy",
                    Margin = new Thickness(5, 0, 0, 0),
                    Padding = new Thickness(5, 0, 5, 0),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 12
                };

                // On click, copy the content to clipboard
                copyButton.Click += (s, e) =>
                {
                    Clipboard.SetText(content);
                };

                DockPanel.SetDock(copyButton, Dock.Right);
                titlePanel.Children.Add(copyButton);
            }

            outerStack.Children.Add(titlePanel);

            // Add content
            if (isCopiable)
            {
                // For copiable content, use a ScrollViewer + TextBox
                var scrollViewer = new ScrollViewer
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Margin = new Thickness(0, 2, 0, 0)
                };

                var textBox = new TextBox
                {
                    Text = content,
                    IsReadOnly = true,
                    TextWrapping = TextWrapping.Wrap,
                    Padding = new Thickness(5, 5, 5, 15),
                };

                scrollViewer.Content = textBox;
                outerStack.Children.Add(scrollViewer);
            }
            else
            {
                // For non-copiable content, just a regular TextBlock
                outerStack.Children.Add(
                    new TextBlock
                    {
                        Text = content,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 5, 0, 0),
                        FontFamily = new FontFamily("Consolas")
                    }
                );
            }

            detailBlock.Child = outerStack;
            policyExplorer.DetailsContainer.Children.Add(detailBlock);
        }

        private void AddCustomBlock(string title, UIElement contentElement)
        {
            var detailBlock = new Border
            {
                Margin = new Thickness(0, 5, 0, 5),
                Padding = new Thickness(10),
                CornerRadius = new CornerRadius(5),
                BorderThickness = new Thickness(1),
            };

            var stackPanel = new StackPanel();

            var titlePanel = new DockPanel
            {
                LastChildFill = false
            };

            // Title
            var titleText = new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            };
            DockPanel.SetDock(titleText, Dock.Left);
            titlePanel.Children.Add(titleText);

            stackPanel.Children.Add(titlePanel);

            stackPanel.Children.Add(contentElement);

            detailBlock.Child = stackPanel;
            policyExplorer.DetailsContainer.Children.Add(detailBlock);
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
            configurator.ConfigurePolicies(
                selectedItems,
                isRevert: false,
                fromEditWindow: fromEditWindow
            );
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
