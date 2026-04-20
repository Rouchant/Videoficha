using System.ComponentModel;
using System.Runtime.CompilerServices;
using Videoficha.Models;
using Videoficha.Infrastructure.Services;

namespace Videoficha.Features.SystemDiagnostics.ViewModels
{
    public class SystemInfoEditViewModel : INotifyPropertyChanged
    {
        private readonly IConfigService _configService;
        private SystemSpec _systemSpec;

        public event PropertyChangedEventHandler? PropertyChanged;

        public SystemInfoEditViewModel(IConfigService configService)
        {
            _configService = configService;
            _systemSpec = _configService.LoadSystemSpec() ?? new SystemSpec();
        }

        public SystemSpec SystemSpec
        {
            get => _systemSpec;
            set { _systemSpec = value; OnPropertyChanged(); }
        }

        public void Save()
        {
            _configService.SaveSystemSpec(SystemSpec);
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
