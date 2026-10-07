using CarRentoWeb.Data;
using CarRentoWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentoWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CarsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public CarsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index(int? brandId)
        {
            var cars = _context.Cars.Include(b => b.Brand).Include(c => c.CarImages)
                .Where(c => c.BrandId == brandId && c.Status == "Available").ToList();

            ViewBag.BrandName = _context.Brands.Where(b => b.BrandId == brandId).Select(b => b.BrandName).FirstOrDefault();
            ViewBag.BrandId = brandId;

            return View(cars);
        }

        public IActionResult Details(int? id)
        {
            var car = _context.Cars
                .Include(c => c.Brand)
                .Include(c => c.CarImages)
                .FirstOrDefault(c => c.CarId == id);

            if (car == null) return NotFound();

            bool isProfileComplete = false;

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                var user = _context.Users.FirstOrDefault(u => u.Id == userId);

                if (user != null)
                {
                    isProfileComplete = !string.IsNullOrWhiteSpace(user.FullName)
                        && !string.IsNullOrWhiteSpace(user.Address)
                        && !string.IsNullOrWhiteSpace(user.DrivingLicenseNo)
                        && !string.IsNullOrWhiteSpace(user.PhoneNumber);
                }
            }
            ViewBag.IsProfileComplete = isProfileComplete;

            return View(car);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult AddToCart(int? carId,DateTime startDate,DateTime endDate)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Please login to continue." });
            }
            if (endDate <= startDate)
            {
                return Json(new { success = false, message = "End date must be after start date." });
            }
            var userId = _userManager.GetUserId(User);
            var car = _context.Cars.Find(carId);
            if (car == null)
            {
                return Json(new { success = false, message = "Car not found." });
            }
            bool hasOverlap = _context.CartItems
            .Any(ci => ci.CarId == carId
            && startDate < ci.RentalEndDate
            && endDate > ci.RentalStartDate);

            if (hasOverlap)
            {
                return Json(new { success = false, message = "This car is already reserved for part of your selected dates. Please choose different dates." });
            }

            var cart = _context.Carts.FirstOrDefault(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new Cart { UserId = userId! };
                _context.Carts.Add(cart);
                _context.SaveChanges();
            }
            var cartItem = new CartItem
            {
                CartId = cart.CartId,
                CarId = car.CarId,
                RentalStartDate = startDate,
                RentalEndDate = endDate,
                PricePerDay = car.PricePerDay
            };
            _context.CartItems.Add(cartItem);
            _context.SaveChanges();
            TempData["success"] = "Add To Cart Successfully";

            var cartCount = _context.CartItems.Count(ci => ci.CartId == cart.CartId);
            return Json(new { success = true, cartCount });
        }
    }
}
