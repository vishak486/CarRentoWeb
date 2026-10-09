using CarRentoWeb.Data;
using CarRentoWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using OrderModel = CarRentoWeb.Models.Order;

namespace CarRentoWeb.Areas.Customer.Pages.Order
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<OrderModel> Orders { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);

            Orders = await _context.Orders
                .Include(o => o.OrderItems!).ThenInclude(oi => oi.Car)
                .Include(o => o.Payment)
                .Where(o => o.UserId == userId && o.Status == OrderStatus.Completed)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }
    }
}