using Microsoft.EntityFrameworkCore;
using Proiect.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proiect.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set;}

        [Required(ErrorMessage = "Continutul este obligatoriu!")]
        public string Content { get; set; }

        public DateTime? Date { get; set; }

        public string? UserId { get; set; }

        public int? GroupId { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public virtual Group? Group { get; set; }

    }
}
