using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ddi.Registry.Web.Controllers
{
	public class ErrorController : Controller
	{
		public ActionResult General()
		{
			return View();
		}

		public ActionResult NoAccess()
		{
			return View();
		}

		public ActionResult NotFound()
		{
			return View();
		}
	}
}
