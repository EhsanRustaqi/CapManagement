using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CapManagement.Shared.DtoModels.CompanyDtoModels
{
    public class CreateCompanyDto
    {
        [Required(ErrorMessage = "Company name is required.")]
        [StringLength(100, ErrorMessage = "Company name cannot exceed 100 characters.")]
        [JsonPropertyName("companyName")]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required for invoicing and compliance.")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
        [JsonPropertyName("address")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "VAT number is required for tax compliance.")]
        [StringLength(15, ErrorMessage = "VAT number cannot exceed 15 characters.")]
        [RegularExpression(@"^[A-Z]{2}[0-9A-Z]+$", ErrorMessage = "Invalid VAT number format (e.g., NL123456789B01).")]
        [JsonPropertyName("vatNumber")]
        public string VATNumber { get; set; } = string.Empty;



        public string CompanyEmail { get; set; } = string.Empty;
    }
}

