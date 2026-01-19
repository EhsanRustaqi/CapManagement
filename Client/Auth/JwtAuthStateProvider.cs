using CapManagement.Client.ServiceClient;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace CapManagement.Client.Auth
{
    public class JwtAuthStateProvider : AuthenticationStateProvider
    {
        private readonly LocalStorageService _localStorage;

        public JwtAuthStateProvider(LocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {

            // 🔹 Step 1: Read the JWT token from localStorage
            // If the user has logged in before, the token should be here


            var token = await _localStorage.GetTokenAsync();

            // 🔴 No token → NOT authenticated

            // 🔴 Step 2: If there is NO token
            // This means:
            // - User never logged in OR
            // - User logged out OR
            // - Token was cleared
            // → User is NOT authenticated
            if (string.IsNullOrWhiteSpace(token))
            {

                // Empty identity = anonymous user
                return new AuthenticationState(
                    new ClaimsPrincipal(new ClaimsIdentity()));
            }


            // 🔹 Step 3: Extract claims from the JWT
            // Claims contain user info like:
            // - UserId
            // - Email
            // - Role
            // - Expiration
            var claims = JwtParser.ParseClaimsFromJwt(token)?.ToList();

            // 🔴 No claims → NOT authenticated

            // 🔴 Step 4: If token exists but claims are missing or invalid
            // This can happen if:
            // - Token is corrupted
            // - Token format is invalid
            // - Token parsing failed
            // → Treat user as NOT authenticated
            if (claims == null || !claims.Any())
            {
                return new AuthenticationState(
                    new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // ✅ ONLY HERE user is authenticated


            // ✅ Step 5: Token exists AND claims are valid
            // Create an identity using the claims from the JWT
            // "jwt" tells Blazor this identity came from JWT authentication
            var identity = new ClaimsIdentity(claims, authenticationType: "jwt");

            // Create a user principal from the identity
            var user = new ClaimsPrincipal(identity);


            // ✅ Step 6: Return authenticated user
            // Blazor now knows:
            // - User IS logged in
            // - AuthorizeView should show Authorized content
            // - [Authorize] should allow access
            return new AuthenticationState(user);
        }

        public void NotifyUserAuthentication(string token)
        {
            var claims = JwtParser.ParseClaimsFromJwt(token)?.ToList();

            if (claims == null || !claims.Any())
            {
                return;
            }

            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(
                Task.FromResult(new AuthenticationState(user)));
        }

        public void NotifyUserLogout()
        {
            var anonymousUser =
                new ClaimsPrincipal(new ClaimsIdentity());

            NotifyAuthenticationStateChanged(
                Task.FromResult(new AuthenticationState(anonymousUser)));
        }
    }
}
