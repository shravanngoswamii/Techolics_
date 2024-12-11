using System;
using System.Windows;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Techolics_
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);

                // Register an event handler for when the main window is set
                DispatcherUnhandledException += OnDispatcherUnhandledException;

                // Subscribe to the Activated event of the application
                Activated += OnApplicationActivated;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An error occurred: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }
        }

        private void OnApplicationActivated(object? sender, EventArgs e)
        {
            if (MainWindow == null)
                return;

            // Apply the system theme to the MainWindow (if needed)
            ApplicationThemeManager.ApplySystemTheme(true);

            // Unsubscribe from Activated after setting up
            Activated -= OnApplicationActivated;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Perform any necessary cleanup for the MainWindow or other resources
            base.OnExit(e);
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            System.Windows.MessageBox.Show($"Unhandled Exception: {e.Exception.Message}", "Error", System.Windows.MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}
