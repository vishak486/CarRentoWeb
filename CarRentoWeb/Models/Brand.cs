using System.ComponentModel.DataAnnotations;

namespace CarRentoWeb.Models
{
    public class Brand
    {
        public int BrandId { get; set; }

        [Required,StringLength(150)]
        public string BrandName { get; set; }

        public string? LogoUrl { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
