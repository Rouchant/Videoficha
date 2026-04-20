using System.ComponentModel;
using System.Runtime.CompilerServices;
using Videoficha.Models;
using Videoficha.Infrastructure.Services;

namespace Videoficha.Features.SystemDiagnostics.ViewModels
{
    public class SystemInfoViewModel : INotifyPropertyChanged
    {
        private readonly IConfigService _configService;
        private SystemSpec _systemSpec;

        public event PropertyChangedEventHandler? PropertyChanged;

        public SystemInfoViewModel(IConfigService configService)
        {
            _configService = configService;
            _systemSpec = _configService.LoadSystemSpec() ?? new SystemSpec();
        }

        public SystemSpec SystemSpec
        {
            get => _systemSpec;
            set { _systemSpec = value; OnPropertyChanged(); }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
