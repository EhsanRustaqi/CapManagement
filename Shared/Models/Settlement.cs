using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;

namespace CapManagement.Shared.Models
{
    public class Settlement : BaseEntity
    {
        [Key]
        [JsonPropertyName("settlementId")]
        public Guid SettlementId { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Company ID is required.")]
        [JsonPropertyName("companyId")]
        public Guid CompanyId { get; set; } // FK to Company

        [Required(ErrorMessage = "Contract ID is required.")]
        [JsonPropertyName("contractId")]
        public Guid ContractId { get; set; } // FK to Contract

        [ForeignKey(nameof(ContractId))]
        [JsonPropertyName("contract")]
        public Contract? Contract { get; set; }

        [Required(ErrorMessage = "Period start date is required.")]
        [DataType(DataType.Date)]
        [JsonPropertyName("periodStart")]
        public DateTime PeriodStart { get; set; }

        [Required(ErrorMessage = "Period end date is required.")]
        [DataType(DataType.Date)]
        [JsonPropertyName("periodEnd")]
        public DateTime PeriodEnd { get; set; }

        [JsonPropertyName("status")]
        public SettlementStatus Status { get; set; } = SettlementStatus.Pending;

        [JsonPropertyName("earnings")]
        public ICollection<Earning> Earnings { get; set; } = new List<Earning>();

        [Range(0, double.MaxValue, ErrorMessage = "Extra costs must be non-negative.")]
        [DataType(DataType.Currency)]
        [JsonPropertyName("extraCosts")]
        public decimal ExtraCosts { get; set; } = 0m;

        // Auto-calculated totals
        [JsonPropertyName("grossAmount")]
        public decimal GrossAmount { get; private set; }

        [JsonPropertyName("rentDeduction")]
        public decimal RentDeduction { get; private set; }

        [JsonPropertyName("netPayout")]
        public decimal NetPayout { get; private set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;


        

        [DataType(DataType.DateTime)]
        [JsonPropertyName("confirmedAt")]
        public DateTime? ConfirmedAt { get; set; }

        [JsonPropertyName("confirmedByDriver")]
        public bool ConfirmedByDriver { get; set; } = false;

        /// <summary>
        /// Automatically calculates totals based on earnings, contract rent, and extra costs.
        /// Call this whenever earnings or extra costs are updated.
        /// </summary>
        /// 
        public void RecalculateTotals()
        {
            // 1️⃣ Total gross (sum of net amounts of all earnings)
            GrossAmount = Earnings.Sum(e => e.NetIncome);

            // 2️⃣ Determine how many unique earning weeks are included
            int uniqueWeeks = Earnings
                .Select(e => e.WeekStart)
                .Distinct()
                .Count();

            // 3️⃣ Rent deduction = number of unique earning weeks × contract weekly rent
            RentDeduction = uniqueWeeks * (Contract?.PaymentAmount ?? 0);

            // 4️⃣ Final net payout
            NetPayout = GrossAmount - RentDeduction - ExtraCosts;

            // 5️⃣ Prevent negative payout
            if (NetPayout < 0)
                NetPayout = 0;
        }
        //public void RecalculateTotals()
        //{
        //    // Total gross = sum of net amounts of all earnings
        //    GrossAmount = Earnings.Sum(e => e.NetIncome);

        //    // Rent deduction = number of weeks * contract weekly rent
        //    if (Contract != null)
        //    {
        //        int weeks = (int)Math.Ceiling((PeriodEnd - PeriodStart).TotalDays / 7.0);
        //        RentDeduction = weeks * Contract.PaymentAmount;
        //    }
        //    else
        //    {
        //        RentDeduction = 0;
        //    }

        //    // Final net payout = Gross - Rent - ExtraCosts
        //    NetPayout = GrossAmount - RentDeduction - ExtraCosts;
        //    if (NetPayout < 0) NetPayout = 0; // Prevent negative payout
        //}
    }

    public enum SettlementStatus
    {
        Pending,   // Generated, waiting for review
        Approved,  // Owner/Admin approved
        Paid,      // Payout executed
        Confirmed  // Driver confirmed receipt
    }
}
