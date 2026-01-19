using CapManagement.Shared.Models;
using CapManagement.Shared.Models.AppicationUserModels;
using Microsoft.AspNetCore.Identity;

namespace CapManagement.Server.AppicationUserModels
{
    public class ApplicationUser:  IdentityUser<Guid>
    {

        //public Guid UserId { get; set; }

        //public string Email { get; set; } = string.Empty;

        //public string PasswordHash { get; set; } = string.Empty;

        public Guid? CompanyId { get; set; } // NULL for Owner
        public Company? Company { get; set; }
        public bool IsDeleted { get; set; } = false;

    }
}
