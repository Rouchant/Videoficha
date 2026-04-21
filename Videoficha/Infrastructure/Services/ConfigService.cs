using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Videoficha.Models;

namespace Videoficha.Infrastructure.Services
{
    public class ConfigService : IConfigService
    {
        private readonly string _configPath;
        private readonly string _settingsFile = "settings.json";
        private readonly string _hardwareFile = "hardware.json";

        public ConfigService()
        {
            _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config");
            EnsureConfigDirectory();
        }

        private void EnsureConfigDirectory()
        {
            if (!Directory.Exists(_configPath))
            {
                Directory.CreateDirectory(_configPath);
            }
        }

        public KioskSettings LoadSettings()
        {
            string path = Path.Combine(_configPath, _settingsFile);
            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    return JsonSerializer.Deserialize<KioskSettings>(json) ?? new KioskSettings();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error cargando settings: {ex.Message}");
                }
            }
            return new KioskSettings();
        }

        public void SaveSettings(KioskSettings settings)
        {
            try
            {
                EnsureConfigDirectory();
                string path = Path.Combine(_configPath, _settingsFile);
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error guardando settings: {ex.Message}");
            }
        }

        public SystemSpec? LoadSystemSpec()
        {
            string path = Path.Combine(_configPath, _hardwareFile);
            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    return JsonSerializer.Deserialize<SystemSpec>(json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error cargando hardware spec: {ex.Message}");
                }
            }
            return null;
        }

        public void SaveSystemSpec(SystemSpec spec)
        {
            try
            {
                EnsureConfigDirectory();
                string path = Path.Combine(_configPath, _hardwareFile);
                string json = JsonSerializer.Serialize(spec, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error guardando hardware spec: {ex.Message}");
            }
        }
    }
}
