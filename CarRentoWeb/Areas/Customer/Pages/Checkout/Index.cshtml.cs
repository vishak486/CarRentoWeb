using CarRentoWeb.Models;
using CarRentoWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using OrderModel = CarRentoWeb.Models.Order;

namespace CarRentoWeb.Areas.Customer.Pages.Checkout
{
    [Area("Customer")]
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly IRazorpayService _razorpay;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;

        public IndexModel(IOrderService orderService, IRazorpayService razorpay, UserManager<ApplicationUser> userManager, IConfiguration config)
        {
            _orderService = orderService;
            _razorpay = razorpay;
            _userManager = userManager;
            _config = config;
        }

        public OrderModel? Order { get; set; }

        public void OnGet()
        {
            // display a simple page - actual order is created when client posts to CreateOrder
        }

        // POST: CreateOrder -> returns JSON with razorpay key and provider order id
        public async Task<IActionResult> OnPostCreateOrderAsync()
        {
            var userId = _userManager.GetUserId(User) ?? throw new InvalidOperationException("User not found");
            var order = await _orderService.CreateOrderFromCartAsync(userId);

            // razorpay expects amount in paise (INR * 100)
            var amountInPaise = (long)(order.Total * 100m);
            var receipt = $"order_{order.OrderId}";

            var providerOrderId = await _razorpay.CreateProviderOrderAsync(amountInPaise, receipt);

            // save provider order id on payment
            var payment = await SaveProviderOrderIdAsync(order.OrderId, providerOrderId);

            var keyId = _config.GetValue<string>("Razorpay:KeyId");
            return new JsonResult(new { key = keyId, providerOrderId, amount = amountInPaise, orderId = order.OrderId });
        }

        // POST: Confirm payment -> receives providerPaymentId, providerOrderId, signature
        public async Task<IActionResult> OnPostConfirmAsync([FromForm] int orderId, [FromForm] string providerPaymentId, [FromForm] string providerOrderId, [FromForm] string signature)
        {
            // verify signature server-side
            var ok = _razorpay.VerifySignature(providerOrderId, providerPaymentId, signature);
            if (!ok)
            {
                await _orderService.MarkPaymentFailedAsync(orderId, "Signature verification failed");
                return new JsonResult(new { success = false, message = "Signature verification failed" });
            }

            // persist success
            await _orderService.MarkPaymentSucceededAsync(orderId, providerPaymentId, signature);
            return new JsonResult(new { success = true });
        }

        private async Task<Payment?> SaveProviderOrderIdAsync(int orderId, string providerOrderId)
        {
            // update the payment record for the order with provider order id
            using var scope = HttpContext.RequestServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CarRentoWeb.Data.ApplicationDbContext>();
            var payment = await db.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
            if (payment is null)
                return null;
            payment.ProviderOrderId = providerOrderId;
            payment.Status = PaymentStatus.Initiated;
            await db.SaveChangesAsync();
            return payment;
        }
    }
}