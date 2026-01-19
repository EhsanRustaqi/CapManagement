using System.ComponentModel.DataAnnotations;

namespace CapManagement.Server.Controllers
{
    public class CreateContractRequest
    {
        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        public Guid DriverId { get; set; }

        [Required]
        public Guid CarId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal PaymentAmount { get; set; }

        public string? Description { get; set; }

        [Required]
        public string Conditions { get; set; } = string.Empty;

        public IFormFile? PdfFile { get; set; }   // only in controller layer
    }
}
