using CarRentoWeb.Data;
using CarRentoWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace CarRentoWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BrandsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public BrandsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Brand> objBrandList = _context.Brands.ToList();
            return Json(new { data = objBrandList });
        }
        [HttpPost]
        public IActionResult Create(Brand obj)
        {
            if(ModelState.IsValid)
            {
                _context.Brands.Add(obj);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(obj);
        }
    }
}
