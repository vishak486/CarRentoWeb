using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentoWeb.Models
{
    public class Car
    {
        public int CarId { get; set; }

        public int BrandId { get; set; }
        public Brand? Brand { get; set; }

        [Required, StringLength(100)]
        public string CarName { get; set; }

        public string? Model { get; set; }
        public int Year { get; set; }
        public string? Transmission { get; set; }
        public string? FuelType { get; set; }
        public int SeatingCapacity { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PricePerDay { get; set; }
        public string Status { get; set; } = "Available";

        public ICollection<CarImage>? CarImages { get; set; }
    }
}
