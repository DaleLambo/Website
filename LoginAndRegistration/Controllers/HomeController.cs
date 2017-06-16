using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LoginAndRegistration.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        [Authorize] // Restricts users who meet the authentication requirements.
        public ActionResult Index()
        {
            return View();
        }
    }
}