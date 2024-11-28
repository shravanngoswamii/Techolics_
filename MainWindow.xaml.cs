using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Techolics_
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadInitialData();
        }

        // Predefined data for each subfolder
        private readonly Dictionary<string, List<Item>> policyData = new Dictionary<string, List<Item>>
        {
            { "1.1", new List<Item>
                {
                    new Item { ID = "1", Name = "saf", Current = "af", Status = "PASS", Description = "This policy is used for password management." },
                    new Item { ID = "2", Name = "hui", Current = "af", Status = "PASS", Description = "This policy is used for auditing logins." },
                    new Item { ID = "3", Name = "safji", Current = "af", Status = "PASS", Description = "This policy helps ensure account security." },
                    new Item { ID = "4", Name = "safasa", Current = "af", Status = "PASS", Description = "This policy strengthens password rules." }
                }
            },
            { "1.2", new List<Item>
                {
                    new Item { ID = "1", Name = "abc", Current = "inactive", Status = "FAIL", Description = "This policy defines account lockout thresholds." },
                    new Item { ID = "2", Name = "xyz", Current = "active", Status = "PASS", Description = "This policy helps secure inactive accounts." }
                }
            },
            { "2.1", new List<Item>
                {
                    new Item { ID = "1", Name = "audit1", Current = "active", Status = "PASS", Description = "This policy ensures audit trails are maintained." },
                    new Item { ID = "2", Name = "audit2", Current = "inactive", Status = "FAIL", Description = "This policy focuses on user actions logging." }
                }
            },
            { "2.2", new List<Item>
                {
                    new Item { ID = "1", Name = "user1", Current = "active", Status = "PASS", Description = "This policy grants user rights for specific tasks." },
                    new Item { ID = "2", Name = "user2", Current = "inactive", Status = "FAIL", Description = "This policy manages user privileges." }
                }
            }
        };

        // Event handler for TreeView item selection
        private void PolicyTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem selectedItem)
            {
                // Update the "Logs" section with the current navigation path
                UpdateNavigationPath(selectedItem);

                if (selectedItem.Tag is string key && policyData.TryGetValue(key, out List<Item> items))
                {
                    // Update the DataGrid with the data associated with the selected item
                    myDataGrid.ItemsSource = items;
                }
                else
                {
                    myDataGrid.ItemsSource = null; // Clear the DataGrid if no data exists
                }
            }
        }

        // Event handler for DataGrid selection change
        private void MyDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Clear the details section
            detailsTextBlock.Text = "";

            // Iterate over all selected items
            foreach (var selectedItem in myDataGrid.SelectedItems)
            {
                if (selectedItem is Item item)
                {
                    detailsTextBlock.Text += $"ID: {item.ID}\n" +
                                             $"Name: {item.Name}\n" +
                                             $"Current: {item.Current}\n" +
                                             $"Status: {item.Status}\n" +
                                             $"Description: {item.Description}\n\n";
                }
            }

            // Show a default message if nothing is selected
            if (string.IsNullOrWhiteSpace(detailsTextBlock.Text))
            {
                detailsTextBlock.Text = "Select rows to view details.";
            }
        }


        // Update the navigation path displayed in the "Logs" section
        private void UpdateNavigationPath(TreeViewItem selectedItem)
        {
            // Build the navigation path by walking up the TreeViewItem hierarchy
            string path = selectedItem.Header.ToString();
            var parent = selectedItem.Parent as TreeViewItem;

            while (parent != null)
            {
                path = $"{parent.Header} > {path}";
                parent = parent.Parent as TreeViewItem;
            }

            // Update the Logs section dynamically
            navigationTextBlock.Text = path;
        }

        // Load initial dummy data into the DataGrid
        private void LoadInitialData()
        {
            myDataGrid.ItemsSource = new List<Item>
            {
                new Item { ID = "0", Name = "No Data", Current = "-", Status = "-", Description = "No description available." }
            };
        }
    }

    // Item class to represent data for the DataGrid
    public class Item
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Current { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
    }
}
