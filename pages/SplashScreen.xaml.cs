using System;
using System.Windows;
using System.Windows.Threading;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
namespace Techolics_.pages
{
    public partial class SplashScreen : FluentWindow
    {
        private string _textToType = "Techolics_";
        private int _currentIndex = 0;
        private DispatcherTimer _typingTimer = new DispatcherTimer(); // Initialize here

        public SplashScreen()
        {
            InitializeComponent();
            StartTypingEffect();
        }

        private void StartTypingEffect()
        {
            _typingTimer.Interval = TimeSpan.FromMilliseconds(50); // Typing speed
            _typingTimer.Tick += TypingEffectTick;
            _typingTimer.Start();
        }

        private void TypingEffectTick(object? sender, EventArgs e)
        {
            if (_currentIndex < _textToType.Length)
            {
                DynamicText.Text += _textToType[_currentIndex];
                _currentIndex++;
            }
            else
            {
                _typingTimer.Stop();
                GoToMainScreen();
            }
        }

        private void GoToMainScreen()
        {
            MainWindow mainWindow = new MainWindow();

            Application.Current.MainWindow = mainWindow;

            mainWindow.Show();

            this.Close();
        }
    }
}
