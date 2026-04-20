using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Videoficha.Infrastructure.Services;
using Videoficha.Models;

namespace Videoficha.Features.Kiosk.ViewModels
{
    public class FileSelectionViewModel : INotifyPropertyChanged
    {
        private readonly IConfigService _configService;
        private KioskSettings _settings;

        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly string DefaultVideoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Samples", "HP.wmv");
        private readonly string DefaultPdfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Samples", "sample.pdf");

        public FileSelectionViewModel(IConfigService configService)
        {
            _configService = configService;
            _settings = _configService.LoadSettings();
        }

        public string VideoPathDisplay => string.IsNullOrEmpty(_settings.SelectedVideoPath) || _settings.SelectedVideoPath == DefaultVideoPath ? "Por defecto" : _settings.SelectedVideoPath;
        public string PdfPathDisplay => string.IsNullOrEmpty(_settings.SelectedPdfPath) || _settings.SelectedPdfPath == DefaultPdfPath ? "Por defecto" : _settings.SelectedPdfPath;

        public string VideoPath
        {
            get => _settings.SelectedVideoPath;
            set 
            { 
                _settings.SelectedVideoPath = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(VideoPathDisplay));
            }
        }

        public string PdfPath
        {
            get => _settings.SelectedPdfPath;
            set 
            { 
                _settings.SelectedPdfPath = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(PdfPathDisplay));
            }
        }

        public void Save()
        {
            _configService.SaveSettings(_settings);
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
