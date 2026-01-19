using CapManagement.Client.IServiceClient;
using CapManagement.Shared;
using CapManagement.Shared.Models.Car_CompanyReportModels;
using System.Net.Http.Json;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace CapManagement.Client.ServiceClient
{
    public class ExpenseReportServiceClient : IExpenseReportServiceClient
    {
        private readonly HttpClient _httpClient;
        public ExpenseReportServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<ApiResponse<ExpenseReportSummaryDto>> GetCarExpenseReportAsync(Guid carId, Guid companyId, DateTime fromDate, DateTime toDate)
        {
            var url = $"api/ExpenseReport/report/car?carId={carId}&companyId={companyId}&fromDate={fromDate:O}&toDate={toDate:O}";

            try
            {
                var httpResponse = await _httpClient.GetAsync(url);
                httpResponse.EnsureSuccessStatusCode();  // Throws 4xx/5xx

                var rawJson = await httpResponse.Content.ReadAsStringAsync();  // Raw response
                Console.WriteLine($"Raw Backend JSON: {rawJson}");  // Log to browser console (F12 > Console)

                var result = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<ExpenseReportSummaryDto>>();
                return result ?? new ApiResponse<ExpenseReportSummaryDto>
                {
                    Success = false,
                    Message = "Deserialized to null - check raw JSON in console."
                };
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"JSON Parse Error: {jsonEx.Message} - Path: {jsonEx.Path}");
                return new ApiResponse<ExpenseReportSummaryDto>
                {
                    Success = false,
                    Message = $"JSON mismatch: {jsonEx.Message} (see console for raw JSON)."
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                return new ApiResponse<ExpenseReportSummaryDto>
                {
                    Success = false,
                    Message = $"Request failed: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<ExpenseReportSummaryDto>> GetCompanyExpenseReportAsync(Guid companyId, DateTime fromDate, DateTime toDate)
        {
            {
                var url =
                    $"api/expenses/report/company" +
                    $"?companyId={companyId}" +
                    $"&fromDate={fromDate:O}" +
                    $"&toDate={toDate:O}";

                var response = await _httpClient.GetFromJsonAsync<ApiResponse<ExpenseReportSummaryDto>>(url);

                return response ?? new ApiResponse<ExpenseReportSummaryDto>
                {
                    Success = false,
                    Message = "Failed to load company expense report."
                };
            }
        }
    }
}
