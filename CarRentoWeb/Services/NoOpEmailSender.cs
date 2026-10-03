using Microsoft.AspNetCore.Identity.UI.Services;

namespace CarRentoWeb.Services
{
    public class NoOpEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // No real email sending yet — just succeed silently.
            return Task.CompletedTask;
        }
    }
}
