using System.Collections.Generic;

namespace Videoficha.Models
{
    public class SystemSpec
    {
        public string Model { get; set; } = string.Empty;
        public string OS { get; set; } = string.Empty;
        public string Processor { get; set; } = string.Empty;
        public string RAM { get; set; } = string.Empty;
        public string Storage { get; set; } = string.Empty;
        public string Graphics { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;

        public List<string> ToList()
        {
            return new List<string> { Model, OS, Processor, RAM, Storage, Graphics, Display };
        }

        public static SystemSpec FromList(List<string> list)
        {
            var spec = new SystemSpec();
            if (list.Count >= 1) spec.Model = list[0];
            if (list.Count >= 2) spec.OS = list[1];
            if (list.Count >= 3) spec.Processor = list[2];
            if (list.Count >= 4) spec.RAM = list[3];
            if (list.Count >= 5) spec.Storage = list[4];
            if (list.Count >= 6) spec.Graphics = list[5];
            if (list.Count >= 7) spec.Display = list[6];
            return spec;
        }
    }
}
