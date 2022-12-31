using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proiect.Models
{
    [PrimaryKey(nameof(UserId), nameof(GroupId))]
    public class UserGroup
    {
        public string UserId { get; set; }

        public int GroupId { get; set; }

        public bool IsAccepted { get; set; }

        public bool IsModerator { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public virtual Group? Group { get; set; }
    }
}
