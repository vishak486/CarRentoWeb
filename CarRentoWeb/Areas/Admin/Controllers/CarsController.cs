using CarRentoWeb.Data;
using CarRentoWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentoWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CarsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CarsController(ApplicationDbContext context,IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Car> objCarList = _context.Cars.Include(c => c.Brand).Include(c => c.CarImages).ToList();
            return Json(new { data = objCarList });
        }

        public IActionResult Create()
        {
            ViewBag.brandList = _context.Brands.Where(b => b.IsActive).ToList();
            return View(new Car());
        }
        [HttpPost]
        public IActionResult Create(Car obj, IFormFile? image1, IFormFile? image2, IFormFile? image3)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                string carImagePath = Path.Combine(wwwRootPath, "images", "car");
                Directory.CreateDirectory(carImagePath);

                string[] allowed = { ".jpg", ".jpeg", ".png", ".webp" };
                var files = new List<IFormFile?> { image1, image2, image3 };
                var savedPaths = new List<string>();
                try
                {
                    foreach(var file in files)
                    {
                        if (file == null) continue;

                        string ext = Path.GetExtension(file.FileName).ToLower();
                        if (!allowed.Contains(ext))
                        {
                            ModelState.AddModelError("", "Only JPG, PNG or WEBP images are allowed.");
                            ViewBag.BrandList = _context.Brands.Where(b => b.IsActive).ToList();
                            return View(obj);
                        }
                        string fileName = Guid.NewGuid().ToString() + ext;
                        using (var fileStream = new FileStream(Path.Combine(carImagePath, fileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }
                        savedPaths.Add("/images/car/" + fileName);

                    }
                }
                catch(Exception ex)
                {
                    ModelState.AddModelError("", $"Image upload failed: {ex.Message}");
                    ViewBag.BrandList = _context.Brands.Where(b => b.IsActive).ToList();
                    return View(obj);
                }

                _context.Cars.Add(obj);
                _context.SaveChanges();

                foreach (var path in savedPaths)
                {
                    _context.CarImages.Add(new CarImage { CarId = obj.CarId, ImageUrl = path });
                }
                _context.SaveChanges();

                TempData["success"] = "Car created successfully";
                return RedirectToAction("Index");
            }
            ViewBag.BrandList = _context.Brands.Where(b => b.IsActive).ToList();
            return View(obj);
        }

        public IActionResult Edit(int? id)
        {
            var car = _context.Cars.Find(id);
            if (car == null) return NotFound();
            ViewBag.BrandList = _context.Brands.Where(b => b.IsActive).ToList();
            return View(car);
        }

        [HttpPost]
        public IActionResult Edit(Car obj)
        {
            if (ModelState.IsValid)
            {
                _context.Cars.Update(obj);
                _context.SaveChanges();
                TempData["success"] = "Car updated successfully";
                return RedirectToAction("Index");
            }
            ViewBag.BrandList = _context.Brands.Where(b => b.IsActive).ToList();
            return View(obj);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult UpdateStatus(int? id, string status)
        {
            var car = _context.Cars.Find(id);
            if (car == null)
            {
                return Json(new { success = false, message = "Car not found" });
            }
            car.Status = status;
            _context.SaveChanges();
            return Json(new { success = true, status = car.Status });
        }
    }
}
