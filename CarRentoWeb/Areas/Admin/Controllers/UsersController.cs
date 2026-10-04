using CarRentoWeb.Models;
using CarRentoWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CarRentoWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var customers = await _userManager.GetUsersInRoleAsync(SD.Role_Customer);
            var data = customers.Select(u => new {
                id=u.Id,
                email=u.Email,
                isLocked = u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.Now
            });
            return Json(new { data });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Lock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            user.LockoutEnd = DateTimeOffset.Now.AddYears(100);
            await _userManager.UpdateAsync(user);

            return Json(new { success = true });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Unlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            user.LockoutEnd = DateTimeOffset.Now;
            await _userManager.UpdateAsync(user);

            return Json(new { success = true });
        }
    }
}
