namespace CarRentoWeb.Models
{
    public class Cart
    {
        public int CartId { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public ICollection<CartItem>? CartItems { get; set; }
    }
}
