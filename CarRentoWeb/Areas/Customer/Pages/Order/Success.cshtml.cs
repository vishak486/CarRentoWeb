using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarRentoWeb.Areas.Customer.Pages.Order
{
    public class SuccessModel : PageModel
    {
        // optional: show the order id if passed as query string or route value
        [BindProperty(SupportsGet = true)]
        public int? OrderId { get; set; }

        public void OnGet()
        {
            // If you want to load more order details, inject ApplicationDbContext and load here.
        }
    }
}