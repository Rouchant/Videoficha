using System;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Videoficha.Models;

namespace Videoficha.Infrastructure.Services
{
    public class SystemProvider : ISystemProvider
    {
        public async Task<SystemSpec> GetSystemInfoAsync()
        {
            return await Task.Run(() =>
            {
                var spec = new SystemSpec();
                try
                {
                    // 1. Processor Name & Generation
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                    {
                        foreach (var item in searcher.Get())
                        {
                            string rawName = item["Name"]?.ToString() ?? "Unknown";
                            string procName = Regex.Replace(rawName, @"\(R\)|\(TM\)", "").Trim();
                            procName = Regex.Replace(procName, @"\s+", " ");

                            string gen = DetectGeneration(procName);
                            spec.Processor = string.IsNullOrEmpty(gen) ? procName : $"{procName} ({gen})";
                        }
                    }

                    // 2. Brand & Model (Handling Generic Manufacturers)
                    string brand = "PC Desktop";
                    string modelName = "Unknown";
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
                    {
                        foreach (var item in searcher.Get())
                        {
                            brand = item["Manufacturer"]?.ToString()?.Trim() ?? "";
                            modelName = item["Model"]?.ToString()?.Trim() ?? "";
                        }
                    }

                    if (IsGenericBrand(brand))
                    {
                        using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard"))
                        {
                            foreach (var item in searcher.Get())
                            {
                                brand = item["Manufacturer"]?.ToString()?.Trim() ?? "Generic";
                                if (IsGenericModel(modelName))
                                {
                                    modelName = item["Product"]?.ToString()?.Trim() ?? "Motherboard";
                                }
                            }
                        }
                    }
                    spec.Model = brand.Contains(modelName, StringComparison.OrdinalIgnoreCase) ? brand : $"{brand} {modelName}";

                    // 3. Operating System (Cleaned)
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
                    {
                        foreach (var item in searcher.Get())
                        {
                            spec.OS = (item["Caption"]?.ToString() ?? "Unknown").Replace("Microsoft ", "").Trim();
                        }
                    }

                    // 4. RAM Size & Type
                    long totalMemory = 0;
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
                    {
                        foreach (var item in searcher.Get())
                        {
                            totalMemory = Convert.ToInt64(item["TotalPhysicalMemory"]);
                        }
                    }

                    double ramGB = totalMemory / (1024.0 * 1024.0 * 1024.0);
                    int ramSize = (int)(Math.Round(ramGB / 4.0) * 4);
                    if (ramSize == 0) ramSize = (int)Math.Max(1, Math.Round(ramGB));

                    string ramType = "DDR4";
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory"))
                    {
                        foreach (var item in searcher.Get())
                        {
                            int type = Convert.ToInt32(item["SMBIOSMemoryType"]);
                            ramType = type switch
                            {
                                26 => "DDR4",
                                34 => "DDR5",
                                35 => "LPDDR5",
                                _ => ramType
                            };
                            break;
                        }
                    }
                    spec.RAM = $"{ramSize} GB {ramType}";

                    // 5. Storage (Total SSD/HDD with 128GB steps)
                    double totalStorageBytes = 0;
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive WHERE MediaType LIKE '%Fixed%'"))
                    {
                        foreach (var item in searcher.Get())
                        {
                            totalStorageBytes += Convert.ToDouble(item["Size"]);
                        }
                    }
                    double totalGB = totalStorageBytes / 1000000000.0;
                    int roundedGB = (int)(Math.Round(totalGB / 128.0) * 128);
                    if (roundedGB == 0) roundedGB = (int)Math.Round(totalGB);

                    spec.Storage = roundedGB >= 1024 
                        ? $"{(int)Math.Round(roundedGB / 1024.0)} TB SSD" 
                        : $"{roundedGB} GB SSD";

                    // 6. Graphics (Prioritize RTX -> GTX -> Radeon)
                    string gpuName = "Generic Graphics";
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                    {
                        var gpus = searcher.Get().Cast<ManagementObject>().ToList();
                        var prioritizedGpu = gpus.FirstOrDefault(g => (g["Name"]?.ToString()?.Contains("RTX") ?? false))
                                            ?? gpus.FirstOrDefault(g => (g["Name"]?.ToString()?.Contains("GTX") ?? false) 
                                                                    || (g["Name"]?.ToString()?.Contains("NVIDIA") ?? false) 
                                                                    || (g["Name"]?.ToString()?.Contains("Radeon") ?? false))
                                            ?? gpus.FirstOrDefault();
                        
                        if (prioritizedGpu != null)
                        {
                            gpuName = prioritizedGpu["Name"]?.ToString()?.Trim() ?? gpuName;
                        }
                    }
                    spec.Graphics = gpuName;
                }
                catch (Exception ex)
                {
                    spec.Model = "PC Generico";
                    spec.Processor = $"Error: {ex.Message}";
                }

                return spec;
            });
        }

        private string DetectGeneration(string procName)
        {
            var intelGenMatch = Regex.Match(procName, @"i[3579]-(\d+)");
            if (intelGenMatch.Success) return $"{intelGenMatch.Groups[1].Value}a Gen";

            if (Regex.IsMatch(procName, @"Core\s+[3579]\s+\d")) return "Serie 1"; // Simplified Core Series
            if (procName.Contains("Ultra")) return "Core Ultra";
            if (procName.Contains("Ryzen") && procName.Contains("AI")) return "Ryzen AI";

            var ryzenGenMatch = Regex.Match(procName, @"Ryzen\s+[3579]\s+(\d)(\d{2,3})");
            if (ryzenGenMatch.Success)
            {
                return ryzenGenMatch.Groups[2].Length == 2 
                    ? $"{ryzenGenMatch.Groups[1].Value}00 Series" 
                    : $"{ryzenGenMatch.Groups[1].Value}000 Series";
            }

            if (Regex.IsMatch(procName, @"N\d{3}")) return "N-Series";

            return "";
        }

        private bool IsGenericBrand(string brand)
        {
            string[] generics = { "To be filled", "System manufacturer", "Default string", "System Product Name" };
            return string.IsNullOrEmpty(brand) || generics.Any(g => brand.Contains(g, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsGenericModel(string model)
        {
            string[] generics = { "Default string", "System Product Name" };
            return string.IsNullOrEmpty(model) || generics.Any(g => model.Contains(g, StringComparison.OrdinalIgnoreCase));
        }
    }
}
