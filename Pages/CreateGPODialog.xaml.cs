using System.Windows;
using Microsoft.Win32;
using Wpf.Ui.Controls;

namespace Techolics_.Pages
{
    public partial class CreateGPODialog : FluentWindow
    {
        public string GPOName => GPONameTextBox.Text;
        public string Description => DescriptionTextBox.Text;
        public string SaveLocation { get; private set; }

        public CreateGPODialog()
        {
            InitializeComponent();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SaveLocation = dialog.SelectedPath;
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SaveLocation))
            {
                MessageBox.Show("Please select a save location.", "Save Location Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
