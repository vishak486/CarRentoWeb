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
    }
}
