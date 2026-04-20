using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Win32;
using Videoficha.Infrastructure.Services;
using Videoficha.Models;

namespace Videoficha.Features.Kiosk.ViewModels
{
    public class FileSelectionViewModel : INotifyPropertyChanged
    {
        private readonly IConfigService _configService;
        private KioskSettings _settings;

        public event PropertyChangedEventHandler? PropertyChanged;

        public FileSelectionViewModel(IConfigService configService)
        {
            _configService = configService;
            _settings = _configService.LoadSettings();
        }

        public string VideoPath
        {
            get => _settings.SelectedVideoPath;
            set { _settings.SelectedVideoPath = value; OnPropertyChanged(); }
        }

        public string SKU
        {
            get => _settings.SKU;
            set { _settings.SKU = value; OnPropertyChanged(); }
        }

        public bool ShowSKU
        {
            get => _settings.ShowSKU;
            set { _settings.ShowSKU = value; OnPropertyChanged(); }
        }

        public string ListPrice
        {
            get => _settings.ListPrice;
            set { _settings.ListPrice = value; OnPropertyChanged(); }
        }

        public string PromoPrice
        {
            get => _settings.PromoPrice;
            set { _settings.PromoPrice = value; OnPropertyChanged(); }
        }

        public bool ShowPrice
        {
            get => _settings.ShowPrice;
            set { _settings.ShowPrice = value; OnPropertyChanged(); }
        }

        public string CTAText
        {
            get => _settings.CTAText;
            set { _settings.CTAText = value; OnPropertyChanged(); }
        }

        public string DistributorLogoPath
        {
            get => _settings.DistributorLogoPath;
            set { _settings.DistributorLogoPath = value; OnPropertyChanged(); }
        }

        public void SelectDistributorLogo()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Imágenes (*.png;*.jpg;*.svg)|*.png;*.jpg;*.svg";
            if (openFileDialog.ShowDialog() == true)
            {
                DistributorLogoPath = openFileDialog.FileName;
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
