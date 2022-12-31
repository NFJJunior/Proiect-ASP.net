using Microsoft.AspNetCore.Identity;

namespace Proiect.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public virtual ICollection<UserGroup>? UserGroups { get; set; }

        public virtual ICollection<Message>? Message { get; set; }
    }
}
