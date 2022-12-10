using Proiect.Models;
using System.ComponentModel.DataAnnotations;

namespace Proiect.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }
        public string Content { get; set; }

        public DateTime Date { get; set; }

        public int? GroupId { get; set; }
        public string? userId { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public virtual Group? Group { get; set; }
    }
}
