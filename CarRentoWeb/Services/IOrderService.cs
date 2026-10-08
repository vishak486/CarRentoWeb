using System.Threading.Tasks;
using CarRentoWeb.Models;
namespace CarRentoWeb.Services
{
    public interface IOrderService
    {
        Task<Order> CreateOrderFromCartAsync(string userId);
        Task MarkPaymentSucceededAsync(int orderId, string providerPaymentId, string providerSignature);
        Task MarkPaymentFailedAsync(int orderId, string reason);
    }
}
