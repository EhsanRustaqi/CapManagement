using CapManagement.Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CapManagement.Shared.DtoModels.ContractDto
{
    public class CreateContractDto
    {
        [Required]
        [JsonPropertyName("companyId")]
        public Guid CompanyId { get; set; }

        [Required]
        [JsonPropertyName("driverId")]
        public Guid DriverId { get; set; }

        [Required]
        [JsonPropertyName("carId")]
        public Guid CarId { get; set; }

        public string CarName { get; set; }

        public string DriverName {  get; set; }

        [Required]
        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime? EndDate { get; set; }

        [Required]
        [JsonPropertyName("status")]
        public ContractStatus Status { get; set; } = ContractStatus.Active;

        [JsonPropertyName("paymentAmount")]
        public decimal PaymentAmount { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("conditions")]
        public string Conditions { get; set; } = string.Empty;

        // For uploading PDF file as Base64 (client → server)
        [JsonPropertyName("pdfContent")]
        public byte[]? PdfContent { get; set; }
    }
}
