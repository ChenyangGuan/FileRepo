using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RoleBase.Models;
using System.IO;
using IdentitySample.Models;
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
        public string Index(Files f)
        {
            if (f.files != null)
            {
                
                var folderpath = Server.MapPath("~/Uploads/");
                if (!Directory.Exists(folderpath)) Directory.CreateDirectory(folderpath);
                foreach (var file in f.files)
                {
                    if (file.ContentLength > 0 && file!=null)
                    {
                        var filename = Path.GetFileName(file.FileName);
                        var path = Path.Combine(folderpath, filename);
                        file.SaveAs(path);
                    }
                  
                }
                return "Successful.";
            }
            else
            {
                return "Error.";
            }
            
        }
	}
}