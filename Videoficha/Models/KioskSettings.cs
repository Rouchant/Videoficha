namespace Videoficha.Models
{
    public class KioskSettings
    {
        public string SelectedVideoPath { get; set; } = string.Empty;
        public string SKU { get; set; } = "SKU-000000";
        public bool ShowSKU { get; set; } = true;
        public string ListPrice { get; set; } = "$0.00";
        public string PromoPrice { get; set; } = "$0.00";
        public bool ShowPrice { get; set; } = true;
        public string CTAText { get; set; } = "EXPLORA";
        public string DistributorLogoPath { get; set; } = string.Empty;
    }
}
