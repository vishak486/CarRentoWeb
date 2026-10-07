using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CarRentoWeb.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }

        public int OrderId { get; set; }
        [JsonIgnore]
        public Order? Order { get; set; }

        public int CarId { get; set; }
        public Car? Car { get; set; }

        public DateTime RentalStartDate { get; set; }
        public DateTime RentalEndDate { get; set; }

        public int Days => (int)Math.Ceiling((RentalEndDate - RentalStartDate).TotalDays);

        [Column(TypeName = "decimal(10,2)")]
        public decimal PricePerDay { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal LineTotal { get; set; }
    }
}
