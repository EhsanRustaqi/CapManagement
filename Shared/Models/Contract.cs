using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CapManagement.Shared.Models
{
    public class Contract : BaseEntity
    {
        [JsonPropertyName("contractId")]
        public Guid ContractId { get; set; }

        [Required(ErrorMessage = "Company ID is required for multi-company isolation.")]
        [JsonPropertyName("companyId")]
        public Guid CompanyId { get; set; } // FK to Company

        [Required(ErrorMessage = "Driver ID is required for the contract.")]
        [JsonPropertyName("driverId")]
        public Guid DriverId { get; set; } // FK to Driver


        [ForeignKey(nameof(DriverId))]
        public Driver Driver { get; set; }  // Navigation property

        [Required(ErrorMessage = "Car ID is required for the contract.")]
        [JsonPropertyName("carId")]
        public Guid CarId { get; set; } // FK to Car

        [ForeignKey(nameof(CarId))]
        public Car Car { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        [DataType(DataType.Date)]
        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [JsonPropertyName("endDate")]
        public DateTime? EndDate { get; set; } // Nullable for ongoing contracts

        [Required(ErrorMessage = "Status is required.")]
        [JsonPropertyName("status")]
        public ContractStatus Status { get; set; } = ContractStatus.Active; // Enum defined below

        [Range(0, double.MaxValue, ErrorMessage = "Payment amount must be non-negative.")]
        [JsonPropertyName("paymentAmount")]
        public decimal PaymentAmount { get; set; } // Optional payment term

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty; // Optional description

        [Required(ErrorMessage = "Conditions are required for legal clarity.")]
        [StringLength(1000, ErrorMessage = "Conditions cannot exceed 1000 characters.")]
        [JsonPropertyName("conditions")]
        public string Conditions { get; set; } = string.Empty; // Required contract terms

        [JsonPropertyName("pdfContent")]
        public byte[]? PdfContent { get; set; } // Nullable byte array for PDF upload
    }

    // Enum for ContractStatus
    public enum ContractStatus
    {
        Inactive = 0,   // Non-active/default
        Active = 1,     // Active contracts (enforced unique per car)
        Terminated = 2,
        Expired = 3,
        Pending = 4
    }
}