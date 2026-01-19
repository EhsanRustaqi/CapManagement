using CapManagement.Shared.DtoModels.EarningDtoModels;
using CapManagement.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CapManagement.Shared.DtoModels.SettlementDtoModels
{
    public class SettlementDto
    {
        [JsonPropertyName("SettlementId")]
        public Guid SettlementId { get; set; }


        [JsonPropertyName("companyId")]
        public Guid CompanyId { get; set; }

        [JsonPropertyName("contractId")]
        public Guid ContractId { get; set; }


        public List<CapManagement.Shared.DtoModels.ContractDto.ContractDto> Contracts { get; set; }

        [JsonPropertyName("periodStart")]
        public DateTime PeriodStart { get; set; }

        [JsonPropertyName("periodEnd")]
        public DateTime PeriodEnd { get; set; }

        [JsonPropertyName("extraCosts")]
        public decimal ExtraCosts { get; set; } = 0;

        [JsonPropertyName("grossAmount")]
        public decimal GrossAmount { get; set; }

        [JsonPropertyName("rentDeduction")]
        public decimal RentDeduction { get; set; }

        [JsonPropertyName("netPayout")]
        public decimal NetPayout { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;


        [JsonPropertyName("confirmedByDriver")]
        public bool ConfirmedByDriver { get; set; } = false;

        [JsonPropertyName("confirmedAt")]
        public DateTime? ConfirmedAt { get; set; }

        [JsonPropertyName("status")]
        public SettlementStatus Status { get; set; } = SettlementStatus.Pending;

        [JsonPropertyName("earnings")]
        public List<EarningDto> Earnings { get; set; } = new();
    }
}
