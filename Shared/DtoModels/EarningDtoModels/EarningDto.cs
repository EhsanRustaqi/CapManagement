using CapManagement.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CapManagement.Shared.DtoModels.EarningDtoModels
{
    public class EarningDto
    {

        [JsonPropertyName("earningId")]
        public Guid EarningId { get; set; }

        [JsonPropertyName("contractId")]
        public Guid ContractId { get; set; }

        [JsonPropertyName("settlementId")]
        public Guid? SettlementId { get; set; }

        [JsonPropertyName("companyId")]
        public Guid CompanyId { get; set; }

        [JsonPropertyName("platform")]
        public PlatformType Platform { get; set; }

        [JsonPropertyName("grossIncome")]
        public decimal GrossIncome { get; set; }

        [JsonPropertyName("btwPercentage")]
        public decimal BtwPercentage { get; set; }

        [JsonPropertyName("btwAmount")]
        public decimal BtwAmount { get; set; }

        [JsonPropertyName("netIncome")]
        public decimal NetIncome { get; set; }

        [JsonPropertyName("incomeDate")]
        public DateTime IncomeDate { get; set; }

        [JsonPropertyName("weekStart")]
        public DateTime WeekStart { get; set; }

        [JsonPropertyName("weekEnd")]
        public DateTime WeekEnd { get; set; }

    }
}
