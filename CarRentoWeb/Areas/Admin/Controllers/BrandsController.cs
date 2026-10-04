using CarRentoWeb.Data;
using CarRentoWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentoWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BrandsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public BrandsController(ApplicationDbContext context,IWebHostEnvironment webHostEnvironment)
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
            List<Brand> objBrandList = _context.Brands.ToList();
            return Json(new { data = objBrandList });
        }
        public IActionResult Create()
        {
            return View(new Brand());
        }
        [HttpPost]
        public IActionResult Create(Brand obj,IFormFile? file)
        {
            if(ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                string productPath = Path.Combine(wwwRootPath,"images", "brand");
                Directory.CreateDirectory(productPath);
                try
                {
                    if(file!=null)
                    {
                        string[] allowed = { ".jpg", ".jpeg", ".png", ".webp" };
                        string ext = Path.GetExtension(file.FileName).ToLower();
                        if (!allowed.Contains(ext))
                        {
                            ModelState.AddModelError("LogoUrl", "Only JPG, PNG or WEBP images are allowed.");
                            return View(obj);
                        }

                        string fileName = Guid.NewGuid().ToString() + ext;
                        using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }

                        obj.LogoUrl = @"/images/brand/" + fileName;
                    }
                }
                catch(Exception ex)
                {
                    TempData["error"] = $"File upload failed: {ex.Message}";
                    return View(obj);
                }
                _context.Brands.Add(obj);
                _context.SaveChanges();
                TempData["success"] = "Brand created successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult ToggleStatus(int? id)
        {
            var brand = _context.Brands.Find(id);
            if(brand==null)
            {
                return Json(new { success = false, message = "Brand not found" });
            }
            brand.IsActive = !brand.IsActive;
            _context.SaveChanges();
            return Json(new { success=true, isActive= brand.IsActive });
        }
    }
}
