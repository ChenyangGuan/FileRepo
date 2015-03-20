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
            if (f.files.Count()>0)
            {
                
                var folderpath = Server.MapPath("~/Uploads/"+GetUser());
                if (!Directory.Exists(folderpath)) Directory.CreateDirectory(folderpath);
                foreach (var file in f.files)
                {
                    if (file!=null && file.ContentLength > 0 )
                    {
                        var filename = Path.GetFileName(file.FileName);
                        var path = Path.Combine(folderpath, filename);
                        file.SaveAs(path);
                    }
                    else
                    {
                        return "Error.Please Choose at least one file.";
                    }
                  
                }
                return "Successful. Upload your files to "+folderpath;
            }
            else
            {
                return "Error.";
            }
            
        }


        //GET
        public string ShowDependency(string filename)
        {
            string path = System.Web.HttpContext.Current.Server.MapPath("~\\App_Data\\FileDependency.xml");
            XDocument doc = XDocument.Load(path);
            var fl = doc.Element("FileDependency").Element("FileList");
            XElement filetag = new XElement("File");
            XElement FileName = new XElement("FileName");
            XElement FullPath = new XElement("FullPath");
            XElement Dependencies = new XElement("Dependencies");
            XElement FileFullPath = new XElement("FileFullPath");
            return "";
        }

	}
}