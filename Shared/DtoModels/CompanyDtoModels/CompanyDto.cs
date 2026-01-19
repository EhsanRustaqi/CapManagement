using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CapManagement.Shared.DtoModels.CompanyDtoModels
{
    public class CompanyDto
    {
        public Guid CompanyId { get; set; }

        [JsonPropertyName("companyName")]
        public string CompanyName { get; set; } = string.Empty;

        [JsonPropertyName("address")]
        public string Address { get; set; } = string.Empty;

        [JsonPropertyName("vatNumber")]
        public string VATNumber { get; set; } = string.Empty;

        public string CompanyEmail { get; set; } = string.Empty;

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }


        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
        public bool Success { get; set; }
    }
}
