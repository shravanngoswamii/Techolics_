using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System;
using System.Windows;

namespace Techolics_.pages
{
    public partial class EditNum : Window
    {
        public EditNum()
        {
            InitializeComponent();
        }

        // OK Button Click
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            int numericValue = MyNumericUpDown.Value ?? 0; // Use null-coalescing operator to handle null
            MessageBox.Show($"OK Clicked. Numeric Value: {numericValue}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Apply Button Click
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            int numericValue = MyNumericUpDown.Value ?? 0;
            MessageBox.Show($"Apply Clicked. Numeric Value Saved: {numericValue}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Cancel Button Click
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Cancel Clicked. Changes were not saved.", "Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            this.Close(); // Close the window
        }
    }
}
