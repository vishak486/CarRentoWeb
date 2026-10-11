using CarRentoWeb.Data;
using CarRentoWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRentoWeb.Services;

namespace CarRentoWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IOrderPdfService _pdfService;

        public OrdersController(ApplicationDbContext context, IOrderPdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
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

        [HttpGet]
        public IActionResult DownloadPdf(int id)
        {
            var order = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Payment)
                .Include(o => o.OrderItems!).ThenInclude(oi => oi.Car).ThenInclude(c => c!.Brand)
                .Include(o => o.OrderItems!).ThenInclude(oi => oi.Car).ThenInclude(c => c!.CarImages)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null) return NotFound();

            var pdf = _pdfService.Generate(order);
            return File(pdf, "application/pdf", $"Order-{order.OrderId}.pdf");
        }
    }
}