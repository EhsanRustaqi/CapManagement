using CapManagement.Shared;
using CapManagement.Shared.DtoModels.CompanyDtoModels;
using CapManagement.Shared.Models;
using CapManagement.Client.IServiceClient;
using System.Net.Http;
using System.Net.Http.Json;

using System.Text.Json;


namespace CapManagement.Client.ServiceClient
{
    /// <summary>
    /// Client-side service responsible for communicating with Expense API endpoints.
    /// Handles CRUD operations, archiving, restoring, and pagination of expenses.
    /// </summary>
    public class CompanyServiceClient: ICompanyServiceClient
    {

  private readonly HttpClient _httpClient;
        public CompanyServiceClient(HttpClient httpClient) // ✅ Use HttpClient directly
        {
            _httpClient = httpClient;
        }


        public async Task<ApiResponse<bool>> ArchiveCompanyAsync(Guid companyId)
    {
        try
        {
            if (companyId == Guid.Empty)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Errors = new List<string> { "Company ID cannot be empty." }
                };
            }

            var response = await _httpClient.PatchAsync($"api/Company/{companyId}/archive", null);
            if (response == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Errors = new List<string> { "No response received from server." }
                };
            }

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                return result ?? new ApiResponse<bool>
                {
                    Success = false,
                    Errors = new List<string> { "Failed to deserialize response." }
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Errors = new List<string> { $"HTTP error: {response.StatusCode} - {response.ReasonPhrase}" }
            };
        }
        catch (HttpRequestException httpEx)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Errors = new List<string> { $"HTTP error archiving company: {httpEx.Message}" }
            };
        }
        catch (JsonException jsonEx)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Errors = new List<string> { $"JSON parse error: {jsonEx.Message}" }
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Errors = new List<string> { $"Unexpected error archiving company: {ex.Message}" }
            };
        }
    }

    public async Task<ApiResponse<CompanyDto>> CreateCompanyAsync(CreateCompanyDto dto, string userId)
    {
        try
        {
            if (dto == null)
            {
                return new ApiResponse<CompanyDto>
                {
                    Success = false,
                    Errors = new List<string> { "CreateCompanyDto cannot be null." }
                };
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                return new ApiResponse<CompanyDto>
                {
                    Success = false,
                    Errors = new List<string> { "User ID cannot be null or empty." }
                };
            }

                var response = await _httpClient.PostAsJsonAsync("api/Company", dto);
                if (response == null)
            {
                return new ApiResponse<CompanyDto>
                {
                    Success = false,
                    Errors = new List<string> { "No response received from server." }
                };
            }

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<CompanyDto>>();
                return result ?? new ApiResponse<CompanyDto>
                {
                    Success = false,
                    Errors = new List<string> { "Failed to deserialize response." }
                };
            }

            return new ApiResponse<CompanyDto>
            {
                Success = false,
                Errors = new List<string> { $"HTTP error creating company: {response.StatusCode} - {response.ReasonPhrase}" }
            };
        }
        catch (HttpRequestException httpEx)
        {
            return new ApiResponse<CompanyDto>
            {
                Success = false,
                Errors = new List<string> { $"HTTP error creating company: {httpEx.Message}" }
            };
        }
        catch (JsonException jsonEx)
        {
            return new ApiResponse<CompanyDto>
            {
                Success = false,
                Errors = new List<string> { $"JSON parse error: {jsonEx.Message}" }
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<CompanyDto>
            {
                Success = false,
                Errors = new List<string> { $"Unexpected error creating company: {ex.Message}" }
            };
        }
    }

    public async Task<ApiResponse<PagedResponse<CompanyDto>>> GetArchivedCompaniesAsync(int pageNumber, int pageSize)
    {
        try
        {
            if (pageNumber < 1)
            {
                return new ApiResponse<PagedResponse<CompanyDto>>
                {
                    Success = false,
                    Errors = new List<string> { "Page number must be greater than or equal to 1." }
                };
            }

            if (pageSize < 1)
            {
                return new ApiResponse<PagedResponse<CompanyDto>>
                {
                    Success = false,
                    Errors = new List<string> { "Page size must be greater than or equal to 1." }
                };
            }

            var response = await _httpClient.GetFromJsonAsync<ApiResponse<PagedResponse<CompanyDto>>>(
                $"api/Company/archived?pageNumber={pageNumber}&pageSize={pageSize}");

            return response ?? new ApiResponse<PagedResponse<CompanyDto>>
            {
                Success = false,
                Errors = new List<string> { "No data received from server." }
            };
        }
        catch (HttpRequestException httpEx)
        {
            return new ApiResponse<PagedResponse<CompanyDto>>
            {
                Success = false,
                Errors = new List<string> { $"HTTP error fetching archived companies: {httpEx.Message}" }
            };
        }
        catch (JsonException jsonEx)
        {
            return new ApiResponse<PagedResponse<CompanyDto>>
            {
                Success = false,
                Errors = new List<string> { $"JSON parse error: {jsonEx.Message}" }
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PagedResponse<CompanyDto>>
            {
                Success = false,
                Errors = new List<string> { $"Unexpected error fetching archived companies: {ex.Message}" }
            };
        }
    }

        public async Task<ApiResponse<PagedResponse<CompanyDto>>> GetCompaniesAsync(int? pageNumber = null, int? pageSize = null)
        {
            try
            {
                int page = pageNumber ?? 1;
                int size = pageSize ?? 10;

                if (page < 1)
                {
                    return new ApiResponse<PagedResponse<CompanyDto>>
                    {
                        Success = false,
                        Errors = new List<string> { "Page number must be greater than or equal to 1." }
                    };
                }

                if (size < 1)
                {
                    return new ApiResponse<PagedResponse<CompanyDto>>
                    {
                        Success = false,
                        Errors = new List<string> { "Page size must be greater than or equal to 1." }
                    };
                }

                var url = $"api/Company?pageNumber={page}&pageSize={size}";
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<PagedResponse<CompanyDto>>>(url);

                return response ?? new ApiResponse<PagedResponse<CompanyDto>>
                {
                    Success = false,
                    Errors = new List<string> { "No data received from server." }
                };
            }
            catch (HttpRequestException httpEx)
            {
                return new ApiResponse<PagedResponse<CompanyDto>>
                {
                    Success = false,
                    Errors = new List<string> { $"HTTP error fetching companies: {httpEx.Message}" }
                };
            }
            catch (JsonException jsonEx)
            {
                return new ApiResponse<PagedResponse<CompanyDto>>
                {
                    Success = false,
                    Errors = new List<string> { $"JSON parse error: {jsonEx.Message}" }
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<PagedResponse<CompanyDto>>
                {
                    Success = false,
                    Errors = new List<string> { $"Unexpected error fetching companies: {ex.Message}" }
                };
            }
        }




        public async Task<ApiResponse<CompanyDto>?> GetCompanyByIdAsync(Guid companyId)
        {
            try
            {
                var url = $"api/Company/{companyId}";
                var httpResponse = await _httpClient.GetAsync(url);
                if (!httpResponse.IsSuccessStatusCode)
                {
                    return new ApiResponse<CompanyDto>
                    {
                        Success = false,
                        Errors = new List<string> { $"HTTP error: {httpResponse.StatusCode} - {httpResponse.ReasonPhrase}" }
                    };
                }
                var json = await httpResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Raw API Response (GetCompanyById): {json}");
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var response = JsonSerializer.Deserialize<ApiResponse<CompanyDto>>(json, options);
                Console.WriteLine($"Deserialized Response: {JsonSerializer.Serialize(response)}");
                return response ?? new ApiResponse<CompanyDto>
                {
                    Success = false,
                    Errors = new List<string> { "No data received from server" }
                };
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP error fetching company {companyId}: {httpEx.Message}");
                return new ApiResponse<CompanyDto>
                {
                    Success = false,
                    Errors = new List<string> { $"HTTP error: {httpEx.Message}" }
                };
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"JSON parse error for company {companyId}: {jsonEx.Message}");
                return new ApiResponse<CompanyDto>
                {
                    Success = false,
                    Errors = new List<string> { $"JSON parse error: {jsonEx.Message}" }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error fetching company {companyId}: {ex.Message}, StackTrace: {ex.StackTrace}");
                return new ApiResponse<CompanyDto>
                {
                    Success = false,
                    Errors = new List<string> { $"Unexpected error: {ex.Message}" }
                };
            }
        }

        //public async Task<PagedResponse<CompanyDto>> GetCompaniesAsync(int? pageNumber = null, int? pageSize = null)
        //{
        //    try
        //    {
        //        int page = pageNumber ?? 1;
        //        int size = pageSize ?? 10;
        //        if (page < 1 || size < 1)
        //        {
        //            Console.WriteLine("Invalid page or size parameters.");
        //            return new PagedResponse<CompanyDto>();
        //        }
        //        var url = $"api/Company?pageNumber={page}&pageSize={size}"; // Adjust to api/companies if needed
        //        var httpResponse = await _httpClient.GetAsync(url);
        //        httpResponse.EnsureSuccessStatusCode();
        //        var json = await httpResponse.Content.ReadAsStringAsync();
        //        Console.WriteLine($"Raw API Response: {json}");

        //        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        //        var result = JsonSerializer.Deserialize<PagedResponse<CompanyDto>>(json, options);
        //        return result ?? new PagedResponse<CompanyDto>();
        //    }
        //    catch (HttpRequestException httpEx)
        //    {
        //        Console.WriteLine($"HTTP error: {httpEx.Message}");
        //        return new PagedResponse<CompanyDto>();
        //    }
        //    catch (JsonException jsonEx)
        //    {
        //        Console.WriteLine($"JSON parse error: {jsonEx.Message}");
        //        return new PagedResponse<CompanyDto>();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Unexpected error: {ex.Message}");
        //        return new PagedResponse<CompanyDto>();
        //    }
        //}
        public async Task<ApiResponse<bool>> RestoreCompanyAsync(Guid companyId)
    {
        try
        {
            if (companyId == Guid.Empty)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Errors = new List<string> { "Company ID cannot be empty." }
                };
            }

            var response = await _httpClient.PatchAsync($"api/Company/{companyId}/restore", null);
            if (response == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Errors = new List<string> { "No response received from server." }
                };
            }

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                return result ?? new ApiResponse<bool>
                {
                    Success = false,
                    Errors = new List<string> { "Failed to deserialize response." }
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Errors = new List<string> { $"HTTP error restoring company: {response.StatusCode} - {response.ReasonPhrase}" }
            };
        }
        catch (HttpRequestException httpEx)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Errors = new List<string> { $"HTTP error restoring company: {httpEx.Message}" }
            };
        }
        catch (JsonException jsonEx)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Errors = new List<string> { $"JSON parse error: {jsonEx.Message}" }
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Errors = new List<string> { $"Unexpected error restoring company: {ex.Message}" }
            };
        }
    }

        public async Task<ApiResponse<bool>> UpdateCompanyAsync(CompanyDto company)
        {
            try
            {
                var url = $"api/Company/{company.CompanyId}";
                var content = new StringContent(JsonSerializer.Serialize(company), System.Text.Encoding.UTF8, "application/json");
                var httpResponse = await _httpClient.PutAsync(url, content);
                if (!httpResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($"HTTP error updating company {company.CompanyId}: {httpResponse.StatusCode}");
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Errors = new List<string> { $"HTTP error: {httpResponse.StatusCode}" }
                    };
                }
                var json = await httpResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Raw API Response (UpdateCompany): {json}");
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var response = JsonSerializer.Deserialize<ApiResponse<bool>>(json, options);
                Console.WriteLine($"Deserialized Response: {JsonSerializer.Serialize(response)}");
                return response ?? new ApiResponse<bool>
                {
                    Success = false,
                    Errors = new List<string> { "No data received from server" }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating company with ID {company.CompanyId}: {ex.Message}, StackTrace: {ex.StackTrace}");
                return new ApiResponse<bool>
                {
                    Success = false,
                    Errors = new List<string> { $"Error updating company: {ex.Message}" }
                };
            }
        }
    }
}
