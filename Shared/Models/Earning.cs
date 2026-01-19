using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CapManagement.Shared.Models
{
    public class Earning : BaseEntity
    {
        [Key]
        [JsonPropertyName("earningId")]
        public Guid EarningId { get; set; } = Guid.NewGuid();

        // 🔹 FK → Contract
        [Required(ErrorMessage = "Contract is required.")]
        [JsonPropertyName("contractId")]
        public Guid ContractId { get; set; }

        [ForeignKey(nameof(ContractId))]
        public virtual Contract Contract { get; set; }

        // 🔹 FK → Settlement (weekly or monthly payout grouping)
        [JsonPropertyName("settlementId")]
        public Guid? SettlementId { get; set; }

        [ForeignKey(nameof(SettlementId))]
        public virtual Settlement? Settlement { get; set; }

        // 🔹 Platform type (Uber, Bolt, SnelEenTaxi, SumUp, etc.)
        [Required(ErrorMessage = "Platform type is required.")]
        [JsonPropertyName("platform")]
        public PlatformType Platform { get; set; }

        // 🔹 Financial fields
        [Required(ErrorMessage = "Gross income is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Gross income must be non-negative.")]
        [DataType(DataType.Currency)]
        [JsonPropertyName("grossIncome")]
        public decimal GrossIncome { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "BTW must be between 0% and 100%.")]
        [JsonPropertyName("btwPercentage")]
        public decimal BtwPercentage { get; set; } = 21m; // Default: NL BTW 21%

        [NotMapped]
        [JsonPropertyName("btwAmount")]
        public decimal BtwAmount => Math.Round(GrossIncome * (BtwPercentage / 100), 2);

        [NotMapped]
        [JsonPropertyName("netIncome")]
        public decimal NetIncome => GrossIncome - BtwAmount;

        // 🔹 Date of income entry (week ending date, for example)
        [Required]
        [DataType(DataType.Date)]
        [JsonPropertyName("incomeDate")]
        public DateTime IncomeDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Company ID is required.")]
        [JsonPropertyName("companyId")]
        public Guid CompanyId { get; set; } // FK to Company
        [Required]
        public DateTime WeekStart { get; set; }
        [Required]
        public DateTime WeekEnd { get; set; }
    }

    public enum PlatformType
    {
        Uber,
        Bolt,
        SnelEenTaxi,
        SumUp
    }
}
