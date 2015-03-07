using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RoleBase.Models;
using System.IO;
using IdentitySample.Models;
using System.Xml.Linq;
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


        //Get Current User Name
        private string GetUser()
        {
            string path = System.Web.HttpContext.Current.Server.MapPath("~\\App_Data\\CurUser.xml");
            XDocument doc = XDocument.Load(path);
            return doc.Element("CurUser").Element("UserName").Value;
        }

        //
        // POST: /Developers/
        [HttpPost]
        public string Index(Fileset f)
        {
            if (f.files != null)
            {
                
                var folderpath = Server.MapPath("~/Uploads/"+GetUser());
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
                return "Successful. Upload your files to "+folderpath;
            }
            else
            {
                return "Error.";
            }
            
        }
	}
}