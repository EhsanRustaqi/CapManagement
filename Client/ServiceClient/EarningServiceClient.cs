using CapManagement.Client.IServiceClient;
using CapManagement.Shared;
using CapManagement.Shared.DtoModels.CarDtoModels;
using CapManagement.Shared.DtoModels.ContractDto;
using CapManagement.Shared.DtoModels.DriverDtoModels;
using CapManagement.Shared.DtoModels.EarningDtoModels;
using CapManagement.Shared.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace CapManagement.Client.ServiceClient
{


    /// <summary>
    /// Client-side service responsible for communicating with Expense API endpoints.
    /// Handles CRUD operations, archiving, restoring, and pagination of expenses.
    /// </summary>
    public class EarningServiceClient : IEarningServiceClient
    {

        private readonly HttpClient _httpClient;
        public EarningServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }




        public Task<ApiResponse<bool>> ArchiveEarningAsync(Guid earningId, Guid companyId)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<EarningDto>> CreateEarningAsync(CreateEarningDto createEarningDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Earning", createEarningDto);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error: Status={response.StatusCode}, Content={errorContent}");
                    return new ApiResponse<EarningDto>
                    {
                        Success = false,
                        Errors = new List<string> { $"Server returned {response.StatusCode}: {errorContent}" }
                    };
                }

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<EarningDto>>();
                return result ?? new ApiResponse<EarningDto>
                {
                    Success = false,
                    Errors = new List<string> { "Empty response from server." }
                };
            }
            catch (System.Text.Json.JsonException ex)
            {
                Console.WriteLine($"JSON Deserialization Error: {ex.Message}");
                return new ApiResponse<EarningDto>
                {
                    Success = false,
                    Errors = new List<string> { $"Invalid JSON response: {ex.Message}" }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                return new ApiResponse<EarningDto>
                {
                    Success = false,
                    Errors = new List<string> { $"Unexpected error: {ex.Message}" }
                };
            }
        }





        public async Task<ApiResponse<List<EarningDto>>> CreateEarningsAsync(List<CreateEarningDto> earnings)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/earnings/create-batch", earnings);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<EarningDto>>>();
            return result ?? new ApiResponse<List<EarningDto>> { Success = false, Message = "Deserialization failed." };
        }

        public Task<ApiResponse<PagedResponse<EarningDto>>> GetArchivedEarningsAsync(Guid companyId, int pageNumber = 1, int pageSize = 10, string? orderBy = null, string? filter = null)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<PagedResponse<EarningDto>>> GetEarningAsync(Guid companyId, int pageNumber = 1, int pageSize = 10, string? orderBy = null, string? filter = null)
        {
            if (companyId == Guid.Empty)
            {
                return new ApiResponse<PagedResponse<EarningDto>>
                {
                    Success = false,
                    Errors = new List<string> { "Invalid company ID" }
                };
            }

            // Build query string
            var queryString = $"?companyId={companyId}&pageNumber={pageNumber}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(orderBy))
            {
                queryString += $"&orderBy={Uri.EscapeDataString(orderBy)}";
            }
            if (!string.IsNullOrEmpty(filter))
            {
                queryString += $"&filter={Uri.EscapeDataString(filter)}";
            }

            try
            {
                // Log for debugging
                Console.WriteLine($"ClientService - Calling API: api/Car{queryString}");

                var response = await _httpClient.GetAsync($"api/Earning{queryString}");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<EarningDto>>>();

                    if (result != null)
                        return result;

                    return new ApiResponse<PagedResponse<EarningDto>>
                    {
                        Success = false,
                        Errors = new List<string> { "Failed to deserialize response" }
                    };
                }

                return new ApiResponse<PagedResponse<EarningDto>>
                {
                    Success = false,
                    Errors = new List<string> { $"HTTP error: {response.StatusCode}" }
                };
            }
            catch (Exception ex)
            {
                // Log for debugging
                Console.WriteLine($"ClientService error: {ex.Message}");
                return new ApiResponse<PagedResponse<EarningDto>>
                {
                    Success = false,
                    Errors = new List<string> { $"Client error: {ex.Message}" }
                };
            }
        }






        public async Task<ApiResponse<EarningDto>> GetEarningByIdAsync(Guid earningId, Guid companyId)
        {

            var url = $"api/Earning/{earningId}?companyId={companyId}";

            var response = await _httpClient.GetAsync(url);

            var raw = await response.Content.ReadAsStringAsync();
            Console.WriteLine("RAW RESPONSE:");
            Console.WriteLine(raw);

            if (!response.IsSuccessStatusCode)
            {
                return new ApiResponse<EarningDto>
                {
                    Success = false,
                    Errors = new List<string> { $"HTTP {response.StatusCode}", raw }
                };
            }

            return JsonSerializer.Deserialize<ApiResponse<EarningDto>>(raw)
                ?? new ApiResponse<EarningDto>
                {
                    Success = false,
                    Errors = new List<string> { "Empty JSON" }
                };

        }






        public Task<ApiResponse<bool>> RestoreEarningAsync(Guid earningId, Guid companyId)
        {
            throw new NotImplementedException();
        }
    }
}
