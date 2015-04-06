﻿using System;
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
        private int id;
        public DevelopersController() { id = 0; }
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

       
        public string[] GetFiles(string str)
        {
            string[] result;
            result = str.Split(';');
            result=result.Take(result.Count() - 1).ToArray();
            return result;
        }
        public void SearchDepen(string filename,XDocument doc,ref List<Files>dataset){
            filename = filename.Substring(filename.LastIndexOf("\\") + 1);
            var tmp = doc.Element("FileDependency");
            IEnumerable<XElement> fn =
    from el in tmp.Elements() select el;
    
            
            foreach (var f in fn)
            {
                if (f.Element("FileName").Value == filename)
                {
                    var denpendentFiles = f.Element("Dependencies").Elements("FileFullPath");
                    foreach (var defile in denpendentFiles)
                    {                        
                        string fileP = defile.Value;
                        FileInfo file = new FileInfo(Server.MapPath(fileP));
                        Files newfile = new Files(++id, (file.Length / 1024).ToString() + "KB", file.Name,fileP,null,"",filename);
                        dataset.Add(newfile);
                    }
                }
            }
           

    }
        //GET
        public ActionResult ShowDependency(string filename)
        {
            string[] fileset = GetFiles(filename);
            
            string path = System.Web.HttpContext.Current.Server.MapPath("~\\App_Data\\FileDependency.xml");
            XDocument doc;
            try
            {
               doc = XDocument.Load(path);
            }
            catch (System.IO.FileNotFoundException)
            {
                return null;
            }
            List<Files> dataset = new List<Files>();
            foreach (string f in fileset)
            {
               SearchDepen(f,doc,ref dataset);
            }

            return Json(dataset, JsonRequestBehavior.AllowGet);
        }

	}
}