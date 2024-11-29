using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Techolics_
{
    public partial class MainWindow : Window
    {
        private CISBenchmark benchmarkValues;
        private CISBenchmarkDocumentation benchmarkDocumentation;

        public MainWindow()
        {
            InitializeComponent();

            // Load data from XAML files
            benchmarkValues = LoadBenchmarkValues("data/CIS_Benchmark_Values.xaml");
            benchmarkDocumentation = LoadBenchmarkDocumentation(
                "data/CIS_Benchmark_Documentation.xaml"
            );

            // Populate the TreeView
            PopulatePolicyTreeView();
        }

        // Method to load CIS_Benchmark_Values.xaml
        private CISBenchmark LoadBenchmarkValues(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(CISBenchmark));
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                var result = serializer.Deserialize(fs) as CISBenchmark;
                if (result == null)
                {
                    throw new InvalidOperationException("Deserialization returned null.");
                }
                return result;
            }
        }

        // Method to load CIS_Benchmark_Documentation.xaml
        private CISBenchmarkDocumentation LoadBenchmarkDocumentation(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(CISBenchmarkDocumentation));
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                var result = serializer.Deserialize(fs) as CISBenchmarkDocumentation;
                if (result == null)
                {
                    throw new InvalidOperationException("Deserialization returned null.");
                }
                return result;
            }
        }

        private void PopulatePolicyTreeView()
        {
            foreach (var section in benchmarkValues.Sections)
            {
                TreeViewItem sectionItem = new TreeViewItem
                {
                    Header = section.Title,
                    Tag = section.Id,
                };

                AddSectionsAndPolicies(sectionItem, section);

                policyTreeView.Items.Add(sectionItem);
            }
        }

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

        private void PolicyTreeView_SelectedItemChanged(
            object sender,
            RoutedPropertyChangedEventArgs<object> e
        )
        {
            if (e.NewValue is TreeViewItem selectedItem)
            {
                // Update the "Logs" section with the current navigation path
                UpdateNavigationPath(selectedItem);

                if (selectedItem.Tag is string key)
                {
                    var items = GetPoliciesBySectionOrPolicyId(key);

                    // Update the DataGrid with the data associated with the selected item
                    if (items != null && items.Count > 0)
                    {
                        myDataGrid.ItemsSource = items;
                    }
                    else
                    {
                        myDataGrid.ItemsSource = null; // Clear the DataGrid if no data exists
                    }
                }
            }
        }

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
                            Name = policy.Title,
                            Current = "N/A", // Set to "N/A" for now
                            Status = "N/A", // Set to "N/A" for now
                            Description = GetPolicyDescription(policy.Id),
                            DefaultValue = policy.DefaultValue?.Value ?? "N/A", // Store DefaultValue
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
                            Name = policy.Title,
                            Current = "N/A",
                            Status = "N/A",
                            Description = GetPolicyDescription(policy.Id),
                            DefaultValue = policy.DefaultValue?.Value ?? "N/A",
                        }
                    );
                }
            }

            return items;
        }

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

        private string GetPolicyDescription(string policyId)
        {
            var docPolicy = benchmarkDocumentation.Policies.FirstOrDefault(p => p.Id == policyId);
            return docPolicy?.Documentation?.Description?.Text ?? "No description available.";
        }

        private void MyDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Clear the details section
            detailsTextBlock.Text = "";

            // Iterate over all selected items
            foreach (var selectedItem in myDataGrid.SelectedItems)
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

                        detailsTextBlock.Text += sb.ToString();
                    }
                    else
                    {
                        detailsTextBlock.Text +=
                            $"ID: {item.ID}\n"
                            + $"Name: {item.Name}\n"
                            + $"Default Value: {item.DefaultValue}\n"
                            + $"Current: {item.Current}\n"
                            + $"Status: {item.Status}\n"
                            + $"Description: Not available\n\n";
                    }
                }
            }

            // Show a default message if nothing is selected
            if (string.IsNullOrWhiteSpace(detailsTextBlock.Text))
            {
                detailsTextBlock.Text = "Select rows to view details.";
            }
        }

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
            navigationTextBlock.Text = path;
        }


        private TreeViewItem? GetParentTreeViewItem(DependencyObject item)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(item);

            while (parent != null && !(parent is TreeViewItem))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as TreeViewItem;
        }
    }

    // Data classes

    [XmlRoot("CIS_Benchmark")]
    public class CISBenchmark
    {
        [XmlElement("Section")]
        public List<Section> Sections { get; set; } = new List<Section>();
    }

    public class Section
    {
        [XmlAttribute("id")]
        public string Id { get; set; } = "";

        [XmlAttribute("title")]
        public string Title { get; set; } = "";

        [XmlElement("Section")]
        public List<Section>? SubSections { get; set; }

        [XmlElement("Policy")]
        public List<Policy>? Policies { get; set; }
    }

    public class Policy
    {
        [XmlAttribute("id")]
        public string Id { get; set; } = "";

        [XmlAttribute("mode")]
        public string Mode { get; set; } = "";

        [XmlAttribute("profile")]
        public string Profile { get; set; } = "";

        [XmlAttribute("title")]
        public string Title { get; set; } = "";

        [XmlAttribute("value_type")]
        public string ValueType { get; set; } = "";

        [XmlElement("DefaultValue")]
        public DefaultValue? DefaultValue { get; set; }

        [XmlElement("ValueConstraints")]
        public ValueConstraints? ValueConstraints { get; set; }
    }

    public class DefaultValue
    {
        [XmlAttribute("domain")]
        public string? Domain { get; set; }

        [XmlAttribute("standalone")]
        public string? Standalone { get; set; }

        [XmlAttribute("value")]
        public string? Value { get; set; }
    }

    public class ValueConstraints
    {
        [XmlElement("RequiredValue")]
        public List<RequiredValue>? RequiredValues { get; set; }
    }

    public class RequiredValue
    {
        [XmlAttribute("operator")]
        public string Operator { get; set; } = "";

        [XmlAttribute("value")]
        public string Value { get; set; } = "";
    }

    [XmlRoot("CIS_Benchmark_Documentation")]
    public class CISBenchmarkDocumentation
    {
        [XmlElement("Policy")]
        public List<DocumentationPolicy> Policies { get; set; } = new List<DocumentationPolicy>();
    }

    public class DocumentationPolicy
    {
        [XmlAttribute("id")]
        public string Id { get; set; } = "";

        [XmlElement("Documentation")]
        public Documentation? Documentation { get; set; }
    }

    public class Documentation
    {
        [XmlElement("Title")]
        public Title? Title { get; set; }

        [XmlElement("ProfileApplicability")]
        public ProfileApplicability? ProfileApplicability { get; set; }

        [XmlElement("Description")]
        public Description? Description { get; set; }

        [XmlElement("Rationale")]
        public Rationale? Rationale { get; set; }

        [XmlElement("Impact")]
        public Impact? Impact { get; set; }

        [XmlElement("Audit")]
        public Audit? Audit { get; set; }

        [XmlElement("Remediation")]
        public Remediation? Remediation { get; set; }

        [XmlElement("DefaultValue")]
        public DefaultValueText? DefaultValue { get; set; }

        [XmlElement("References")]
        public References? References { get; set; }
    }

    public class Title
    {
        [XmlText]
        public string Text { get; set; } = "";
    }

    public class ProfileApplicability
    {
        [XmlElement("Text")]
        public string Text { get; set; } = "";
    }

    public class Description
    {
        [XmlElement("Text")]
        public string Text { get; set; } = "";
    }

    public class Rationale
    {
        [XmlElement("Text")]
        public string Text { get; set; } = "";
    }

    public class Impact
    {
        [XmlElement("Text")]
        public string Text { get; set; } = "";
    }

    public class Audit
    {
        [XmlElement("Text")]
        public string Text { get; set; } = "";
    }

    public class Remediation
    {
        [XmlElement("Text")]
        public string Text { get; set; } = "";

        [XmlElement("CodeBlock")]
        public CodeBlock? CodeBlock { get; set; }
    }

    public class CodeBlock
    {
        [XmlElement("Line")]
        public List<string>? Lines { get; set; }
    }

    public class DefaultValueText
    {
        [XmlElement("Text")]
        public string Text { get; set; } = "";
    }

    public class References
    {
        [XmlElement("Reference")]
        public List<Reference>? ReferenceList { get; set; }
    }

    public class Reference
    {
        [XmlAttribute("url")]
        public string Url { get; set; } = "";

        [XmlText]
        public string Text { get; set; } = "";
    }

    // Item class to represent data for the DataGrid
    public class Item
    {
        public string ID { get; set; } = "";
        public string Name { get; set; } = "";
        public string Current { get; set; } = "";
        public string Status { get; set; } = "";
        public string Description { get; set; } = "";
        public string DefaultValue { get; set; } = ""; // Added property
    }
}
