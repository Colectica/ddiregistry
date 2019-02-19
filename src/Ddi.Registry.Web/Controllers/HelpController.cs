using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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

		[Authorize(Roles = "admin")]
		public ActionResult Administrator()
		{
			return View();
		}

    }
}
