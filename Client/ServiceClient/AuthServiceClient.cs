using CapManagement.Client.Auth;
using CapManagement.Client.IServiceClient;
using CapManagement.Shared.DtoModels.LoginDto;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace CapManagement.Client.ServiceClient
{
    public class AuthServiceClient : IAuthServiceClient
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _js;
        private readonly JwtAuthStateProvider _jwtAuthStateProvider;
        public AuthServiceClient(HttpClient http, IJSRuntime js, JwtAuthStateProvider authStateProvider)
        {
            _http = http;
            _js = js;
            _jwtAuthStateProvider = authStateProvider;
        }

        public async Task<string?> GetTokenAsync()
        {
            return await _js.InvokeAsync<string?>("localStorage.getItem", "accessToken");
        }




        public async Task<bool> IsLoggedInAsync()
        {
            var token = await GetTokenAsync();
            return !string.IsNullOrEmpty(token);
        }





        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/Auth/login", request);

                if (!response.IsSuccessStatusCode)
                {
                    // Optional: read error message from API
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception(string.IsNullOrWhiteSpace(error)
                        ? "Invalid login credentials"
                        : error);
                }

                var result = await response.Content.ReadFromJsonAsync<LoginResponse>()
                             ?? throw new Exception("Empty response from server");

                await _js.InvokeVoidAsync(
                    "localStorage.setItem",
                    "accessToken",
                    result.AccessToken);
                _jwtAuthStateProvider.NotifyUserAuthentication(result.AccessToken);

                return result;
            }
            catch (HttpRequestException ex)
            {
                // Network / server unreachable
                throw new Exception("Unable to reach the server", ex);
            }
            catch (JSException ex)
            {
                // localStorage failure
                throw new Exception("Failed to store authentication token", ex);
            }
            catch (Exception)
            {
                throw; // rethrow so UI can handle it
            }
        }

        public async Task LogoutAsync()
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", "accessToken");
        }
    }
}
