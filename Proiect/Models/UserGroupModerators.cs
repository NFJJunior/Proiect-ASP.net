using Proiect.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proiect.Models
{
    [PrimaryKey(nameof(UserId), nameof(GroupId))]
    public class UserGroupModerators
    {
        
        public string UserId { get; set; }
        public int GroupId { get; set; }
        public bool isModerator { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual Group Group { get; set; }

    }
}
