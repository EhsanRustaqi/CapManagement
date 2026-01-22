using CapManagement.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CapManagement.Shared.DtoModels.ContractDto
{
    public class ContractDto
    {

        [JsonPropertyName("contractId")]
        public Guid ContractId { get; set; }

        [JsonPropertyName("companyId")]
        public Guid CompanyId { get; set; }

        [JsonPropertyName("driverId")]
        public Guid DriverId { get; set; }

        [JsonPropertyName("driverName")]
        public string? DriverName { get; set; }

        [JsonPropertyName("carId")]
        public Guid CarId { get; set; }

        [JsonPropertyName("carName")]
        public string? CarName { get; set; }



        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime? EndDate { get; set; }

        [JsonPropertyName("status")]
        public ContractStatus Status { get; set; }

        [JsonPropertyName("paymentAmount")]
        public decimal PaymentAmount { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("conditions")]
        public string Conditions { get; set; } = string.Empty;

        [JsonPropertyName("hasPdf")]
        public bool HasPdf { get; set; }

        [JsonPropertyName("pdfUrl")]
        public string? PdfUrl { get; set; }
    }
}
