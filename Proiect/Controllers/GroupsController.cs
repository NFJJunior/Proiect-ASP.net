using Proiect.Data;
using Proiect.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using Ganss.Xss;

namespace Proiect.Controllers
{
    [Authorize]
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;
        public GroupsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        //  Functii ajutatoare
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

        [NonAction]
        public UserGroup? GetUserGroup(int groupId, string userId)
        {
            UserGroup? userGroup = db.UserGroups.Where(ug => ug.GroupId == groupId)
                                                .Where(ug => ug.UserId == userId)
                                                .FirstOrDefault();

            return userGroup;
        }

        [NonAction]
        private void SetAccessRights(int id)
        {
            var curentUser = _userManager.GetUserId(User);

            ViewBag.curentUser = curentUser;
            ViewBag.userGroup = GetUserGroup(id, curentUser);
            ViewBag.isAdmin = User.IsInRole("Admin");
        }

        //  Partea de CRUD
        public IActionResult Index()
        {
            // Alegem sa afisam 3 grupuri pe pagina
            int _perPage = 3;
            var groups = db.Groups.Include("Category");

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }
            int totalItems = groups.Count();

            // Se preia pagina curenta din View-ul asociat
            // Numarul paginii este valoarea parametrului page din ruta
            // /Groups/Index?page=valoare
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);
            // Pentru prima pagina offsetul o sa fie zero
            // Pentru pagina 2 o sa fie 3
            // Asadar offsetul este egal cu numarul de grupuri care au fost deja afisate pe paginile anterioare
            var offset = 0;
            // Se calculeaza offsetul in functie de numarul paginii la care suntem
            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            // Se preiau articolele corespunzatoare pentru fiecare pagina la care ne aflam
            // in functie de offset
            var paginatedGroups = groups.Skip(offset).Take(_perPage);
            // Preluam numarul ultimei pagini
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);
            // Trimitem grupurile cu ajutorul unui ViewBag catre View-ul corespunzator

            ViewBag.Groups = paginatedGroups;

            return View();
        }

        public IActionResult Show(int id)
        {
            Group? group = db.Groups.Include("Category")
                                    .Include("Messages")
                                    .Include("Messages.User")
                                    .Where(grp => grp.Id == id)
                                    .FirstOrDefault();

            if (group == null)
                return RedirectToAction("Index");

            SetAccessRights(id);

            return View(group);
        }

        [HttpPost]
        public IActionResult Show([FromForm] Message message)
        {
            message.Date = DateTime.Now;
            message.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Messages.Add(message);
                db.SaveChanges();

                return RedirectToAction("Show", message.GroupId);
            }

            Group group = db.Groups.Include("Category")
                                   .Include("Messages")
                                   .Include("Messages.User")
                                   .Where(grp => grp.Id == message.GroupId)
                                   .First();

            SetAccessRights((int)message.GroupId);

            return View(group);
        }

        public IActionResult New()
        {
            Group group = new Group();
            
            group.AllCategories = GetAllCategories();

            return View(group);
        }

        [HttpPost]
        public IActionResult New(Group group)
        {
            var sanitizer = new HtmlSanitizer();
            group.Date = DateTime.Now;
            UserGroup ug = new UserGroup();
            
            if (ModelState.IsValid)
            {
                group.Description = sanitizer.Sanitize(group.Description);
                db.Groups.Add(group);
                db.SaveChanges();

                ug.UserId = _userManager.GetUserId(User);
                ug.GroupId = group.Id;
                ug.IsModerator = true;
                ug.IsAccepted = true;
                db.UserGroups.Add(ug);
                db.SaveChanges();

                TempData["message"] = "Grupul a fost adaugat";

                return RedirectToAction("Index");
            }

            group.AllCategories = GetAllCategories();

            return View(group);
        }

        public IActionResult Edit(int id)
        {
            Group? group = db.Groups.Include("Category")
                                    .Include("Messages")
                                    .Include("Messages.User")
                                    .Where(grp => grp.Id == id)
                                    .FirstOrDefault();

            if (group == null)
                return RedirectToAction("Index");

            UserGroup? userGroup = GetUserGroup(id, _userManager.GetUserId(User));

            if ((userGroup != null && userGroup.IsModerator) || User.IsInRole("Admin"))
            {
                if (group == null)
                    return RedirectToAction("Index");

                group.AllCategories = GetAllCategories();

                return View(group);
            }

            TempData["message"] = "Nu aveti dreptul de a edita acest grup!";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Edit(int id, Group requestGroup)
        {
            var sanitizer = new HtmlSanitizer();
            Group group = db.Groups.Where(grp => grp.Id == id)
                                   .First();

            if (ModelState.IsValid)
            {
                requestGroup.Description=sanitizer.Sanitize(requestGroup.Description);
                group.Name = requestGroup.Name;
                group.Description = requestGroup.Description;
                group.CategoryId = requestGroup.CategoryId;
                db.SaveChanges();

                TempData["message"] = "Articolul a fost modificat!";

                return RedirectToAction("Index");
            }

            requestGroup.AllCategories = GetAllCategories();

            return View(requestGroup);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            UserGroup userGroup = GetUserGroup(id, _userManager.GetUserId(User));

            if ((userGroup != null && userGroup.IsModerator) || User.IsInRole("Admin"))
            {

                Group group = db.Groups.Include("Messages")
                                       .Where(grp => grp.Id == id)
                                       .First();

                db.Groups.Remove(group);
                db.SaveChanges();

                TempData["message"] = "Grupul a fost sters";
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul de a sterge acest grup!";
            }
            
            return RedirectToAction("Index");
        }

        public IActionResult ShowMembers(int id)
        {
            Group? group = db.Groups.Where(grp => grp.Id == id)
                                    .FirstOrDefault();

            if (group == null)
                return RedirectToAction("Index");

            var users = db.UserGroups.Include("User");
            ViewBag.users = users;

            SetAccessRights(id);

            /*//  am folosit hash ca sa nu se repete id-urile
            HashSet<string> usersIdHash = new HashSet<string>();

            //  aici le-am luat pe toate din UserGroup
            var usersId = from ug in db.UserGroups
                          where ug.GroupId == id
                          select ug.UserId;

            foreach (var i in usersId)
            {
                usersIdHash.Add(i.ToString());
            }

            List<string> userName = new List<string>();
            foreach (var i in usersIdHash)
            {
                var name = db.ApplicationUser.Where(usr => usr.Id == i)
                                             .First();
                userName.Add(name.UserName);

            }
            ViewBag.usersIdHash = usersIdHash;
            ViewBag.Users = userName;
            ViewBag.GroupId = id;

            //  un dictionar
            foreach (var u in userName)
                ViewData[u] = db.ApplicationUser.Where(usr => usr.UserName == u)
                                                .First().Id;*/

            return View();
        }

        [HttpPost]
        public IActionResult Join(int id)
        {
            var curentUser = _userManager.GetUserId(User);

            //  verific daca userul face deja parte din grupul
            //  in care vrea sa dea join
            UserGroup? ug = GetUserGroup(id, curentUser);

            if (ug == null)
            {
                ug = new UserGroup();
                ug.UserId = curentUser;
                ug.GroupId = id;
                ug.IsModerator = false;
                ug.IsAccepted = false;
                db.UserGroups.Add(ug);
                db.SaveChanges();

                TempData["message"] = "Welcome in the group!";
            }
            else
                TempData["message"] = "Esti deja in grup vere!";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Leave(int id)
        {
            var curentUser = _userManager.GetUserId(User);

            UserGroup? ug = GetUserGroup(id, curentUser);

            int nrModerators = db.UserGroups.Where(ug => ug.GroupId == id)
                                            .Where(ug => ug.IsModerator == true)
                                            .Count();
                                
            if(ug != null)
            {
                if (ug.IsModerator && nrModerators < 2)
                {
                    TempData["message"] = "Esti ultimul moderator al grupului!";
                }
                else
                {
                    db.UserGroups.Remove(ug);
                    db.SaveChanges();

                    TempData["message"] = "Ai parasit grupul!";
                }

            }
            else
                TempData["message"] = "Nici macar nu esti in grup!";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult MakeModerator(int groupId, string userId)
        {
            UserGroup ug = GetUserGroup(groupId, userId);

            ug.IsModerator = true;
            db.SaveChanges();

            TempData["message"] = "Adaugare in grupul de moderatori realizata!";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RemoveUser(int groupId, string userId)
        {
            UserGroup ug = GetUserGroup(groupId, userId);

            db.UserGroups.Remove(ug);
            db.SaveChanges();

            TempData["message"] = "User - ul a fost eliminat din grup!";

            return RedirectToAction("Index");
        }

        public IActionResult ShowMy()
        {
            var curentUserId = _userManager.GetUserId(User);
            var groups = db.UserGroups.Include("Group")
                                      .Where(ug => ug.UserId == curentUserId).Where(usr=>usr.IsAccepted==true);

            ViewBag.groups = groups;

            return View();
        }

        public IActionResult JoinRequests(int id)
        {
            var users = db.UserGroups.Include("User")
                                     .Where(u => u.IsAccepted == false);

            ViewBag.users = users;

            SetAccessRights(id);

            return View();
        }

        [HttpPost]
        public IActionResult AcceptRequest(int groupId, string userId)
        {
            UserGroup ug = GetUserGroup(groupId, userId);

            ug.IsAccepted = true;
            db.SaveChanges();

            return RedirectToAction("JoinRequests", groupId);
        }

    }
}




