using System;
using System.Management;
using System.Threading.Tasks;
using Videoficha.Models;
using System.Linq;

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
                            spec.Processor = obj["Name"].ToString()?.Replace("(R)", "").Replace("(TM)", "").Trim() ?? "Desconocido";
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
                    using (var searcher = new ManagementObjectSearcher("select Size from Win32_DiskDrive"))
                    {
                        long totalSize = 0;
                        foreach (var obj in searcher.Get())
                        {
                            totalSize += Convert.ToInt64(obj["Size"]);
                        }
                        spec.Storage = $"{(totalSize / 1024 / 1024 / 1024)} GB SSD/HDD";
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
                    spec.Processor = "Procesador Intel Core";
                    spec.RAM = "16 GB RAM";
                    spec.Storage = "512 GB SSD";
                    spec.Display = "Full HD 1080p";
                    spec.Graphics = "Gráficos Integrados";
                    spec.OS = "Windows 11";
                }

                return spec;
            });
        }
    }
}
