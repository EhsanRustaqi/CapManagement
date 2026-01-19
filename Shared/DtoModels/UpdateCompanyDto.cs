using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapManagement.Shared.DtoModels
{
    public class UpdateCompanyDto
    {

        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        [StringLength(100)]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string VATNumber { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true; // optional for admin
    }
}
