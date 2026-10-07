using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentoWeb.Models
{
    public enum PaymentStatus
    {
        NotStarted,
        Initiated,
        Success,
        Failed,
        Refunded
    }
    public class Payment
    {
        public int PaymentId { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public string? ProviderOrderId { get; set; }
        public string? ProviderPaymentId { get; set; }
        public string? ProviderSignature { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.NotStarted;

        public DateTime? PaidAt { get; set; }
    }
}
