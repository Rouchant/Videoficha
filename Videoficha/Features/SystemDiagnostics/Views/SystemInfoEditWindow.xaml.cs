using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Videoficha.Features.SystemDiagnostics.ViewModels;
using Videoficha.Infrastructure.Services;

namespace Videoficha.Features.SystemDiagnostics.Views
{
    public partial class SystemInfoEditWindow : ContentDialog
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
            this.DataContext = _viewModel;

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

        private void InactivityTimer_Tick(object? sender, object e)
        {
            inactivityTimer?.Stop();
            this.Hide();
        }

        private void SaveButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _viewModel.Save();
        }
    }
}
