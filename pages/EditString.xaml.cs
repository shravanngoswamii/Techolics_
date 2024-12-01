using System;
using System.Windows;

namespace Techolics_.pages
{
    /// <summary>
    /// Interaction logic for EditString.xaml
    /// </summary>
    public partial class EditString : Window
    {
        public EditString()
        {
            InitializeComponent();
        }

        // OK Button Click Event Handler
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the user input from the TextBox
            string userInput = stringInput.Text;

            if (string.IsNullOrEmpty(userInput))
            {
                MessageBox.Show("Please enter a valid string.");
            }
            else
            {
                MessageBox.Show($"You entered: {userInput}");
            }
        }

        // Apply Button Click Event Handler
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the user input from the TextBox
            string userInput = stringInput.Text;

            if (string.IsNullOrEmpty(userInput))
            {
                MessageBox.Show("Please enter a valid string.");
            }
            else
            {
                MessageBox.Show($"You applied: {userInput}");
            }
        }

        // Cancel Button Click Event Handler
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the window without saving
            this.Close();
        }
    }
}
