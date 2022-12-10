using Microsoft.AspNetCore.Identity;

namespace Proiect.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual UserGroupModerators? UserGroupModerators { get; set; }
    }
}
