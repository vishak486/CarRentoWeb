using CarRentoWeb.Data;
using CarRentoWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentoWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);

            var cartItems=_context.CartItems.Include(ci=>ci.Car).ThenInclude(c=>c!.Brand)
                .Include(ci => ci.Car).ThenInclude(c => c!.CarImages)
                .Where(ci => ci.Cart!.UserId == userId).ToList();

            return View(cartItems);
        }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult Remove(int id)
        {
            var userId = _userManager.GetUserId(User);
            var item = _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefault(ci => ci.CartItemId == id && ci.Cart!.UserId == userId);

            if (item == null)
            {
                return Json(new { success = false, message = "Item not found." });
            }

            _context.CartItems.Remove(item);
            _context.SaveChanges();
            TempData["success"] = "Cart Item Removed";

            return Json(new { success = true });
        }
    }
}
