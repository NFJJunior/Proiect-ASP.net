using Proiect.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proiect.Models
{
    public class Group
    {

        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime Date { get; set; }

        public int? CategoryId { get; set; }

        public string ?UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public virtual Category? Category { get; set; }

        public virtual ICollection<Message>? Messages { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? Categ { get; set; }

    }
}
