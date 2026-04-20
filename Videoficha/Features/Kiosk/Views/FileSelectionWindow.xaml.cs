using System;
using System.Windows;
using Microsoft.Win32;
using Videoficha.Features.Kiosk.ViewModels;
using Videoficha.Infrastructure.Services;
using Videoficha.Features.SystemDiagnostics.Views;

namespace Videoficha.Features.Kiosk.Views
{
    public partial class FileSelectionWindow : Window
    {
        private readonly FileSelectionViewModel _viewModel;

        public string? VideoFilePath { get; private set; }

        public FileSelectionWindow()
        {
            InitializeComponent();
            _viewModel = new FileSelectionViewModel(new ConfigService());
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

        private void EditSpecsButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new SystemInfoEditWindow { Owner = this };
            editWindow.ShowDialog();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Save();
            VideoFilePath = _viewModel.VideoPath;
            DialogResult = true;
            Close();
        }
    }
}
