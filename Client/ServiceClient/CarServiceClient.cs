using CapManagement.Client.IServiceClient;
using CapManagement.Client.Pages.Driver.Driver;
using CapManagement.Shared;
using CapManagement.Shared.DtoModels.CarDtoModels;
using CapManagement.Shared.DtoModels.DriverDtoModels;
using CapManagement.Shared.Models;
using System.Net.Http.Json;

namespace CapManagement.Client.ServiceClient
{


    /// <summary>
    /// Client-side service responsible for communicating with Expense API endpoints.
    /// Handles CRUD operations, archiving, restoring, and pagination of expenses.
    /// </summary>
    public class CarServiceClient : ICarServiceClient
    {
        private readonly HttpClient _httpClient;

        public CarServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<ApiResponse<bool>> ArchiveCarAsync(Guid carId, Guid companyId)
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
                var response = await _httpClient.PatchAsync($"api/Car/{carId}/archive?companyId={companyId}", null);
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

        public async Task<ApiResponse<CarDto>> CreateCarsAsync(CreateCarDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Car", dto);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error: Status={response.StatusCode}, Content={errorContent}");
                    return new ApiResponse<CarDto>
                    {
                        Success = false,
                        Errors = new List<string> { $"Server returned {response.StatusCode}: {errorContent}" }
                    };
                }

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<CarDto>>();
                return result ?? new ApiResponse<CarDto>
                {
                    Success = false,
                    Errors = new List<string> { "Empty response from server." }
                };
            }
            catch (System.Text.Json.JsonException ex)
            {
                Console.WriteLine($"JSON Deserialization Error: {ex.Message}");
                return new ApiResponse<CarDto>
                {
                    Success = false,
                    Errors = new List<string> { $"Invalid JSON response: {ex.Message}" }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                return new ApiResponse<CarDto>
                {
                    Success = false,
                    Errors = new List<string> { $"Unexpected error: {ex.Message}" }
                };
            }
        }

        public async Task<ApiResponse<PagedResponse<CarDto>>> GetArchivedCarsAsync(Guid companyId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Car/archived?companyId={companyId}&pageNumber={pageNumber}&pageSize={pageSize}");
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error: Status={response.StatusCode}, Content={errorContent}");
                    return new ApiResponse<PagedResponse<CarDto>>
                    {
                        Success = false,
                        Errors = new List<string> { $"Server returned {response.StatusCode}: {errorContent}" }
                    };
                }
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<CarDto>>>();
                if (result == null)
                {
                    Console.WriteLine("Deserialization returned null");
                    return new ApiResponse<PagedResponse<CarDto>>
                    {
                        Success = false,
                        Errors = new List<string> { "Empty or invalid response from server." }
                    };
                }
                return result;
            }
            catch (System.Text.Json.JsonException ex)
            {

                return new ApiResponse<PagedResponse<CarDto>>
                {
                    Success = false,
                    Errors = new List<string> { $"Invalid JSON response: {ex.Message}" }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                return new ApiResponse<PagedResponse<CarDto>>
                {
                    Success = false,
                    Errors = new List<string> { $"Unexpected error: {ex.Message}" }
                };
            }
        }





        public async Task<ApiResponse<List<CarDto>>> GetAvailableCarsForContractAsync(Guid companyId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Car/available-for-contract/{companyId}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error: Status={response.StatusCode}, Content={errorContent}");

                    return new ApiResponse<List<CarDto>>
                    {
                        Success = false,
                        Errors = new List<string>
                {
                    $"Server returned {response.StatusCode}: {errorContent}"
                }
                    };
                }

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<CarDto>>>();

                if (result == null)
                {
                    Console.WriteLine("Deserialization returned null");

                    return new ApiResponse<List<CarDto>>
                    {
                        Success = false,
                        Errors = new List<string>
                {
                    "Empty or invalid response from server."
                }
                    };
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HTTP Exception in GetAvailableCarsForContractAsync: {ex}");

                return new ApiResponse<List<CarDto>>
                {
                    Success = false,
                    Errors = new List<string>
            {
                $"Unexpected error while loading available cars: {ex.Message}"
            }
                };
            }
        }

        public async Task<ApiResponse<CarDto>> GetCarByIdAsync(Guid CarId, Guid companyId)
        {
            var response = await _httpClient.GetAsync($"api/Car/{CarId}?companyId={companyId}");
            return await response.Content.ReadFromJsonAsync<ApiResponse<CarDto>>()
                   ?? new ApiResponse<CarDto> { Success = false, Errors = new List<string> { "Empty response from server." } };
        }





        public async Task<CarDto?> GetCarInfoFromRdwAsync(string numberPlate)
        {
            var response = await _httpClient.GetAsync($"api/car/rdw/{numberPlate}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<CarDto>();
        }





        //public async Task<ApiResponse<PagedResponse<CarDto>>> GetCarsAsync(Guid companyId, int pageNumber = 1, int pageSize = 10)
        //{
        //    var response = await _httpClient.GetAsync($"api/Car?companyId={companyId}&pageNumber={pageNumber}&pageSize={pageSize}");
        //    return await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<CarDto>>>()
        //           ?? new ApiResponse<PagedResponse<CarDto>> { Success = false, Errors = new List<string> { "Empty response from server." } };
        //}
        public async Task<ApiResponse<PagedResponse<CarDto>>> GetCarsAsync(
         Guid companyId,
         int pageNumber = 1,
         int pageSize = 10,
         string? orderBy = null,
         string? filter = null)
        {
            if (companyId == Guid.Empty)
            {
                return new ApiResponse<PagedResponse<CarDto>>
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

                var response = await _httpClient.GetAsync($"api/Car{queryString}");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<CarDto>>>();

                    if (result != null)
                        return result;

                    return new ApiResponse<PagedResponse<CarDto>>
                    {
                        Success = false,
                        Errors = new List<string> { "Failed to deserialize response" }
                    };
                }

                return new ApiResponse<PagedResponse<CarDto>>
                {
                    Success = false,
                    Errors = new List<string> { $"HTTP error: {response.StatusCode}" }
                };
            }
            catch (Exception ex)
            {
                // Log for debugging
                Console.WriteLine($"ClientService error: {ex.Message}");
                return new ApiResponse<PagedResponse<CarDto>>
                {
                    Success = false,
                    Errors = new List<string> { $"Client error: {ex.Message}" }
                };
            }
        }

        public async Task<ApiResponse<bool>> RestoreCarAsync(Guid carId, Guid companyId)
        {
            try
            {
                var response = await _httpClient.PatchAsync($"api/Car/{carId}/restore?companyId={companyId}", null);
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

        public async Task<ApiResponse<bool>> UpdateCarAsync(CarDto car)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Car/{car.CarId}", car);
            return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
                   ?? new ApiResponse<bool> { Success = false, Errors = new List<string> { "Empty response from server." } };
        }
    }
}
