using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace CapManagement.Client.ServiceClient.UserService
{
    public class CurrentUserService
    {
        private readonly AuthenticationStateProvider _authStateProvider;
        private ClaimsPrincipal? _currentUser;
        public string UserEmail { get; private set; } = "Guest";
        public ClaimsPrincipal? CurrentUser => _currentUser;
        public bool IsAuthenticated => _currentUser?.Identity?.IsAuthenticated == true;



        public CurrentUserService(AuthenticationStateProvider authStateProvider)
        {
            _authStateProvider = authStateProvider;
            // Subscribe to auth state changes for reactivity
            _authStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
        }

        private async void OnAuthenticationStateChanged(Task<AuthenticationState> task)
        {
            var authState = await task;
            UpdateUser(authState.User);
        }

        public async Task InitializeAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            UpdateUser(authState.User);
        }


        private void UpdateUser(ClaimsPrincipal user)
        {
            _currentUser = user;
            UserEmail = user.Identity?.IsAuthenticated == true
                ? user.FindFirst(ClaimTypes.Email)?.Value
                ?? user.FindFirst("email")?.Value
                ?? user.Identity.Name
                ?? "User"
                : "Guest";

            // Trigger UI updates in components (via StateHasChanged if needed, but injection handles this)
            NotifyStateChanged?.Invoke();
        }


        public event Action? NotifyStateChanged; // Optional: For manual notifications if needed

        // Dispose subscription on service disposal (good practice)
        public void Dispose()
        {
            _authStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
        }



    }
}
