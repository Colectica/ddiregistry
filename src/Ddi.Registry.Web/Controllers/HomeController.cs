using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ddi.Registry.Web.Models;
using Ddi.Registry.Data;

namespace Ddi.Registry.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";

            return View();
        }

        public ActionResult About()
        {
            return View();
        }

		public ActionResult Tools()
		{
			return View();
		}

		public ActionResult RegistrySource()
		{
			return View();
		}

        [HttpPost]
        public ActionResult Index(SearchModel model)
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction("Search", "Agency", new { term = model.Term });
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
