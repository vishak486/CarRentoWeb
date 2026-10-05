using CarRentoWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentoWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CarsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CarsController(ApplicationDbContext context)
        {
            _context = context;
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

            return View(car);
        }
    }
}
