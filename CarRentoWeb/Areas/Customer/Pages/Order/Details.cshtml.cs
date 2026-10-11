using CarRentoWeb.Data;
using CarRentoWeb.Models;
using CarRentoWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OrderModel = CarRentoWeb.Models.Order;

namespace CarRentoWeb.Areas.Customer.Pages.Order
{
    [Authorize]
    public class Details : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOrderPdfService _pdfService;

        public Details(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IOrderPdfService pdfService)
        {
            _userManager = userManager;
            _context = context;
            _pdfService = pdfService;
        }
        public OrderModel? Order { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = _userManager.GetUserId(User);

            Order = await _context.Orders
                .Include(o => o.OrderItems!).ThenInclude(oi => oi.Car)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);

            if (Order == null) return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnGetDownloadPdfAsync(int id)
        {
            var userId = _userManager.GetUserId(User);

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Payment)
                .Include(o => o.OrderItems!).ThenInclude(oi => oi.Car).ThenInclude(c => c!.Brand)
                .Include(o => o.OrderItems!).ThenInclude(oi => oi.Car).ThenInclude(c => c!.CarImages)
                .FirstOrDefaultAsync(o => o.OrderId == id
                                       && o.UserId == userId
                                       && o.Status == OrderStatus.Completed);

            if (order == null) return NotFound();

            var pdf = _pdfService.Generate(order);
            return File(pdf, "application/pdf", $"Order-{order.OrderId}.pdf");
        }
    }
}
