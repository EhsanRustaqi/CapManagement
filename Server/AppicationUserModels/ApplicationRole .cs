using CapManagement.Shared.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapManagement.Server.AppicationUserModels
{
    public class ApplicationRole : IdentityRole<Guid>
    {

        public bool IsSystemRole { get; set; } = true;
    }
}
