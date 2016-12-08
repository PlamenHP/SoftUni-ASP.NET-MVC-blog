using Blog.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using System.Data.Entity;

namespace Blog.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
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
                ViewBag.Admins = admins;

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


        // GET: User/Edit
        public ActionResult Edit(string id)
        {
            //validate id
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                // Get user from DB
                var user = database.Users.FirstOrDefault(u => u.Id == id);

                // Check if user exists
                if (user == null)
                {
                    return HttpNotFound();
                }


                // Create a view model
                var viewModel = new EditUserViewModel();
                viewModel.User = user;
                viewModel.Roles = GetUserRoles(user, database);

                // Pass the model to the view
                return View(viewModel);
            }
        }

        // POST: User/Edit
        [HttpPost]
        public ActionResult Edit(string id, EditUserViewModel viewModel)
        {
            // Check if model is valid
            if (ModelState.IsValid)
            {
                using (var database = new BlogDbContext())
                {
                    // Get user from database
                    var user = database.Users.FirstOrDefault(u=>u.Id == id);

                    // Check if user exists
                    if (user == null)
                    {
                        return HttpNotFound();
                    }

                    // If password field is not empty, change password
                    if (!string.IsNullOrEmpty(viewModel.Password))
                    {
                        var hasher = new PasswordHasher();
                        var passwordHash = hasher.HashPassword(viewModel.Password);
                        user.PasswordHash = passwordHash;
                    }

                    // Set user properties
                    user.Email = viewModel.User.Email;
                    user.FullName = viewModel.User.FullName;
                    user.UserName = viewModel.User.UserName;
                    this.SetUserRoles(viewModel, user, database);

                    // Save changes
                    database.Entry(user).State = EntityState.Modified;
                    return RedirectToAction("List"); 
                }
            }

            return View(viewModel);
        }

        // GET: User/Delete
        public ActionResult Delete(string id)
        {
            if(id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                // get user from database
                var user = database.Users.FirstOrDefault(u=>u.Id == id);

                // Check if user exists
                if (user == null)
                {
                    return HttpNotFound();
                }
                return View(user);
            }
        }

        // POST: User/Delete
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database  = new BlogDbContext())
            {
                // Get user from database
                var user = database.Users.FirstOrDefault(u=>u.Id == id);

                // Get user articles from databse
                var articles = database.Articles.Where(a=>a.Author.Id == id);

                // Delete user articles
                foreach (var article in articles)
                {
                    database.Articles.Remove(article);
                }

                // Delete user and save changes
                database.Users.Remove(user);
                database.SaveChanges();

                return RedirectToAction("List");
            }
        }

        private void SetUserRoles(EditUserViewModel viewModel, BlogUser user, BlogDbContext database)
        {
            var userManager = HttpContext.GetOwinContext()
                                        .GetUserManager<ApplicationUserManager>();

            foreach (var role in viewModel.Roles)
            {
                if (role.IsSelected && !userManager.IsInRole(user.Id, role.Name))
                {
                    userManager.AddToRole(user.Id, role.Name);
                }
                else if (!role.IsSelected && userManager.IsInRole(user.Id, role.Name))
                {
                    userManager.RemoveFromRole(user.Id, role.Name);
                }
            }
        }

        private IList<Role> GetUserRoles(BlogUser user, BlogDbContext db)
        {
            // Create user manager
            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

            // Get all  application roles
            var roles = db.Roles.
                Select(r => r.Name)
                .OrderBy(r => r)
                .ToList();

            // For each application role, check if the user gas it
            var userRoles = new List<Role>();

            foreach (var roleName in roles)
            {
                var role = new Role { Name = roleName };

                if (userManager.IsInRole(user.Id, roleName))
                {
                    role.IsSelected = true;
                }

                userRoles.Add(role);
            }

            // Return a list with all roles
            return userRoles;
        }
    }
}