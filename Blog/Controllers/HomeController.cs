using Blog.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Blog.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("ListCategories");
        }

        public ActionResult ListCategories()
        {
            using (var db = new BlogDbContext())
            {
                var categories = db.Categories
                    .Include(c => c.Articles)
                    .OrderBy(c => c.Name)
                    .ToList();

                return View(categories);
            }
        }

        public ActionResult ListArticles(int? categoryId)
        {
            if (categoryId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new BlogDbContext())
            {
                var articles = db.Articles
                    .Where(a=>a.CategoryId == categoryId)
                    .Include(c => c.Author)
                    .Include(a=>a.Tags)
                    .ToList();

                return View(articles);
            }
        }
    }
}