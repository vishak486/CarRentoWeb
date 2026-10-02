using System.Runtime.ConstrainedExecution;
using System.Text.Json.Serialization;

namespace CarRentoWeb.Models
{
    public class CarImage
    {
        public int CarImageId { get; set; }

        public int CarId { get; set; }
        [JsonIgnore]
        public Car? Car { get; set; }

        public string? ImageUrl { get; set; }
    }
}
