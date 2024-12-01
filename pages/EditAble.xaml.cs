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
    /// <summary>
    /// Interaction logic for EditAble.xaml
    /// </summary>
    public partial class EditAble : Window
    {
        public EditAble()
        {
            InitializeComponent();
        }

        // Handle Enable radio button check event
        private void EnableRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            // You can put any custom logic here when the "Enable" radio button is selected
            MessageBox.Show("Enable selected");
        }

        // Handle Disable radio button check event
        private void DisableRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            // You can put any custom logic here when the "Disable" radio button is selected
            MessageBox.Show("Disable selected");
        }

        // OK Button Click Event Handler
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Logic for OK button
            MessageBox.Show("OK button clicked");
        }

        // Apply Button Click Event Handler
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            // Logic for Apply button
            MessageBox.Show("Apply button clicked");
        }

        // Cancel Button Click Event Handler
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Logic for Cancel button
            MessageBox.Show("Cancel button clicked");
        }
    }
}
