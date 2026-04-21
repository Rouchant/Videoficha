using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Videoficha.Features.SystemDiagnostics.ViewModels;
using Videoficha.Infrastructure.Services;

namespace Videoficha.Features.SystemDiagnostics.Views
{
    public partial class SystemInfoEditWindow : Window
    {
        private readonly SystemInfoEditViewModel _viewModel;
        private readonly ISystemProvider _systemProvider;
        private DispatcherTimer? inactivityTimer;
        private const int InactivityThreshold = 300000; // 5 minutes

        public SystemInfoEditWindow()
        {
            InitializeComponent();
            
            _systemProvider = new SystemProvider();
            _viewModel = new SystemInfoEditViewModel(new ConfigService(), _systemProvider);
            DataContext = _viewModel;

            InitializeTimer();
        }

        private async void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string fieldName)
            {
                btn.IsEnabled = false;
                await _viewModel.RestoreFieldAsync(fieldName);
                btn.IsEnabled = true;
            }
        }

        private void InitializeTimer()
        {
            inactivityTimer = new DispatcherTimer();
            inactivityTimer.Interval = TimeSpan.FromMilliseconds(InactivityThreshold);
            inactivityTimer.Tick += InactivityTimer_Tick;
            inactivityTimer.Start();
        }

        private void InactivityTimer_Tick(object? sender, EventArgs e)
        {
            inactivityTimer?.Stop();
            this.Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Save();
            DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}
