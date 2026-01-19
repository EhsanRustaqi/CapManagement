using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapManagement.Shared.Models
{
    public class DriverExpense : BaseEntity
    {
        [Key]
        public Guid DriverExpenseId { get; set; } = Guid.NewGuid();
        [Required(ErrorMessage = "Driver ID is required.")]
        public Guid DriverId { get; set; }

        [ForeignKey(nameof(DriverId))]
        public Driver? Driver { get; set; }

        [Required(ErrorMessage = "Contract ID is required.")]
        public Guid ContractId { get; set; }

        [ForeignKey(nameof(ContractId))]
        public Contract? Contract { get; set; }

        [Required(ErrorMessage = "Expense amount is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be non-negative.")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; } // Total cost

        [Required(ErrorMessage = "Expense type is required.")]
        public DriverExpenseType Type { get; set; } // Fuel, Parking, Toll, Other

        [Required(ErrorMessage = "VAT percent is required.")]
        [Range(0, 100, ErrorMessage = "VAT percent must be between 0 and 100.")]
        public decimal VatPercent { get; set; } = 21m;

        [NotMapped]
        [Display(Name = "VAT Amount")]
        [DataType(DataType.Currency)]
        public decimal VatAmount => Math.Round(Amount * VatPercent / (100 + VatPercent), 2);

        [NotMapped]
        [Display(Name = "Net Amount")]
        [DataType(DataType.Currency)]
        public decimal NetAmount => Amount - VatAmount;

        [Required(ErrorMessage = "Expense date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Expense Date")]
        public DateTime ExpenseDate { get; set; }
    }

    public enum DriverExpenseType
    {
        Fuel,
        Parking,
        Toll,
        Finds,
        Other
    }
}
