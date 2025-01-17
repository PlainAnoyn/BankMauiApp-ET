using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ET.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Example of creating a user with a specific role
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "user@example.com"),
                new Claim(ClaimTypes.Role, "Admin")
            }, "Test authentication type");

            var user = new ClaimsPrincipal(identity);
            return Task.FromResult(new AuthenticationState(user));
        }

        public void NotifyAuthenticationStateChanged()
        {
            var authState = GetAuthenticationStateAsync();
            NotifyAuthenticationStateChanged(authState);
        }

        public void MarkUserAsAuthenticated(string email)
        {
            var claims = new[] { new Claim(ClaimTypes.Name, email) };
            var identity = new ClaimsIdentity(claims, "apiauth_type");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }

        public void MarkUserAsLoggedOut()
        {
            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }
    }
}