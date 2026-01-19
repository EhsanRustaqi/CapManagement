using CapManagement.Client.IServiceClient;
using CapManagement.Shared;
using CapManagement.Shared.DtoModels.DriverDtoModels;
using CapManagement.Shared.Models;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace CapManagement.Client.ServiceClient
{


    /// <summary>
    /// Client-side service responsible for communicating with Expense API endpoints.
    /// Handles CRUD operations, archiving, restoring, and pagination of expenses.
    /// </summary>
    public class DriverServiceClient : IDriverSericeClient
    {
        private readonly HttpClient _httpClient;

        public DriverServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResponse<DriverDto>> CreateDriverAsync(CreateDriverDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Driver", dto);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error: Status={response.StatusCode}, Content={errorContent}");
                    return new ApiResponse<DriverDto>
                    {
                        Success = false,
                        Errors = new List<string> { $"Server returned {response.StatusCode}: {errorContent}" }
                    };
                }

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<DriverDto>>();
                return result ?? new ApiResponse<DriverDto>
                {
                    Success = false,
                    Errors = new List<string> { "Empty response from server." }
                };
            }
            catch (System.Text.Json.JsonException ex)
            {
                Console.WriteLine($"JSON Deserialization Error: {ex.Message}");
                return new ApiResponse<DriverDto>
                {
                    Success = false,
                    Errors = new List<string> { $"Invalid JSON response: {ex.Message}" }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                return new ApiResponse<DriverDto>
                {
                    Success = false,
                    Errors = new List<string> { $"Unexpected error: {ex.Message}" }
                };
            }
        }

        public async Task<ApiResponse<PagedResponse<DriverDto>>> GetDriversAsync(Guid companyId, int pageNumber = 1, int pageSize = 10)
        {
            var response = await _httpClient.GetAsync($"api/Driver?companyId={companyId}&pageNumber={pageNumber}&pageSize={pageSize}");
            return await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<DriverDto>>>()
                   ?? new ApiResponse<PagedResponse<DriverDto>> { Success = false, Errors = new List<string> { "Empty response from server." } };
        }




        public async Task<ApiResponse<DriverDto>> GetDriverByIdAsync(Guid driverId, Guid companyId)
        {
            var response = await _httpClient.GetAsync($"api/Driver/{driverId}?companyId={companyId}");
            return await response.Content.ReadFromJsonAsync<ApiResponse<DriverDto>>()
                   ?? new ApiResponse<DriverDto> { Success = false, Errors = new List<string> { "Empty response from server." } };
        }






        public async Task<ApiResponse<bool>> UpdateDriverAsync(DriverDto driver)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Driver/{driver.DriverId}", driver);
            return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
                   ?? new ApiResponse<bool> { Success = false, Errors = new List<string> { "Empty response from server." } };
        }






        public async Task<ApiResponse<bool>> ArchiveDriverAsync(Guid driverId, Guid companyId)
        {
            if (companyId == Guid.Empty)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Errors = new List<string> { "Company ID is required." }
                };
            }

            try
            {
                var response = await _httpClient.PatchAsync($"api/Driver/{driverId}/archive?companyId={companyId}", null);
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    return new ApiResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Errors = new List<string>()
                    };
                }

                var content = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Errors = new List<string> { $"HTTP {response.StatusCode}: {content}" }
                    };
                }

                // Attempt to deserialize JSON response (for 400 Bad Request with message)
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                return result ?? new ApiResponse<bool>
                {
                    Success = false,
                    Errors = new List<string> { $"Invalid response format: {content}" }
                };
            }
            catch (System.Text.Json.JsonException ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Errors = new List<string> { $"JSON deserialization error: {ex.Message}" }
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Errors = new List<string> { $"Client error: {ex.Message}" }
                };
            }
        }

        //public async Task<ApiResponse<bool>> RestoreDriverAsync(Guid driverId, Guid companyId)
        //{
        //    var response = await _httpClient.PatchAsync($"api/Driver/{driverId}/restore?companyId={companyId}", null);
        //    return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
        //           ?? new ApiResponse<bool> { Success = false, Errors = new List<string> { "Empty response from server." } };
        //}

        public async Task<ApiResponse<bool>> RestoreDriverAsync(Guid driverId, Guid companyId)
        {
            try
            {
                var response = await _httpClient.PatchAsync($"api/Driver/{driverId}/restore?companyId={companyId}", null);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error: Status={response.StatusCode}, Content={errorContent}");
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Errors = new List<string> { $"Server returned {response.StatusCode}: {errorContent}" }
                    };
                }
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                if (result == null)
                {
                    Console.WriteLine("Deserialization returned null");
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Errors = new List<string> { "Empty or invalid response from server." }
                    };
                }
                return result;
            }
            catch (System.Text.Json.JsonException ex)
            {
                
                return new ApiResponse<bool>
                {
                    Success = false,
                    Errors = new List<string> { $"Invalid JSON response: {ex.Message}" }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                return new ApiResponse<bool>
                {
                    Success = false,
                    Errors = new List<string> { $"Unexpected error: {ex.Message}" }
                };
            }
        }

        public async Task<ApiResponse<PagedResponse<DriverDto>>> GetArchivedDriversAsync(Guid companyId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Driver/archived?companyId={companyId}&pageNumber={pageNumber}&pageSize={pageSize}");
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error: Status={response.StatusCode}, Content={errorContent}");
                    return new ApiResponse<PagedResponse<DriverDto>>
                    {
                        Success = false,
                        Errors = new List<string> { $"Server returned {response.StatusCode}: {errorContent}" }
                    };
                }
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<DriverDto>>>();
                if (result == null)
                {
                    Console.WriteLine("Deserialization returned null");
                    return new ApiResponse<PagedResponse<DriverDto>>
                    {
                        Success = false,
                        Errors = new List<string> { "Empty or invalid response from server." }
                    };
                }
                return result;
            }
            catch (System.Text.Json.JsonException ex)
            {
               
                return new ApiResponse<PagedResponse<DriverDto>>
                {
                    Success = false,
                    Errors = new List<string> { $"Invalid JSON response: {ex.Message}" }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                return new ApiResponse<PagedResponse<DriverDto>>
                {
                    Success = false,
                    Errors = new List<string> { $"Unexpected error: {ex.Message}" }
                };
            }
        }
    }
}
