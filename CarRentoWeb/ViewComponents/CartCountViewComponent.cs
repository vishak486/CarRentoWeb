using CarRentoWeb.Data;
using CarRentoWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CarRentoWeb.ViewComponents
{
    public class CartCountViewComponent :ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartCountViewComponent(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public IViewComponentResult Invoke()
        {
            int count = 0;
            if (UserClaimsPrincipal.Identity != null && UserClaimsPrincipal.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(UserClaimsPrincipal);
                count = _context.CartItems.Count(ci => ci.Cart!.UserId == userId);
            }

            return View(count);
        }
    }
}
