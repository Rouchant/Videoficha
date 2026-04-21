using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Videoficha.Models;
using Videoficha.Infrastructure.Services;

namespace Videoficha.Features.SystemDiagnostics.ViewModels
{
    public class SystemInfoEditViewModel : INotifyPropertyChanged
    {
        private readonly IConfigService _configService;
        private readonly ISystemProvider _systemProvider;
        private SystemSpec _systemSpec;

        public event PropertyChangedEventHandler? PropertyChanged;

        public SystemInfoEditViewModel(IConfigService configService, ISystemProvider systemProvider)
        {
            _configService = configService;
            _systemProvider = systemProvider;
            _systemSpec = _configService.LoadSystemSpec() ?? new SystemSpec();
        }

        public string Model
        {
            get => _systemSpec.Model;
            set { _systemSpec.Model = value; OnPropertyChanged(); }
        }

        public string OS
        {
            get => _systemSpec.OS;
            set { _systemSpec.OS = value; OnPropertyChanged(); }
        }

        public string Processor
        {
            get => _systemSpec.Processor;
            set { _systemSpec.Processor = value; OnPropertyChanged(); }
        }

        public string RAM
        {
            get => _systemSpec.RAM;
            set { _systemSpec.RAM = value; OnPropertyChanged(); }
        }

        public string Storage
        {
            get => _systemSpec.Storage;
            set { _systemSpec.Storage = value; OnPropertyChanged(); }
        }

        public string Graphics
        {
            get => _systemSpec.Graphics;
            set { _systemSpec.Graphics = value; OnPropertyChanged(); }
        }

        public string Display
        {
            get => _systemSpec.Display;
            set { _systemSpec.Display = value; OnPropertyChanged(); }
        }

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

        public void Save()
        {
            _configService.SaveSystemSpec(_systemSpec);
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
