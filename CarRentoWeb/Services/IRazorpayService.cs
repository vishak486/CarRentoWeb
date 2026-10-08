namespace CarRentoWeb.Services
{
    public interface IRazorpayService
    {
        Task<string> CreateProviderOrderAsync(long amountInPaise,string receipt);
        bool VerifySignature(string providerOrderId, string providerPaymentId, string signature);
    }
}
