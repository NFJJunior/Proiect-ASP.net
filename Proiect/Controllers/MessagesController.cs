using Proiect.Data;
using Proiect.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Proiect.Models;

namespace Proiect.Controllers
{
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext db;
        public MessagesController(
        ApplicationDbContext context
        )
        {
            db = context;
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            Message msg = db.Messages.Find(id);
                db.Messages.Remove(msg);
                db.SaveChanges();
                return Redirect("/Groups/Show/" + msg.GroupId);
        }

        public IActionResult Edit(int id)
        {
            Message comm = db.Messages.Find(id);
           
                return View(comm);
        }

        [HttpPost]

        public IActionResult Edit(int id, Message requestComment)
        {
            Message comm = db.Messages.Find(id);

            if (ModelState.IsValid)
            {    
                    comm.Content = requestComment.Content;
                    db.SaveChanges();
                    return Redirect("/Groups/Show/" + comm.GroupId);
            }
            else
            {
                return View(requestComment);
            }

        }
    }
}
