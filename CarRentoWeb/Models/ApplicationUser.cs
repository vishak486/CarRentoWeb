using Microsoft.AspNetCore.Identity;

namespace CarRentoWeb.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? DrivingLicenseNo { get; set; }
    }
}
