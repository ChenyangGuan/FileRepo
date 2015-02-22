using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RoleBase.Models;
using System.IO;
namespace RoleBase.Controllers
{
    [Authorize(Roles = "Developer")]
    public class DevelopersController : Controller
    {
        //
        // GET: /Developers/
        public ActionResult Index()
        {
            return View();
        }

        //
        // POST: /Developers/
        [HttpPost]
        public ActionResult Index(Files f)
        {
            foreach (var file in f.files)
            {
                if (file.ContentLength > 0)
                {
                    var filename = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/Uploads/"), filename);
                    file.SaveAs(path);
                }

            }
            return Content("Success");
        }
	}
}