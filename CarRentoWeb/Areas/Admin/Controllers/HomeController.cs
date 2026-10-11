using CarRentoWeb.Data;
using CarRentoWeb.Models;
using CarRentoWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentoWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="Admin")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var paidOrders = _context.Orders.Where(o => o.Status == OrderStatus.Completed);

            var customers = await _userManager.GetUsersInRoleAsync(SD.Role_Customer);

            var model = new DashboardViewModel
            {
                TotalRevenue = await paidOrders.SumAsync(o => (decimal?)o.Total) ?? 0,
                TotalOrders = await paidOrders.CountAsync(),
                TotalCustomers = customers.Count,
                TotalCars = await _context.Cars.CountAsync(),
                AvailableCars = await _context.Cars.CountAsync(c => c.Status == "Available"),
                RentedCars = await _context.Cars.CountAsync(c => c.Status == "Rented"),
                MaintenanceCars = await _context.Cars.CountAsync(c => c.Status == "Maintenance"),
                RecentOrders = await paidOrders
                    .Include(o => o.User)
                    .Include(o => o.OrderItems!).ThenInclude(oi => oi.Car)
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(5)
                    .ToListAsync()
            };

            // Revenue for the last 6 months (including months with no orders)
            var firstMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-5);

            var monthlyOrders = await paidOrders
                .Where(o => o.CreatedAt >= firstMonth)
                .Select(o => new { o.CreatedAt, o.Total })
                .ToListAsync();

            for (int i = 0; i < 6; i++)
            {
                var month = firstMonth.AddMonths(i);
                model.RevenueLabels.Add(month.ToString("MMM yyyy"));
                model.RevenueValues.Add(
                    monthlyOrders
                        .Where(o => o.CreatedAt.Year == month.Year && o.CreatedAt.Month == month.Month)
                        .Sum(o => o.Total));
            }

            // Top 5 most booked cars (paid orders only)
            var topCars = await _context.OrderItems
                .Where(oi => oi.Order!.Status == OrderStatus.Completed)
                .GroupBy(oi => new { oi.CarId, oi.Car!.CarName })
                .Select(g => new { g.Key.CarName, Bookings = g.Count() })
                .OrderByDescending(x => x.Bookings)
                .Take(5)
                .ToListAsync();

            model.TopCarLabels = topCars.Select(x => x.CarName).ToList();
            model.TopCarValues = topCars.Select(x => x.Bookings).ToList();

            return View(model);
        }
    }
}
