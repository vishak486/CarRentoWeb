using CarRentoWeb.Data;
using CarRentoWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CarRentoWeb.Areas.Customer.Pages.Order
{
    [Authorize]
    public class SuccessModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SuccessModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty(SupportsGet = true)]
        public int? OrderId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);

            var isPaidOrderOfUser = OrderId.HasValue && await _context.Orders
                .AnyAsync(o => o.OrderId == OrderId.Value
                            && o.UserId == userId
                            && o.Status == OrderStatus.Completed);

            if (!isPaidOrderOfUser)
            {
                return NotFound();
            }

            return Page();
        }
    }
}