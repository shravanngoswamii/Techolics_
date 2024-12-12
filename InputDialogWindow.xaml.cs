using System.Windows;
using Wpf.Ui.Controls;

namespace Techolics_
{
    public partial class InputDialogWindow : FluentWindow
    {
        public string UserInput { get; private set; } = string.Empty;

        public InputDialogWindow(string title, string prompt)
        {
            InitializeComponent();
            this.Title = title;
            PromptTextBlock.Text = prompt;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            UserInput = InputTextBox.Text;
            DialogResult = true; // returns true to ShowDialog()
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
