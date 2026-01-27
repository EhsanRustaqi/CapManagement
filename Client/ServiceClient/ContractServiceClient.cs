using CapManagement.Client.IServiceClient;
using CapManagement.Client.Pages.Driver.Driver;
using CapManagement.Shared;
using CapManagement.Shared.DtoModels.CarDtoModels;
using CapManagement.Shared.DtoModels.ContractDto;
using CapManagement.Shared.DtoModels.DriverDtoModels;
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
    public class ContractServiceClient : IContractServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions; // Injected options
        public ContractServiceClient(HttpClient httpClient, JsonSerializerOptions jsonOptions)
        {
            _httpClient = httpClient;
            _jsonOptions = jsonOptions;
        }





        public async Task<ApiResponse<bool>> ArchiveContractAsync(Guid contractId, Guid companyId)
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
                var response = await _httpClient.PatchAsync($"api/Contract/{contractId}/archive?companyId={companyId}", null);
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




        //public async Task<ApiResponse<ContractDto>> CreateContractAsync(CreateContractDto createContractDto)
        //{
        //    try
        //    {
        //        var response = await _httpClient.PostAsJsonAsync("api/Contract/create", createContractDto, _jsonOptions);
        //        if (!response.IsSuccessStatusCode)
        //        {
        //            var errorContent = await response.Content.ReadAsStringAsync();
        //            Console.WriteLine($"API Error: Status={response.StatusCode}, Content={errorContent}");
        //            return new ApiResponse<ContractDto>
        //            {
        //                Success = false,
        //                Errors = new List<string> { $"Server returned {response.StatusCode}: {errorContent}" }
        //            };
        //        }

        //        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ContractDto>>(_jsonOptions);
        //        return result ?? new ApiResponse<ContractDto>
        //        {
        //            Success = false,
        //            Errors = new List<string> { "Empty response from server." }
        //        };
        //    }
        //    catch (System.Text.Json.JsonException ex)
        //    {
        //        Console.WriteLine($"JSON Deserialization Error: {ex.Message}");
        //        return new ApiResponse<ContractDto>
        //        {
        //            Success = false,
        //            Errors = new List<string> { $"Invalid JSON response: {ex.Message}" }
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Unexpected Error: {ex.Message}");
        //        return new ApiResponse<ContractDto>
        //        {
        //            Success = false,
        //            Errors = new List<string> { $"Unexpected error: {ex.Message}" }
        //        };
        //    }
        //}
        public async Task<ApiResponse<ContractDto>> CreateContractAsync(CreateContractDto createContractDto, Stream? pdfStream = null)
        {
            try
            {
                var content = new MultipartFormDataContent
        {
            { new StringContent(createContractDto.CompanyId.ToString()), "CompanyId" },
            { new StringContent(createContractDto.DriverId.ToString()), "DriverId" },
            { new StringContent(createContractDto.CarId.ToString()), "CarId" },
            { new StringContent(createContractDto.StartDate.ToString("o")), "StartDate" }
        };

                if (createContractDto.EndDate != null)
                    content.Add(new StringContent(createContractDto.EndDate?.ToString("o") ?? ""), "EndDate");

                content.Add(new StringContent(createContractDto.Status.ToString()), "Status");
                content.Add(new StringContent(createContractDto.PaymentAmount.ToString()), "PaymentAmount");
                content.Add(new StringContent(createContractDto.Description ?? ""), "Description");
                content.Add(new StringContent(createContractDto.Conditions ?? ""), "Conditions");

                if (pdfStream != null)
                {
                    var fileContent = new StreamContent(pdfStream);
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                    content.Add(fileContent, "PdfFile", "contract.pdf");
                }

                var response = await _httpClient.PostAsync("api/Contract/create", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<ContractDto>
                    {
                        Success = false,
                        Errors = new List<string> { $"Server returned {response.StatusCode}: {errorContent}" }
                    };
                }

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<ContractDto>>();
                return result ?? new ApiResponse<ContractDto>
                {
                    Success = false,
                    Errors = new List<string> { "Empty response from server." }
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ContractDto>
                {
                    Success = false,
                    Errors = new List<string> { $"Unexpected error: {ex.Message}" }
                };
            }
        }



        public async Task<ApiResponse<PagedResponse<ContractDto>>> GetArchivedContractsAsync(Guid companyId, int pageNumber = 1, int pageSize = 10, string? orderBy = null, string? filter = null)
        {
            if (companyId == Guid.Empty)
            {
                return new ApiResponse<PagedResponse<ContractDto>>
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
                Console.WriteLine($"ClientService - Calling API: api/contract{queryString}");

                var response = await _httpClient.GetAsync($"api/Contract/archived{queryString}");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<ContractDto>>>();

                    if (result != null)
                        return result;

                    return new ApiResponse<PagedResponse<ContractDto>>
                    {
                        Success = false,
                        Errors = new List<string> { "Failed to deserialize response" }
                    };
                }

                return new ApiResponse<PagedResponse<ContractDto>>
                {
                    Success = false,
                    Errors = new List<string> { $"HTTP error: {response.StatusCode}" }
                };
            }
            catch (Exception ex)
            {
                // Log for debugging
                Console.WriteLine($"ClientService error: {ex.Message}");
                return new ApiResponse<PagedResponse<ContractDto>>
                {
                    Success = false,
                    Errors = new List<string> { $"Client error: {ex.Message}" }
                };
            }
        }

        public async Task<ApiResponse<PagedResponse<ContractDto>>> GetContractAsync(Guid companyId, int pageNumber = 1, int pageSize = 10, string? orderBy = null, string? filter = null)
        {
            if (companyId == Guid.Empty)
            {
                return new ApiResponse<PagedResponse<ContractDto>>
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

                var response = await _httpClient.GetAsync($"api/Contract{queryString}");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<ContractDto>>>();

                    if (result != null)
                        return result;

                    return new ApiResponse<PagedResponse<ContractDto>>
                    {
                        Success = false,
                        Errors = new List<string> { "Failed to deserialize response" }
                    };
                }

                return new ApiResponse<PagedResponse<ContractDto>>
                {
                    Success = false,
                    Errors = new List<string> { $"HTTP error: {response.StatusCode}" }
                };
            }
            catch (Exception ex)
            {
                // Log for debugging
                Console.WriteLine($"ClientService error: {ex.Message}");
                return new ApiResponse<PagedResponse<ContractDto>>
                {
                    Success = false,
                    Errors = new List<string> { $"Client error: {ex.Message}" }
                };
            }
        }





        public async Task<ApiResponse<ContractDto>> GetContractByIdAsync(Guid contractId, Guid companyId)
        {
            var response = await _httpClient.GetAsync($"api/Contract/{contractId}?companyId={companyId}");

            return await response.Content.ReadFromJsonAsync<ApiResponse<ContractDto>>()
                ?? new ApiResponse<ContractDto>
                {
                    Success = false,
                    Errors = new List<string> { "Empty response from server." }
                };


        
        }








        public async Task<ApiResponse<byte[]>> GetContractPdfAsync(Guid contractId, Guid companyId)
        {
            try
            {
                if (contractId == Guid.Empty || companyId == Guid.Empty)
                {
                    Console.WriteLine($"Invalid input: ContractId={contractId}, CompanyId={companyId}");
                    return new ApiResponse<byte[]>
                    {
                        Success = false,
                        Errors = new List<string> { "Invalid contract or company ID." }
                    };
                }

                // Log for debugging
                Console.WriteLine($"ClientService - Calling API: api/Contract/{contractId}/pdf?companyId={companyId}");
                var response = await _httpClient.GetAsync($"api/Contract/{contractId}/pdf?companyId={companyId}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error: Status={response.StatusCode}, Content={errorContent}");
                    return new ApiResponse<byte[]>
                    {
                        Success = false,
                        Errors = new List<string> { $"Server returned {response.StatusCode}: {errorContent}" }
                    };
                }

                // Read PDF as byte array
                var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    Console.WriteLine("Empty or invalid PDF content received");
                    return new ApiResponse<byte[]>
                    {
                        Success = false,
                        Errors = new List<string> { "Empty or invalid PDF content." }
                    };
                }

                return new ApiResponse<byte[]>
                {
                    Success = true,
                    Data = pdfBytes
                };
            }
            catch (System.Text.Json.JsonException ex)
            {
                Console.WriteLine($"JSON Deserialization Error: {ex.Message}");
                return new ApiResponse<byte[]>
                {
                    Success = false,
                    Errors = new List<string> { $"Invalid JSON response: {ex.Message}" }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ClientService Error: {ex.Message}");
                return new ApiResponse<byte[]>
                {
                    Success = false,
                    Errors = new List<string> { $"Client error: {ex.Message}" }
                };
            }
        }





        public async Task<ApiResponse<PagedResponse<ContractDto>>> GetInActiveContractAsync(Guid companyId, int pageNumber = 1, int pageSize = 10, string? orderBy = null, string? filter = null)
        {
            if (companyId == Guid.Empty)
            {
                return new ApiResponse<PagedResponse<ContractDto>>
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

                var response = await _httpClient.GetAsync($"api/Contract/history{queryString}");
                if (response.IsSuccessStatusCode)
                { 
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<ContractDto>>>();

                    if (result != null)
                        return result;

                    return new ApiResponse<PagedResponse<ContractDto>>
                    {
                        Success = false,
                        Errors = new List<string> { "Failed to deserialize response" }
                    };
                }

                return new ApiResponse<PagedResponse<ContractDto>>
                {
                    Success = false,
                    Errors = new List<string> { $"HTTP error: {response.StatusCode}" }
                };
            }
            catch (Exception ex)
            {
                // Log for debugging
                Console.WriteLine($"ClientService error: {ex.Message}");
                return new ApiResponse<PagedResponse<ContractDto>>
                {
                    Success = false,
                    Errors = new List<string> { $"Client error: {ex.Message}" }
                };
            }
        }






        public Task<ApiResponse<bool>> RestoreContractAsync(Guid contractId, Guid companyId)
        {
            throw new NotImplementedException();
        }






        public async Task<ApiResponse<bool>> UpdateContractAsync(UpdateContractDto updateContractDto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Contract/{updateContractDto.ContractId}", updateContractDto);

            return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
          ?? new ApiResponse<bool> { Success = false, Errors = new List<string> { "Empty response from server." } };

            
        }
    }
}
