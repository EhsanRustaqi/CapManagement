using CapManagement.Client.IServiceClient;
using CapManagement.Client.Pages.Driver.Driver;
using CapManagement.Shared;
using CapManagement.Shared.DtoModels.DriverDtoModels;
using CapManagement.Shared.DtoModels.SettlementDtoModels;
using CapManagement.Shared.Models;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace CapManagement.Client.ServiceClient
{


    /// <summary>
    /// Client-side service responsible for communicating with Expense API endpoints.
    /// Handles CRUD operations, archiving, restoring, and pagination of expenses.
    /// </summary>
    public class SettlmentServiceClient : ISettlmentClientService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl = "/api/settlement";  // Adjust if base path differs

        public SettlmentServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }



        public async Task<ApiResponse<SettlementDto>> CreateSettlementAsync(CreateSettlementDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_apiBaseUrl}/create", dto);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<SettlementDto>>();
            return result ?? new ApiResponse<SettlementDto> { Success = false, Message = "Deserialization failed." };
        }





        public async Task<ApiResponse<PagedResponse<SettlementDto>>> GetAllSettlementAsync(Guid companyId, int pageNumber = 1, int pageSize = 10, string? orderBy = null, string? filter = null)
        {
            var response = await _httpClient.GetAsync($"api/Settlement?companyId={companyId}&pageNumber={pageNumber}&pageSize={pageSize}");
            return await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<SettlementDto>>>()
                   ?? new ApiResponse<PagedResponse<SettlementDto>> { Success = false, Errors = new List<string> { "Empty response from server." } };
        }





        public async Task<ApiResponse<SettlementDto>> GetSettlementByIdAsync(Guid settlementId, Guid companyId)
        {
            var url = $"api/Settlement/{settlementId}?companyId={companyId}";
            var response = await _httpClient.GetAsync(url);

            // Capture raw content for debugging cases where HTML is returned
            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new ApiResponse<SettlementDto>
                {
                    Success = false,
                    Errors = new List<string> { $"API Error {response.StatusCode}: {raw}" }
                };
            }

            try
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<SettlementDto>>()
                       ?? new ApiResponse<SettlementDto>
                       { Success = false, Errors = new List<string> { "Empty JSON response." } };
            }
            catch (Exception ex)
            {
                return new ApiResponse<SettlementDto>
                {
                    Success = false,
                    Errors = new List<string>
            {
                $"Deserialization failed: {ex.Message}",
                $"Raw response: {raw}"
            }
                };
            }
        }





        public async Task<byte[]> DownloadSettlementPdfAsync(Guid settlementId, Guid companyId)
        {
            //return await _httpClient.GetByteArrayAsync($"api/settlements/{settlementId}/{companyId}/pdf");
            try
            {
        
                var requestUri = $"api/Settlement/{settlementId}/{companyId}/pdf";
                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

                // Add authorization header if needed
                //var token = await _authStateProvider.GetAuthenticationStateAsync(); // If using CustomAuthStateProvider
                //if (token.User.Identity?.IsAuthenticated == true)
                //{
                //    var authToken = await ((CustomAuthStateProvider)_authStateProvider).GetTokenAsync(); // Get JWT token
                //    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
                //}

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode(); // Throws if not 200 OK

                // Read as bytes for PDF
                var pdfBytes = await response.Content.ReadAsByteArrayAsync();

                if (pdfBytes.Length == 0)
                {
                    throw new InvalidOperationException("PDF response is empty.");
                }

                return pdfBytes;
            }
            catch (HttpRequestException ex)
            {
                //_logger.LogError(ex, "HTTP error downloading PDF for settlement {SettlementId}", settlementId);
                throw new InvalidOperationException($"Failed to download PDF: {ex.Message}");
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error downloading PDF for settlement {SettlementId}", settlementId);
                throw new InvalidOperationException($"Unexpected error downloading PDF: {ex.Message}");
            }
        }
    }
}
    