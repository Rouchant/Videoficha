using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Videoficha.Features.Kiosk.Views
{
    public partial class LeerFicha : Window
    {
        private DispatcherTimer? inactivityTimer;
        private const int InactivityThreshold = 300000; // 5 minutes

        public LeerFicha(string pdfPath)
        {
            InitializeComponent();
            _ = InitializeWebView();
            LoadPDF(pdfPath);
            SetWindowSize();
            
            inactivityTimer = new DispatcherTimer();
            inactivityTimer.Interval = TimeSpan.FromMilliseconds(InactivityThreshold);
            inactivityTimer.Tick += InactivityTimer_Tick;

            this.MouseMove += ResetInactivityTimer;
            this.KeyDown += ResetInactivityTimer;

            inactivityTimer.Start();
        }

        private async System.Threading.Tasks.Task InitializeWebView()
        {
            await pdfWebView.EnsureCoreWebView2Async();
        }

        private void SetWindowSize()
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;

            this.Width = screenWidth * 0.8;
            this.Height = screenHeight * 0.8;

            this.Left = (screenWidth - this.Width) / 2;
            this.Top = (screenHeight - this.Height) / 2;
        }

        private void LoadPDF(string pdfPath)
        {
            pdfWebView.Source = new Uri($"file:///{pdfPath}");
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Border_MouseDown(object? sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void ResetInactivityTimer(object? sender, EventArgs e)
        {
            inactivityTimer?.Stop();
            inactivityTimer?.Start();
        }

        private void InactivityTimer_Tick(object? sender, EventArgs e)
        {
            this.Close();
        }
    }
}
