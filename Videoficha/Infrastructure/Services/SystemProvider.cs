using System;
using System.Management;
using System.Threading.Tasks;
using Videoficha.Models;
using System.Linq;
using System.Text.RegularExpressions;

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
                    // Procesador
                    using (var searcher = new ManagementObjectSearcher("select Name from Win32_Processor"))
                    {
                        foreach (var obj in searcher.Get())
                        {
                            spec.Processor = CleanProcessorName(obj["Name"]?.ToString());
                            break;
                        }
                    }

                    // RAM (Método robusto para VMs y Real Hardware)
                    using (var searcher = new ManagementObjectSearcher("select TotalPhysicalMemory from Win32_ComputerSystem"))
                    {
                        foreach (var obj in searcher.Get())
                        {
                            long totalBytes = Convert.ToInt64(obj["TotalPhysicalMemory"]);
                            double totalGB = totalBytes / 1024.0 / 1024.0 / 1024.0;
                            // Redondeo al entero más cercano (ej: 15.9 -> 16)
                            spec.RAM = $"{(int)Math.Round(totalGB)} GB RAM";
                        }
                    }

                    // Almacenamiento
                    using (var searcher = new ManagementObjectSearcher("select Size, Model, MediaType from Win32_DiskDrive"))
                    {
                        long totalBytes = 0;
                        bool hasSSD = false;
                        foreach (var obj in searcher.Get())
                        {
                            long size = Convert.ToInt64(obj["Size"]);
                            if (size <= 0) continue;
                            
                            totalBytes += size;
                            string model = obj["Model"]?.ToString()?.ToUpper() ?? "";
                            string mediaType = obj["MediaType"]?.ToString()?.ToUpper() ?? "";
                            
                            if (model.Contains("SSD") || model.Contains("NVME") || mediaType.Contains("SSD"))
                            {
                                hasSSD = true;
                            }
                        }
                        spec.Storage = FormatStorage(totalBytes, hasSSD);
                    }

                    // Graficos
                    using (var searcher = new ManagementObjectSearcher("select Name from Win32_VideoController"))
                    {
                        foreach (var obj in searcher.Get())
                        {
                            string name = obj["Name"]?.ToString() ?? "";
                            // Evitar "Microsoft Remote Display" o similares si hay otros
                            if (name.Contains("Microsoft") && !name.Contains("Surface") && string.IsNullOrEmpty(spec.Graphics))
                            {
                                spec.Graphics = "Gráficos Integrados";
                            }
                            else if (!string.IsNullOrEmpty(name))
                            {
                                spec.Graphics = name;
                                break; // Priorizamos la primera GPU real
                            }
                        }
                        if (string.IsNullOrEmpty(spec.Graphics)) spec.Graphics = "Gráficos Integrados";
                    }

                    // Pantalla (Resolución Nativa)
                    try 
                    {
                        using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT CurrentHorizontalResolution FROM Win32_VideoController"))
                        {
                            foreach (var obj in searcher.Get())
                            {
                                int width = Convert.ToInt32(obj["CurrentHorizontalResolution"]);
                                if (width <= 0) continue;

                                if (width >= 3840) spec.Display = "4K Ultra HD";
                                else if (width >= 2560) spec.Display = "QHD 2K";
                                else if (width >= 1920) spec.Display = "Full HD 1080p";
                                else if (width >= 1360) spec.Display = "HD 720p";
                                else spec.Display = "Full HD 1080p";
                                break;
                            }
                        }
                    }
                    catch { spec.Display = "Full HD 1080p"; }

                    // Modelo y Fabricante
                    using (var searcher = new ManagementObjectSearcher("select Manufacturer, Model from Win32_ComputerSystem"))
                    {
                        foreach (var obj in searcher.Get())
                        {
                            spec.Model = obj["Model"]?.ToString() ?? "PC de Exhibición";
                        }
                    }

                    spec.OS = Environment.OSVersion.Version.Major >= 10 ? "Windows 11" : "Windows 10";
                }
                catch (Exception)
                {
                    spec.Model = "PC de Exhibición";
                    spec.Processor = "Intel Core i7";
                    spec.RAM = "16 GB RAM";
                    spec.Storage = "512 GB SSD";
                    spec.Display = "Full HD 1080p";
                    spec.Graphics = "Gráficos Integrados";
                    spec.OS = "Windows 11";
                }

                return spec;
            });
        }

        public string CleanProcessorName(string? rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName)) return "Desconocido";

            string name = rawName;
            
            // 1. Quitar (R) y (TM)
            name = name.Replace("(R)", "").Replace("(TM)", "");

            // 2. Quitar Generación (ej: 11th Gen)
            name = Regex.Replace(name, @"\d+th Gen", "", RegexOptions.IgnoreCase);
            
            // 3. Quitar Series (ej: 5000 Series)
            name = Regex.Replace(name, @"\d+ Series", "", RegexOptions.IgnoreCase);
            name = Regex.Replace(name, @"Series", "", RegexOptions.IgnoreCase);

            // 4. Quitar frecuencia (ej: @ 3.00GHz)
            name = Regex.Replace(name, @"@.*", "");

            // 5. Quitar "Processor" y "6-Core" etc.
            name = Regex.Replace(name, @"\d+-Core", "", RegexOptions.IgnoreCase);
            name = Regex.Replace(name, @"Processor", "", RegexOptions.IgnoreCase);

            // 6. Limpieza de espacios extra
            name = Regex.Replace(name, @"\s+", " ").Trim();

            return name;
        }
        private string FormatStorage(long totalBytes, bool hasSSD)
        {
            if (totalBytes <= 0) return "No detectado";

            double totalGB = (double)totalBytes / 1024 / 1024 / 1024;
            
            // Redondeamos al múltiplo de 128GB más cercano (para capturar 128, 256, 512, 1024, etc.)
            // ya que los discos comerciales suelen venir en estos tamaños.
            int roundedGB = (int)(Math.Round(totalGB / 128.0) * 128.0);
            if (roundedGB < 128) roundedGB = 128;

            string type = hasSSD ? "SSD" : "HDD";
            
            if (roundedGB < 1024)
            {
                return $"{roundedGB} GB {type}";
            }
            else
            {
                int tb = roundedGB / 1024;
                int rem = roundedGB % 1024;
                
                if (rem == 0) return $"{tb} TB {type}";
                return $"{tb} TB + {rem} GB {type}";
            }
        }
    }
}
