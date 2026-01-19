using CapManagement.Client.IServiceClient;
using CapManagement.Client.Pages.Driver.Driver;
using CapManagement.Shared;
using CapManagement.Shared.DtoModels.DriverDtoModels;
using CapManagement.Shared.DtoModels.ExpenseDtoModels;
using CapManagement.Shared.Models;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace CapManagement.Client.ServiceClient
{


    /// <summary>
    /// Client-side service responsible for communicating with Expense API endpoints.
    /// Handles CRUD operations, archiving, restoring, and pagination of expenses.
    /// </summary>
    public class ExpenseServiceClient : IExpenseServiceClient
    {


        /// <summary>
        /// Injected HttpClient used to communicate with backend API.
        /// </summary>
        private readonly HttpClient _httpClient;
        public ExpenseServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        /// <summary>
        /// Archives an expense for a specific company.
        /// Archived expenses are not deleted but hidden from active records which soft deleted is implemented for audit.
        /// </summary>
        public async Task<ApiResponse<bool>> ArchiveExpenseAsync(Guid expenseId, Guid companyId)
        {

            // Validate required company identifier
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

                // Send PATCH request to archive the expense
                var response = await _httpClient.PatchAsync($"api/Expense/{expenseId}/archive?companyId={companyId}", null);
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    return new ApiResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Errors = new List<string>()
                    };
                }


                // Read server response for error details
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


                // Handles JSON parsing issues
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



        /// <summary>
        /// Creates a new expense entry for a company.
        /// </summary>
        public async Task<ApiResponse<ExpenseDto>> CreateExpenseAsync(CreateExpenseDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Expense", dto);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error: Status={response.StatusCode}, Content={errorContent}");
                    return new ApiResponse<ExpenseDto>
                    {
                        Success = false,
                        Errors = new List<string> { $"Server returned {response.StatusCode}: {errorContent}" }
                    };
                }

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<ExpenseDto>>();
                return result ?? new ApiResponse<ExpenseDto>
                {
                    Success = false,
                    Errors = new List<string> { "Empty response from server." }
                };
            }
            catch (System.Text.Json.JsonException ex)
            {
                Console.WriteLine($"JSON Deserialization Error: {ex.Message}");
                return new ApiResponse<ExpenseDto>
                {
                    Success = false,
                    Errors = new List<string> { $"Invalid JSON response: {ex.Message}" }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                return new ApiResponse<ExpenseDto>
                {
                    Success = false,
                    Errors = new List<string> { $"Unexpected error: {ex.Message}" }
                };
            }
        }





        /// <summary>
        /// Retrieves archived expenses for a company with pagination.
        /// </summary>
        public async Task<ApiResponse<PagedResponse<ExpenseDto>>> GetArchivedExpenseAsync(Guid companyId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Expense/archived?companyId={companyId}&pageNumber={pageNumber}&pageSize={pageSize}");
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error: Status={response.StatusCode}, Content={errorContent}");
                    return new ApiResponse<PagedResponse<ExpenseDto>>
                    {
                        Success = false,
                        Errors = new List<string> { $"Server returned {response.StatusCode}: {errorContent}" }
                    };
                }
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<ExpenseDto>>>();
                if (result == null)
                {
                    Console.WriteLine("Deserialization returned null");
                    return new ApiResponse<PagedResponse<ExpenseDto>>
                    {
                        Success = false,
                        Errors = new List<string> { "Empty or invalid response from server." }
                    };
                }
                return result;
            }
            catch (System.Text.Json.JsonException ex)
            {

                return new ApiResponse<PagedResponse<ExpenseDto>>
                {
                    Success = false,
                    Errors = new List<string> { $"Invalid JSON response: {ex.Message}" }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                return new ApiResponse<PagedResponse<ExpenseDto>>
                {
                    Success = false,
                    Errors = new List<string> { $"Unexpected error: {ex.Message}" }
                };
            }
        }




        /// <summary>
        /// Retrieves the active list of  expenses for a company with pagination filter and sorting .
        /// </summary>
        public async Task<ApiResponse<PagedResponse<ExpenseDto>>> GetExpenseAsync(Guid companyId, int pageNumber = 1, int pageSize = 10, string? orderBy = null, string? filter = null)
        {
            try
            {
                var queryString = $"?companyId={companyId}&pageNumber={pageNumber}&pageSize={pageSize}";

                if (!string.IsNullOrWhiteSpace(orderBy))
                    queryString += $"&orderBy={Uri.EscapeDataString(orderBy)}";

                if (!string.IsNullOrWhiteSpace(filter))
                    queryString += $"&filter={Uri.EscapeDataString(filter)}";

                var response = await _httpClient.GetAsync($"api/Expense{queryString}");

                return await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<ExpenseDto>>>()
                       ?? new ApiResponse<PagedResponse<ExpenseDto>> { Success = false };
            }
            catch (Exception ex)
            {
                return new ApiResponse<PagedResponse<ExpenseDto>>
                {
                    Success = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }




        /// <summary>
        /// Retrieves a single expense by its ID for a specific company.
        /// </summary>
        public async Task<ApiResponse<ExpenseDto>> GetExpenseByIdAsync(Guid expenseId, Guid companyId)
        {
            var response = await _httpClient.GetAsync($"api/Expense/{expenseId}?companyId={companyId}");
            return await response.Content.ReadFromJsonAsync<ApiResponse<ExpenseDto>>()
                   ?? new ApiResponse<ExpenseDto> { Success = false, Errors = new List<string> { "Empty response from server." } };
        }




        /// <summary>
        /// Restores a previously archived expense.
        /// </summary>

        public async Task<ApiResponse<bool>> RestoreExpenseAsync(Guid expenseId, Guid companyId)
        {
            try
            {
                var response = await _httpClient.PatchAsync($"api/Expense/{expenseId}/restore?companyId={companyId}", null);
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





        public async Task<ApiResponse<bool>> UpdateExpenseAsync(UpdateExpenseDto expense)
        {
            try
            {
                if (expense == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Expense data is required."
                    };
                }

                var response = await _httpClient.PutAsJsonAsync(
                    $"api/Expense/{expense.ExpenseId}",
                    expense);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content
                        .ReadFromJsonAsync<ApiResponse<bool>>();

                    return error ?? new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Failed to update expense."
                    };
                }

                var result = await response.Content
                    .ReadFromJsonAsync<ApiResponse<bool>>();

                return result ?? new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Invalid response from server."
                };
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"HTTP error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Unexpected error: {ex.Message}"
                };
            }
        }





        public async Task<ApiResponse<PageResult<ExpenseDto>>> GetExpensesByCarIdAsync(Guid carId, Guid companyId, int pageNumber, int pageSize)
        {
            try
            {
                var url =
                    $"api/Expense/by-car/{carId}" +
                    $"?companyId={companyId}" +
                    $"&pageNumber={pageNumber}" +
                $"&pageSize={pageSize}";

                var response = await _httpClient.GetFromJsonAsync<
                    ApiResponse<PageResult<ExpenseDto>>>(url);

                return response ?? new ApiResponse<PageResult<ExpenseDto>>
                {
                    Success = false,
                    Message = "No response from server"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<PageResult<ExpenseDto>>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

    }
}
