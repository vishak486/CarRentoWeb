namespace CarRentoWeb.Models
{
    public class DashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalCars { get; set; }
        public int AvailableCars { get; set; }
        public int RentedCars { get; set; }
        public List<Order> RecentOrders { get; set; } = new();

        public List<string> RevenueLabels { get; set; } = new();
        public List<decimal> RevenueValues { get; set; } = new();

        public int MaintenanceCars { get; set; }
        public List<string> TopCarLabels { get; set; } = new();
        public List<int> TopCarValues { get; set; } = new();

    }
}
