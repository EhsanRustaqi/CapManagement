using CapManagement.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CapManagement.Shared.DtoModels.ExpenseDtoModels
{
    public class ExpenseDto
    {

        [JsonPropertyName("expenseId")]
        public Guid ExpenseId { get; set; }

        [JsonPropertyName("carId")]
        public Guid CarId { get; set; }

        [JsonPropertyName("carName")]
        public string? CarName { get; set; }



        [JsonPropertyName("numberPlate")]
        public string? NumberPlate { get; set; }

        [JsonPropertyName("companyId")]
        public Guid CompanyId { get; set; }

        [JsonPropertyName("type")]
        public ExpenseType Type { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }   // total incl VAT

        [JsonPropertyName("vatPercent")]
        public decimal VatPercent { get; set; }

        [JsonPropertyName("vatAmount")]
        public decimal VatAmount { get; set; }   // computed in service

        [JsonPropertyName("netAmount")]
        public decimal NetAmount { get; set; }   // computed in service

        [JsonPropertyName("expenseDate")]
        public DateTime ExpenseDate { get; set; }

        [JsonPropertyName("quarter")]
        public int Quarter { get; set; }


    }
}
