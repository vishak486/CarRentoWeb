using CarRentoWeb.Data;
using CarRentoWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace CarRentoWeb.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _db;

        public OrderService(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<Order> CreateOrderFromCartAsync(string userId)
        {
            var cartItems = await _db.CartItems
               .Include(ci => ci.Car)
               .Where(ci => ci.Cart!.UserId == userId)
               .ToListAsync();

            if (cartItems.Count == 0)
                throw new InvalidOperationException("Cart is empty");

            foreach (var ci in cartItems)
            {
                bool alreadyBooked = await _db.OrderItems
                    .AnyAsync(oi => oi.CarId == ci.CarId
                        && oi.Order!.Status == OrderStatus.Completed
                        && ci.RentalStartDate < oi.RentalEndDate
                        && ci.RentalEndDate > oi.RentalStartDate);

                if (alreadyBooked)
                    throw new InvalidOperationException(
                        $"{ci.Car?.CarName} was just booked by another customer for your dates. Please remove it from your cart and choose different dates.");
            }

            var order = new Order
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                OrderItems = new List<OrderItem>()
            };

            decimal subTotal = 0m;
            foreach (var ci in cartItems)
            {
                var days = Math.Max(1, (int)Math.Ceiling((ci.RentalEndDate - ci.RentalStartDate).TotalDays));
                var lineTotal = ci.PricePerDay * days;

                var oi = new OrderItem
                {
                    CarId = ci.CarId,
                    RentalStartDate = ci.RentalStartDate,
                    RentalEndDate = ci.RentalEndDate,
                    PricePerDay = ci.PricePerDay,
                    LineTotal = lineTotal
                };
                order.OrderItems.Add(oi);
                subTotal += lineTotal;
            }
            order.SubTotal = subTotal;
            order.Tax = 0m; // adjust if needed
            order.Total = order.SubTotal + order.Tax;

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                _db.Orders.Add(order);
                await _db.SaveChangesAsync();

                var payment = new Payment
                {
                    OrderId = order.OrderId,
                    Amount = order.Total,
                    Status = PaymentStatus.Initiated
                };
                _db.Payments.Add(payment);

                // clear cart items
                _db.CartItems.RemoveRange(cartItems);

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                // reload navigation
                await _db.Entry(order).Collection(o => o.OrderItems).LoadAsync();
                return order!;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task MarkPaymentSucceededAsync(int orderId, string providerPaymentId, string providerSignature)
        {
            var payment = await _db.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (payment is null || order is null)
                throw new InvalidOperationException("Order or payment not found");

            payment.ProviderPaymentId = providerPaymentId;
            payment.ProviderSignature = providerSignature;
            payment.Status = PaymentStatus.Success;
            payment.PaidAt = DateTime.UtcNow;

            order.Status = OrderStatus.Completed;

            await _db.SaveChangesAsync();
        }

        public async Task MarkPaymentFailedAsync(int orderId, string reason)
        {
            var payment = await _db.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (payment is null || order is null)
                throw new InvalidOperationException("Order or payment not found");

            payment.Status = PaymentStatus.Failed;
            order.Status = OrderStatus.Failed;

            await _db.SaveChangesAsync();
        }
    }
}
