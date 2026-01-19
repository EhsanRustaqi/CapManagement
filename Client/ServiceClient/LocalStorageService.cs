using System.Text.Json;
using Microsoft.JSInterop;


namespace CapManagement.Client.ServiceClient
{
    public class LocalStorageService
    {
        private readonly IJSRuntime _jsRuntime;
        public LocalStorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }




        public async Task SetItemAsync(string key, string value)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
        }






        public async Task<string?> GetItemAsync(string key)
        {
            var result = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
            return result;
        }

        public async Task<string?> GetTokenAsync()
        {
            return await _jsRuntime.InvokeAsync<string?>(
                "localStorage.getItem", "accessToken");
        }






        public async Task RemoveItemAsync(string key)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }

        public async Task ClearAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.clear");
        }



        // Typed overloads (e.g., for JSON serialization)
        public async Task SetItemAsync<T>(string key, T value) where T : class
        {
            var json = JsonSerializer.Serialize(value);
            await SetItemAsync(key, json);
        }






        public async Task<T?> GetItemAsync<T>(string key) where T : class
        {
            var json = await GetItemAsync(key);
            return json != null ? JsonSerializer.Deserialize<T>(json) : null;
        }






        public async Task<string[]> GetAllKeysAsync()
        {
            return await _jsRuntime.InvokeAsync<string[]>("localStorage.getAllKeys");
        }
    }
}
