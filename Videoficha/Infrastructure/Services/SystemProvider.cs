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

                    // RAM
                    using (var searcher = new ManagementObjectSearcher("select Capacity from Win32_PhysicalMemory"))
                    {
                        long totalCapacity = 0;
                        foreach (var obj in searcher.Get())
                        {
                            totalCapacity += Convert.ToInt64(obj["Capacity"]);
                        }
                        spec.RAM = $"{(totalCapacity / 1024 / 1024 / 1024)} GB RAM";
                    }

                    // Almacenamiento
                    using (var searcher = new ManagementObjectSearcher("select Size, Model, MediaType from Win32_DiskDrive"))
                    {
                        long totalBytes = 0;
                        bool hasSSD = false;
                        foreach (var obj in searcher.Get())
                        {
                            totalBytes += Convert.ToInt64(obj["Size"]);
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
                            spec.Graphics = obj["Name"].ToString() ?? "Integrados";
                            break;
                        }
                    }

                    // Pantalla (Resolución Nativa)
                    try 
                    {
                        using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT CurrentHorizontalResolution, CurrentVerticalResolution FROM Win32_VideoController"))
                        {
                            foreach (var obj in searcher.Get())
                            {
                                int width = Convert.ToInt32(obj["CurrentHorizontalResolution"]);
                                int height = Convert.ToInt32(obj["CurrentVerticalResolution"]);
                                
                                if (width >= 3840) spec.Display = "4K Ultra HD";
                                else if (width >= 2560) spec.Display = "QHD 2K";
                                else if (width >= 1920) spec.Display = "Full HD 1080p";
                                else if (width >= 1360) spec.Display = "HD 720p";
                                else spec.Display = $"{width} x {height}";
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
                            string manufacturer = obj["Manufacturer"].ToString() ?? "";
                            spec.Model = obj["Model"].ToString() ?? "PC Genérico";
                            
                            // Guardamos el fabricante en una propiedad extra o lo deducimos luego
                            // Para mantener compatibilidad, podemos usar Model para guardar ambos temporalmente
                            // o mejor aún, añadir el campo Manufacturer al SystemSpec si lo deseas.
                            // Por ahora, asumiremos que el Model ya contiene información útil.
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
