using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RoleBase.Controllers
{
    [Authorize(Roles = "NormalUser")]
    public class NormalUsersController : Controller
    {
        //
        // GET: /NormalUsers/
        public ActionResult Index()
        {
            return View();
        }
	}
}