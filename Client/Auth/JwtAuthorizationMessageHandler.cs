using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
namespace CapManagement.Client.Auth
{
    public class JwtAuthorizationMessageHandler : DelegatingHandler
    {
        private readonly IJSRuntime _js;
        public JwtAuthorizationMessageHandler(IJSRuntime js)
        {
            _js = js;
        }
   
    protected override async Task<HttpResponseMessage> SendAsync(
           HttpRequestMessage request,
           CancellationToken cancellationToken)
    {
        var token = await _js.InvokeAsync<string?>(
            "localStorage.getItem", "accessToken");

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);

    
    }
    }
}

