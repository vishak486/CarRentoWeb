using CarRentoWeb.Data;
using CarRentoWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace CarRentoWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class BrandsController : Controller
    {
        public const string ActiveBrandsCacheKey = "ActiveBrands";

        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public BrandsController(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }
        public IActionResult Index()
        {
            if(!_cache.TryGetValue(ActiveBrandsCacheKey, out List<Brand>? brands))
            {
                brands = _context.Brands.Where(b => b.IsActive).ToList();
                var options=new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                };
                _cache.Set(ActiveBrandsCacheKey, brands, options);
            }
            return View(brands);
        }
    }
}
