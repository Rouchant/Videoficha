using Videoficha.Models;

namespace Videoficha.Infrastructure.Services
{
    public interface IConfigService
    {
        KioskSettings LoadSettings();
        void SaveSettings(KioskSettings settings);
        SystemSpec? LoadSystemSpec();
        void SaveSystemSpec(SystemSpec spec);
    }
}
