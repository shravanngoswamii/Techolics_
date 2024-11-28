using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


using System.Collections.Generic;
using System.Windows;

namespace Techolics_
    {
        public partial class MainWindow : Window
        {
            public MainWindow()
            {
                InitializeComponent();
                // Load sample data into the DataGrid
                List<Item> items = new List<Item>
            {
                new Item { ID = "1", Name = "Item1", Current = "Active", Status = "PASS" },
                new Item { ID = "2", Name = "Item2", Current = "Inactive", Status = "FAIL" },
                new Item { ID = "3", Name = "Item3", Current = "Active", Status = "PASS" },
                new Item { ID = "4", Name = "Item4", Current = "Active", Status = "PASS" }
            };

                // Set the ItemsSource of the DataGrid to the list of items
                myDataGrid.ItemsSource = items;
            }
        }

        // Item class to represent data for the DataGrid
        public class Item
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string Current { get; set; }
            public string Status { get; set; }
        }
    }


