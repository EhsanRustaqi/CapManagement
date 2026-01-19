using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace CapManagement.Shared.Models
{
    public class Expense : BaseEntity
    {
        [JsonPropertyName("ExpenseId")]
        [Key]
        public Guid ExpenseId { get; set; } = new Guid();
        [Required] 
        public Guid CarId { get; set; }
        [ForeignKey(nameof(CarId))] public Car? Car { get; set; }

        [Required] public Guid CompanyId { get; set; }

        [Required] public ExpenseType Type { get; set; } // Maintenance, APK, etc.

        [Required][Range(0, double.MaxValue)] public decimal Amount { get; set; } // total cost paid by owner

        [Required][Range(0, 100)] public decimal VatPercent { get; set; } = 21m;

        [NotMapped] public decimal VatAmount => Math.Round(Amount * VatPercent / (100 + VatPercent), 2);
        [NotMapped] public decimal NetAmount => Amount - VatAmount;

        [Required] public DateTime ExpenseDate { get; set; }

        public int Quarter => ((ExpenseDate.Month - 1) / 3) + 1; // for quarterly VAT return
    }

    public enum ExpenseType
    {
        Maintenance,
        Repair,
        APK,
        Insurance,
        Fuel,
        Other
    }
}
