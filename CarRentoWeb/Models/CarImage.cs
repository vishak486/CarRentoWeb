using System.Runtime.ConstrainedExecution;

namespace CarRentoWeb.Models
{
    public class CarImage
    {
        public int CarImageId { get; set; }

        public int CarId { get; set; }
        public Car? Car { get; set; }

        public string? ImageUrl { get; set; }
    }
}
