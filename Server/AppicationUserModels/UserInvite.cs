using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapManagement.Shared.Models.AppicationUserModels
{
    public class UserInvite : BaseEntity
    {
        public Guid UserInviteId { get; set; }

        public string Email { get; set; } = string.Empty;

        public Guid CompanyId { get; set; }

        public string RoleName { get; set; } = string.Empty;
        // "CompanyAdmin" or "Driver"

        public string Token { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

    }
}
