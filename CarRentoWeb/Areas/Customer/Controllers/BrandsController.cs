using CarRentoWeb.Data;
using Microsoft.AspNetCore.Mvc;

namespace CarRentoWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class BrandsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public BrandsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var brands = _context.Brands.Where(b => b.IsActive).ToList();
            return View(brands);
        }
    }
}
