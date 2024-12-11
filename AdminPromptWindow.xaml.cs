//AdminPromptWindow.xaml.cs
using System.Windows;
using System.Windows.Input;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Techolics_
{
    public partial class AdminPromptWindow : FluentWindow
    {
        public AdminPromptWindow()
        {
            InitializeComponent();

            // Initialize the theme watcher
            Loaded += (sender, args) =>
            {
                SystemThemeWatcher.Watch(
                    this,                                    // Window instance
                    WindowBackdropType.Mica,                // Background type
                    true                                     // Automatically update accents
                );
            };

            // Clean up the watcher when the window is closing
            Closing += (sender, args) =>
            {
                if (new System.Windows.Interop.WindowInteropHelper(this).Handle != IntPtr.Zero)
                {
                    SystemThemeWatcher.UnWatch(this);
                }
            };
        }

        private void FluentWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Enable window dragging
                this.DragMove();
            }
        }

        private async void YesButton_Click(object sender, RoutedEventArgs e)
        {
            var exeName = Process.GetCurrentProcess().MainModule?.FileName;
            if (exeName == null)
            {
                await ShowErrorMessageAsync("Failed to retrieve the executable name.");
                return;
            }

            var startInfo = new ProcessStartInfo(exeName)
            {
                UseShellExecute = true,
                Verb = "runas"
            };

            try
            {
                Process.Start(startInfo);
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                await ShowErrorMessageAsync($"Failed to restart with admin privileges:\n{ex.Message}");
            }
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async Task ShowErrorMessageAsync(string message)
        {
            var messageBox = new Wpf.Ui.Controls.MessageBox
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "Close",
                CloseButtonAppearance = ControlAppearance.Danger
            };

            await messageBox.ShowDialogAsync();
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                // Ensure the window handle is valid before unwatching
                if (new System.Windows.Interop.WindowInteropHelper(this).Handle != IntPtr.Zero)
                {
                    SystemThemeWatcher.UnWatch(this);
                }
            }
            catch (Exception ex)
            {
                // Log or handle exceptions if necessary
                Debug.WriteLine($"Error during UnWatch: {ex.Message}");
            }

            base.OnClosed(e);
        }

    }
}
