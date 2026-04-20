using Microsoft.Win32;
using System.Windows;
using Videoficha.Features.Kiosk.ViewModels;
using Videoficha.Infrastructure.Services;

namespace Videoficha.Features.Kiosk.Views
{
    public partial class FileSelectionWindow : Window
    {
        private readonly FileSelectionViewModel _viewModel;

        public string VideoFilePath => _viewModel.VideoPath;
        public string OtherFilePath => _viewModel.PdfPath;

        public FileSelectionWindow()
        {
            InitializeComponent();
            _viewModel = new FileSelectionViewModel(new ConfigService());
            DataContext = _viewModel;
        }

        private void SelectVideoButton_Click(object? sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Video Files|*.wmv;*.mp4;*.avi;*.mkv",
                Title = "Seleccionar Video"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _viewModel.VideoPath = openFileDialog.FileName;
            }
        }

        private void SelectPdfButton_Click(object? sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "PDF Files|*.pdf",
                Title = "Seleccionar PDF"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _viewModel.PdfPath = openFileDialog.FileName;
            }
        }

        private void OkButton_Click(object? sender, RoutedEventArgs e)
        {
            _viewModel.Save();
            DialogResult = true;
            Close();
        }
    }
}
