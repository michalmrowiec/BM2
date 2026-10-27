using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using BM2.Server.Data;
using static BM2.Client.Localization.Strings;

namespace BM2.Server.Components.Account;

// Remove the "else if (EmailSender is IdentityNoOpEmailSender)" block from RegisterConfirmation.razor after updating with a real implementation.
internal sealed class IdentityNoOpEmailSender : IEmailSender<ApplicationUser>
{
    private readonly IEmailSender emailSender = new NoOpEmailSender();

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
        emailSender.SendEmailAsync(email, Tr("Confirm your email"), Tr("Please confirm your account by <a href='{0}'>clicking here</a>.", confirmationLink));

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
        emailSender.SendEmailAsync(email, Tr("Reset your password"), Tr("Please reset your password by <a href='{0}'>clicking here</a>.", resetLink));

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
        emailSender.SendEmailAsync(email, Tr("Reset your password"), Tr("Please reset your password using the following code: {0}", resetCode));
}
