using Proiect.Data;
using Proiect.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Proiect.Models;

namespace Proiect.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;
        public MessagesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Edit(int id)
        {
            Message? msg = db.Messages.Find(id);

            if (msg == null)
                return Redirect("/Groups/Index");

            if (msg.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                return View(msg);

            TempData["message"] = "Nu i a tau comentariu!";

            return Redirect("/Groups/Index");
        }

        [HttpPost]
        public IActionResult Edit(int id, Message requestComment)
        {
            Message msg = db.Messages.Find(id);

            if (ModelState.IsValid)
            {    
                msg.Content = requestComment.Content;
                db.SaveChanges();

                return Redirect("/Groups/Show/" + msg.GroupId);
            }
            
            return View(requestComment);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            Message msg = db.Messages.Find(id);

            if (msg.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Messages.Remove(msg);
                db.SaveChanges();

                return Redirect("/Groups/Show/" + msg.GroupId);
            }

            TempData["message"] = "Nu i a tau comentariu!";

            return Redirect("/Groups/Index");
        }
    }
}
