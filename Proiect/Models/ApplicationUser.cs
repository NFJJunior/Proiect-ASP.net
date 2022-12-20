using Microsoft.AspNetCore.Identity;

namespace Proiect.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<UserGroupModerators>? UserGroupModerators { get; set; }
    }
}
