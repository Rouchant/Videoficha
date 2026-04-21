using System;
using System.Windows;
using Microsoft.Win32;
using Videoficha.Features.Kiosk.ViewModels;
using Videoficha.Infrastructure.Services;
using System.Windows.Controls;

namespace Videoficha.Features.Kiosk.Views
{
    public partial class FileSelectionWindow : Window
    {
        private readonly FileSelectionViewModel _viewModel;

        public string? VideoFilePath { get; private set; }

        public FileSelectionWindow()
        {
            InitializeComponent();
            _viewModel = new FileSelectionViewModel(new ConfigService(), new SystemProvider());
            DataContext = _viewModel;
        }

        private void SelectVideoButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Videos (*.mp4;*.wmv;*.avi)|*.mp4;*.wmv;*.avi";
            if (openFileDialog.ShowDialog() == true)
            {
                _viewModel.VideoPath = openFileDialog.FileName;
            }
        }

        private void SelectLogoButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectDistributorLogo();
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

        private void SelectInactivityVideoButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectInactivityVideo();
        }

        private void RestoreMainVideoButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.RestoreDefaultMainVideo();
        }

        private void RestoreInactivityVideoButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.RestoreDefaultInactivityVideo();
        }

        private void RestoreLogoButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.RestoreDefaultLogo();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Save();
            VideoFilePath = _viewModel.VideoPath;
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
