using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Videoficha.Models;
using Videoficha.Infrastructure.Services;
using System.Management;

namespace Videoficha.Features.Kiosk.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ISystemProvider _systemProvider;
        private readonly IConfigService _configService;
        private KioskSettings _settings;
        private SystemSpec _systemSpec;
        private bool _isLoading;
        private string _cpuVendorIcon = string.Empty;
        private string _manufacturerLogo = string.Empty;
        private string _distributorLogo = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel(ISystemProvider systemProvider, IConfigService configService)
        {
            _systemProvider = systemProvider;
            _configService = configService;
            _settings = new KioskSettings();
            _systemSpec = new SystemSpec();
        }

        public KioskSettings Settings
        {
            get => _settings;
            set { _settings = value; OnPropertyChanged(); }
        }

        public SystemSpec SystemSpec
        {
            get => _systemSpec;
            set { _systemSpec = value; OnPropertyChanged(); }
        }

        public string CpuVendorIcon
        {
            get => _cpuVendorIcon;
            set { _cpuVendorIcon = value; OnPropertyChanged(); }
        }

        public string ManufacturerLogo
        {
            get => _manufacturerLogo;
            set { _manufacturerLogo = value; OnPropertyChanged(); }
        }

        public string DistributorLogo
        {
            get => _distributorLogo;
            set { _distributorLogo = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public async Task InitializeAsync()
        {
            IsLoading = true;
            Settings = _configService.LoadSettings();
            
            var savedSpec = _configService.LoadSystemSpec();
            if (savedSpec == null || string.IsNullOrEmpty(savedSpec.Display))
            {
                SystemSpec = await _systemProvider.GetSystemInfoAsync();
                _configService.SaveSystemSpec(SystemSpec);
            }
            else
            {
                SystemSpec = savedSpec;
            }

            UpdateLogos();
            IsLoading = false;
        }

        private void UpdateLogos()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string imagesDir = Path.Combine(baseDir, "Assets", "Images");

            // 1. CPU Logo
            string cpuIcon = "intel.png";
            if (SystemSpec.Processor.Contains("AMD", StringComparison.OrdinalIgnoreCase) || 
                SystemSpec.Processor.Contains("Ryzen", StringComparison.OrdinalIgnoreCase))
                cpuIcon = "amd.png";
            CpuVendorIcon = Path.Combine(imagesDir, cpuIcon);

            // 2. Manufacturer Logo (Auto-detection)
            string manufacturer = GetManufacturerName();
            string mfgFile = "hp.png"; // Default
            if (manufacturer.Contains("HP", StringComparison.OrdinalIgnoreCase)) mfgFile = "hp.png";
            else if (manufacturer.Contains("Lenovo", StringComparison.OrdinalIgnoreCase)) mfgFile = "lenovo.png";
            else if (manufacturer.Contains("Acer", StringComparison.OrdinalIgnoreCase)) mfgFile = "acer.png";
            else if (manufacturer.Contains("Asus", StringComparison.OrdinalIgnoreCase)) mfgFile = "asus.png";
            else if (manufacturer.Contains("Samsung", StringComparison.OrdinalIgnoreCase)) mfgFile = "samsung.png";
            
            ManufacturerLogo = Path.Combine(imagesDir, mfgFile);

            // 3. Distributor Logo (Custom or Default)
            if (!string.IsNullOrEmpty(Settings.DistributorLogoPath) && File.Exists(Settings.DistributorLogoPath))
            {
                DistributorLogo = Settings.DistributorLogoPath;
            }
            else
            {
                DistributorLogo = Path.Combine(imagesDir, "falabella.png"); // Default store
            }
        }

        private string GetManufacturerName()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("select Manufacturer from Win32_ComputerSystem"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        return obj["Manufacturer"].ToString() ?? "";
                    }
                }
            }
            catch { }
            return "";
        }

        public void SaveSettings()
        {
            _configService.SaveSettings(Settings);
            UpdateLogos();
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
