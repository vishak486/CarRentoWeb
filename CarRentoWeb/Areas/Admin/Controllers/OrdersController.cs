using CarRentoWeb.Data;
using CarRentoWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentoWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var orders = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems!).ThenInclude(oi => oi.Car)
                .Include(o => o.Payment)
                .Where(o => o.Status == OrderStatus.Completed)
                .OrderByDescending(o => o.CreatedAt)
                .ToList()
                .Select(o => new
                {
                    orderId = o.OrderId,
                    customer = o.User?.Email,
                    cars = string.Join(", ", o.OrderItems!.Select(oi => oi.Car?.CarName)),
                    createdAt = o.CreatedAt.ToLocalTime().ToString("dd MMM yyyy"),
                    total = o.Total,
                    status = o.Status.ToString()
                });

            return Json(new { data = orders });
        }
    }
}