using ACUEDUCTO2.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ACUEDUCTO2.Components.Account
{
    internal sealed class IdentityUserAccessor(IServiceScopeFactory serviceScopeFactory, IdentityRedirectManager redirectManager)
    {
        private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        private readonly IdentityRedirectManager _redirectManager = redirectManager ?? throw new ArgumentNullException(nameof(redirectManager));

        public async Task<ApplicationUser> GetRequiredUserAsync(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.GetUserAsync(context.User);

            if (user is null)
            {
                _redirectManager.RedirectToWithStatus("Account/InvalidUser", $"Error: Unable to load user with ID '{userManager.GetUserId(context.User)}'.", context);
            }

            return user!;
        }
    }
}
