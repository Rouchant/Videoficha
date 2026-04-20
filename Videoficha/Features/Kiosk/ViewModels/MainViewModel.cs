using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Videoficha.Models;
using Videoficha.Infrastructure.Services;

namespace Videoficha.Features.Kiosk.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ISystemProvider _systemProvider;
        private readonly IConfigService _configService;
        private KioskSettings _settings;
        private SystemSpec _systemSpec;
        private bool _isLoading;

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
            if (savedSpec == null)
            {
                SystemSpec = await _systemProvider.GetSystemInfoAsync();
                _configService.SaveSystemSpec(SystemSpec);
            }
            else
            {
                SystemSpec = savedSpec;
            }

            IsLoading = false;
        }

        public void SaveSettings()
        {
            _configService.SaveSettings(Settings);
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
