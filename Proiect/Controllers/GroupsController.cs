using Proiect.Data;
using Proiect.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;

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

        private void SetAccessRights(int id)
        {
            var curentUser = _userManager.GetUserId(User);

            ViewBag.curentUser = curentUser;
            ViewBag.alreadyJoined = AlreadyJoined(id, curentUser);
            ViewBag.isModerator = IsModerator(id, curentUser);
            ViewBag.isAdmin = User.IsInRole("Admin");
        }

        public IActionResult Show(int id)
        {
            Group group = db.Groups.Include("Category")
                                   .Include("User")
                                   .Include("Messages")
                                   .Include("Messages.User")
                                   .Where(grp => grp.Id == id).First();

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
                return Redirect("/Groups/Show/" + message.GroupId);
            }

            else
            {
                Group art = db.Groups.Include("Category").Include("Messages").Include("User").Include("Messages.User")
                               .Where(art => art.Id == message.GroupId)
                               .First();

                SetAccessRights((int)message.GroupId);

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
            group.UserId = _userManager.GetUserId(User);
            UserGroupModerators ugr = new UserGroupModerators();
            
            if (ModelState.IsValid)
            {
                db.Groups.Add(group);
                
                db.SaveChanges();

                ugr.UserId = _userManager.GetUserId(User);
                ugr.GroupId = group.Id;
                ugr.isModerator = true;
                db.UserGroupModerators.Add(ugr);

                db.SaveChanges();


                /**/
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
            if (IsModerator(id, _userManager.GetUserId(User)) == true || User.IsInRole("Admin"))
            {
                Group group = db.Groups.Include("Category").Where(grp => grp.Id == id).First();

                group.Categ = GetAllCategories();

                return View(group);
            }

            TempData["message"] = "Nu aveti dreptul de a edita acest grup!";

            return RedirectToAction("Index");
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
        public ActionResult Delete(int id)
        {
            if (IsModerator(id,_userManager.GetUserId(User)) == true || User.IsInRole("Admin"))
            {

                Group article = db.Groups.Include("Messages").Where(art => art.Id == id).First();
                UserGroupModerators ugm = db.UserGroupModerators.Where(art => art.GroupId == id).First();



                db.Groups.Remove(article);
                db.SaveChanges();
                TempData["message"] = "Grupul a fost sters";
            }
            else {
                TempData["message"] = "Nu aveti dreptul de a sterge acest grup!";
            }
            
            
           
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
        public bool IsModerator(int GroupId,string UserId)
        {
            UserGroupModerators? ugr = db.UserGroupModerators
                               .Where(art => art.GroupId == GroupId)
                               .Where(art=>art.UserId==UserId)
                               .FirstOrDefault();
            if (ugr == null)
                return false;
            if (ugr.isModerator == false)
                return false;

            return true;
        }

        public bool AlreadyJoined(int GroupId,string UserId)
        {
            UserGroupModerators? ugr = db.UserGroupModerators
                                        .Where(ugr => ugr.GroupId == GroupId)
                                        .Where(ugr => ugr.UserId == UserId)
                                        .FirstOrDefault();

            if (ugr == null) 
                return false;

            return true;
        }


        public IActionResult IndexNou()
        {
            return View();
        }


        public IActionResult ShowMembers(int id)
        {
            //am folosit hash ca sa nu se repete id-urile
            HashSet<string> usersIdHash = new HashSet<string>();
            //aici le-am luat pe toate din UserGroupModerators
            var usersid = from ugm in db.UserGroupModerators
                          where ugm.GroupId == id
                          select ugm.UserId;
            foreach (var i in usersid)
            {
                usersIdHash.Add(i.ToString());
            }
                

            List<string> userName = new List<string>();
            foreach (var i in usersIdHash)

            {
                var name = db.ApplicationUser.Where(ust => ust.Id == i).First();
                userName.Add(name.UserName);

            }
            ViewBag.usersIdHash = usersIdHash;
            ViewBag.Users = userName;
            ViewBag.GroupId = id;

            //un dictionar
            foreach(var u in userName)
                ViewData[u] = db.ApplicationUser.Where(usr=>usr.UserName==u).First().Id;
                

            return View();
        }


        [HttpPost]
        public ActionResult Join(int id)
        {
            var curentUser = _userManager.GetUserId(User);

            //  verific daca userul face deja parte din grupul
            //  in care vrea sa dea join
            UserGroupModerators? ugm = db.UserGroupModerators
                                .Where(ug => ug.GroupId == id)
                                .Where(ug => ug.UserId == curentUser)
                                .FirstOrDefault();

            if (ugm == null)
            {
                ugm = new UserGroupModerators();
                ugm.UserId = curentUser;
                ugm.GroupId = id;
                ugm.isModerator = false;
                db.UserGroupModerators.Add(ugm);
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

            UserGroupModerators? ugm = db.UserGroupModerators
                                         .Where(ug => ug.GroupId == id)
                                         .Where(ug => ug.UserId == curentUser)
                                         .FirstOrDefault();

            int nrModerators = db.UserGroupModerators
                                 .Where(ug => ug.GroupId == id)
                                 .Where(ug => ug.isModerator == true)
                                 .Count();
                                
            if(ugm != null)
            {
                if (nrModerators > 1)
                {
                    db.UserGroupModerators.Remove(ugm);
                    db.SaveChanges();

                    TempData["message"] = "Ai parasit grupul!";
                }
                else
                    TempData["message"] = "Esti ultimul moderator al grupului!";

            }
            else
                TempData["message"] = "Nici macar nu esti in grup!";

            return RedirectToAction("Index");
        }

        public ActionResult MakeModerator(int groupId,string userId)
        {
            UserGroupModerators ugm = db.UserGroupModerators.Where(grai=>grai.GroupId==groupId).Where(hasa=>hasa.UserId==userId).First();

            ugm.isModerator= true;
            db.SaveChanges();
            TempData["message"] = "Adaugare in grupul de moderatori realizata !";
            return RedirectToAction("Index");

        }
        public ActionResult ShowMy()
        {
           var curentUserId = _userManager.GetUserId(User);
           var groupIds = db.UserGroupModerators.Where(ugm => ugm.UserId == curentUserId);
            List<string> numeGrupuri = new List<string>();
            foreach (var groupid in groupIds) {
                var grup = db.Groups.Where(gru => gru.Id == groupid.GroupId).First().Name;
                numeGrupuri.Add(grup);
            }
            ViewBag.numeGrupuri = numeGrupuri;
            return View();
        }
    }

}




