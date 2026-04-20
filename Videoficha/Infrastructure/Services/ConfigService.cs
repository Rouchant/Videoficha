using System;
using System.IO;
using System.Linq;
using Videoficha.Models;

namespace Videoficha.Infrastructure.Services
{
    public class ConfigService : IConfigService
    {
        private readonly string _configPath;
        private readonly string _videoSelectionFile = "videoSelection.txt";
        private readonly string _pdfSelectionFile = "pdfSelection.txt";
        private readonly string _systemInfoFile = "systemInfo.txt";

        public ConfigService()
        {
            _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config");
            if (!Directory.Exists(_configPath))
            {
                Directory.CreateDirectory(_configPath);
            }
        }

        public KioskSettings LoadSettings()
        {
            return new KioskSettings
            {
                SelectedVideoPath = LoadFileContent(_videoSelectionFile),
                SelectedPdfPath = LoadFileContent(_pdfSelectionFile)
            };
        }

        public void SaveSettings(KioskSettings settings)
        {
            SaveFileContent(_videoSelectionFile, settings.SelectedVideoPath);
            SaveFileContent(_pdfSelectionFile, settings.SelectedPdfPath);
        }

        public SystemSpec? LoadSystemSpec()
        {
            string path = Path.Combine(_configPath, _systemInfoFile);
            if (File.Exists(path))
            {
                var lines = File.ReadLines(path).ToList();
                return SystemSpec.FromList(lines);
            }
            return null;
        }

        public void SaveSystemSpec(SystemSpec spec)
        {
            string path = Path.Combine(_configPath, _systemInfoFile);
            File.WriteAllLines(path, spec.ToList());
        }

        private string LoadFileContent(string fileName)
        {
            string path = Path.Combine(_configPath, fileName);
            return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        }

        private void SaveFileContent(string fileName, string content)
        {
            string path = Path.Combine(_configPath, fileName);
            File.WriteAllText(path, content);
        }
    }
}
