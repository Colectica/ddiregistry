using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Ddi.Registry.Web.Controllers
{
    public class HelpController : Controller
    {
        //
        // GET: /Help/

        public ActionResult Index()
        {
            return View();
        }

		[Authorize(Roles = "admin,SuperAdmin")]
		public ActionResult Administrator()
		{
			return View();
		}

    }
}
