using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace FileRepository.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminsController : Controller
    {
        
        // GET: Admins
        public ActionResult Index()
        {
            return View();
        }

        private string[] GetFiles(string str, char spli)
        {
            string[] result;
            result = str.Split(spli);
            result = result.Take(result.Count() - 1).ToArray();
            return result;
        }
        //GET
        public string DeleteFiles(string filename)
        {
            string result = "Success";
            string[] files = GetFiles(filename, ';');
            try
            {
                foreach (string f in files)
                {
                    string path = Server.MapPath(f);
                    FileInfo deletefile = new FileInfo(path);
                    deletefile.Delete();
                }
            }
            catch
            {
                result = "Error";
            }
            return result;
        }

        //GET
        public string RenameFolder(string path, string name)
        {
           
            if (path == "~\\Uploads\\") 
                return "Can't Rename this Folder.";
            
            string Folderpath = Server.MapPath(path);
            string newpath = Folderpath.Substring(0, Folderpath.LastIndexOf("\\")+1);
            newpath += name;
            try
            {
                Directory.Move(Folderpath, newpath);
            }
            catch
            {
                return "Can't Rename this Folder.";
            }
            
            return "Success";
        }
    }
}