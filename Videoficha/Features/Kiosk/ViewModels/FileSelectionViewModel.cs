using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Win32;
using Videoficha.Infrastructure.Services;
using Videoficha.Models;

namespace Videoficha.Features.Kiosk.ViewModels
{
    public class FileSelectionViewModel : INotifyPropertyChanged
    {
        private readonly IConfigService _configService;
        private readonly ISystemProvider _systemProvider;
        private KioskSettings _settings;
        private SystemSpec _systemSpec;

        public event PropertyChangedEventHandler? PropertyChanged;

        public FileSelectionViewModel(IConfigService configService, ISystemProvider systemProvider)
        {
            _configService = configService;
            _systemProvider = systemProvider;
            _settings = _configService.LoadSettings();
            _systemSpec = _configService.LoadSystemSpec() ?? new SystemSpec();
        }

        // --- HARDWARE PROPERTIES ---
        public string Model { get => _systemSpec.Model; set { _systemSpec.Model = value; OnPropertyChanged(); } }
        public string OS { get => _systemSpec.OS; set { _systemSpec.OS = value; OnPropertyChanged(); } }
        public string Processor { get => _systemSpec.Processor; set { _systemSpec.Processor = value; OnPropertyChanged(); } }
        public string RAM { get => _systemSpec.RAM; set { _systemSpec.RAM = value; OnPropertyChanged(); } }
        public string Storage { get => _systemSpec.Storage; set { _systemSpec.Storage = value; OnPropertyChanged(); } }
        public string Graphics { get => _systemSpec.Graphics; set { _systemSpec.Graphics = value; OnPropertyChanged(); } }
        public string Display { get => _systemSpec.Display; set { _systemSpec.Display = value; OnPropertyChanged(); } }

        public async Task RestoreFieldAsync(string fieldName)
        {
            var autoSpec = await _systemProvider.GetSystemInfoAsync();
            switch (fieldName)
            {
                case "Model": Model = autoSpec.Model; break;
                case "Processor": Processor = autoSpec.Processor; break;
                case "RAM": RAM = autoSpec.RAM; break;
                case "Storage": Storage = autoSpec.Storage; break;
                case "Display": Display = autoSpec.Display; break;
                case "Graphics": Graphics = autoSpec.Graphics; break;
                case "OS": OS = autoSpec.OS; break;
            }
        }

        // --- KIOSK SETTINGS PROPERTIES ---
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

        public bool StrikethroughListPrice
        {
            get => _settings.StrikethroughListPrice;
            set { _settings.StrikethroughListPrice = value; OnPropertyChanged(); }
        }

        public string CTAText
        {
            get => _settings.CTAText;
            set { _settings.CTAText = value; OnPropertyChanged(); }
        }

        public string InactivityVideoPath
        {
            get => _settings.InactivityVideoPath;
            set { _settings.InactivityVideoPath = value; OnPropertyChanged(); }
        }

        public bool ShowDistributorLogo
        {
            get => _settings.ShowDistributorLogo;
            set { _settings.ShowDistributorLogo = value; OnPropertyChanged(); }
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

        public void RestoreDefaultLogo()
        {
            DistributorLogoPath = string.Empty;
        }

        public void SelectInactivityVideo()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Videos (*.mp4;*.wmv;*.avi)|*.mp4;*.wmv;*.avi";
            if (openFileDialog.ShowDialog() == true)
            {
                InactivityVideoPath = openFileDialog.FileName;
            }
        }

        public void RestoreDefaultMainVideo()
        {
            VideoPath = "Assets/Samples/landing-generic.mp4";
        }

        public void RestoreDefaultInactivityVideo()
        {
            InactivityVideoPath = "Assets/Samples/promo-generic.mp4";
        }

        public void Save()
        {
            _configService.SaveSettings(_settings);
            _configService.SaveSystemSpec(_systemSpec);
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
