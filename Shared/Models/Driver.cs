using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CapManagement.Shared.Models
{
    public class Driver: BaseEntity
    {
        [JsonPropertyName("driverId")]
        public Guid DriverId { get; set; }

        [Required(ErrorMessage = "Driver first name is required.")]
        [StringLength(50, ErrorMessage = "Driver first name cannot exceed 50 characters.")]
        [JsonPropertyName("driverName")]
        public string DriverName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Driver last name is required.")]
        [StringLength(50, ErrorMessage = "Driver last name cannot exceed 50 characters.")]
        [JsonPropertyName("lastName")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required.")]
        [DataType(DataType.Date)]
        [Range(typeof(DateTime), "1/1/1900", "1/1/2010", ErrorMessage = "Date of birth must be between 1900 and 2010.")]
        [JsonPropertyName("dateOfBirth")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "User ID is required to link the driver to an account.")]
        [JsonPropertyName("userId")]
        public Guid UserId { get; set; } // FK to User

        [Required(ErrorMessage = "Phone number is required for contact.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 characters.")]
        [JsonPropertyName("phoneNumber")]
        public string Phonenumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Company ID is required for multi-company isolation.")]
        [JsonPropertyName("companyId")]
        public Guid CompanyId { get; set; } // FK to Company

        [Required(ErrorMessage = "License number is required for legal authorization.")]
        [StringLength(20, ErrorMessage = "License number cannot exceed 20 characters.")]
        [JsonPropertyName("licenseNumber")]
        public string LicenseNumber { get; set; } = string.Empty;

        public ICollection<Contract> Contracts { get; set; } = new List<Contract>(); // All contracts of this driver

        [ForeignKey("CompanyId")]
        [JsonIgnore]
        public virtual Company? Company { get; set; } // Navigation – use your existing Company model
    }
}
