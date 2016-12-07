using Blog.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Blog.Controllers.Admin
{
    public class UserController : Controller
    {
        // GET: User
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        // GET: User/List
        public ActionResult List()
        {
            using (var database = new BlogDbContext())
            {
                var users = database.Users.ToList();

                var admins = GetAdminUserNames(users, database);

                return View(users);
            }
        }

        private HashSet<string> GetAdminUserNames(List<BlogUser> users, BlogDbContext blogDB)
        {
            var userManager = new UserManager<BlogUser>(new UserStore<BlogUser>(blogDB));

            var admins = new HashSet<string>();

            foreach (var user in users)
            {
                if (userManager.IsInRole(user.Id, "Admin"))
                {
                    admins.Add(user.UserName);
                }
            }

            return admins;
        }
    }
}