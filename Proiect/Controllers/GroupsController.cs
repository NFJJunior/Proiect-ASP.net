using Proiect.Data;
using Proiect.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Proiect.Controllers
{
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;
        public GroupsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        [Authorize(Roles = "User,Admin")]
        public IActionResult Index()
        {
            var groups = db.Groups.Include("Category").Include("User");

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }
            ViewBag.Groups = groups;


            return View();
        }
        public IActionResult Show(int id)
        {
            Group group = db.Groups.Include("Category").Include("User").Include("Messages").Include("Messages.User").
                Where(grp => grp.Id == id).First();

            return View(group);
        }
        /*private void setAccessRights()
        {
            ViewBag.AfisareButoane = false;
            if (User.IsInRole("Editor"))
            {
                ViewBag.AfisareButoane = true;

            }
            ViewBag.UserCurent = _userManager.GetUserId(User);
            ViewBag.EsteAdmin = User.IsInRole("Admin");
        }*/

        [HttpPost]
        public IActionResult Show([FromForm] Message message)
        {
            message.Date = DateTime.Now;
            message.userId = "0f2026e2-9b42-42be-bff8-18de85de6d7e";


            if (ModelState.IsValid)
            {
                db.Messages.Add(message);
                db.SaveChanges();
                return Redirect("/Groups/Show/" + message.GroupId);
            }

            else
            {
                Group art = db.Groups.Include("Category").Include("Messages").Include("User").Include("Messages.User")
                               .Where(art => art.Id == message.GroupId)
                               .First();

                //return Redirect("/Groups/Show/" + comm.GroupId);

                return View(art);
            }
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult New()
        {
            Group group = new Group();
            group.Categ = GetAllCategories();
            return View(group);
        }
        [HttpPost]
        public IActionResult New(Group group)
        {
            group.Date = DateTime.Now;
            group.UserId = "0f2026e2-9b42-42be-bff8-18de85de6d7e";


            if (ModelState.IsValid)
            {
                db.Groups.Add(group);
                db.SaveChanges();
                TempData["message"] = "Grupul a fost adaugat";
                return RedirectToAction("Index");
            }
            else
            {
                group.Categ = GetAllCategories();
                return View(group);
            }
        }

        public IActionResult Edit(int id)
        {

            Group group = db.Groups.Include("Category").Where(grp=>grp.Id==id).First();
            group.Categ = GetAllCategories();

                return View(group);
            

        }

        // Se adauga articolul modificat in baza de date
        [HttpPost]
        public IActionResult Edit(int id, Group requestGroup)
        {
            Group group = db.Groups.Include("Category").Where(grp => grp.Id == id).First();
           requestGroup.Categ = GetAllCategories();

            if (ModelState.IsValid)
            {
                
                    group.Name = requestGroup.Name;
                    group.Description = requestGroup.Description;
                    group.CategoryId = requestGroup.CategoryId;
                    TempData["message"] = "Articolul a fost modificat";
                    db.SaveChanges();
                return RedirectToAction("Index");


            }
            else
            {
                return View(requestGroup);
            }
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int id)
        {
            Group article = db.Groups.Include("Messages").Where(art => art.Id == id).First();

            
            
                db.Groups.Remove(article);
                db.SaveChanges();
                TempData["message"] = "Grupul a fost sters";
            
           
            return RedirectToAction("Index");
        }


        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            // generam o lista de tipul SelectListItem fara elemente
            var selectList = new List<SelectListItem>();

            // extragem toate categoriile din baza de date
            var categories = from cat in db.Categories
                             select cat;

            // iteram prin categorii
            foreach (var category in categories)
            {
                // adaugam in lista elementele necesare pentru dropdown
                // id-ul categoriei si denumirea acesteia
                selectList.Add(new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.CategoryName.ToString()
                });
            }
            
            return selectList;
        }

        public IActionResult IndexNou()
        {
            return View();
        }
    }

}




