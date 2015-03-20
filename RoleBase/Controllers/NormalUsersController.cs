using RoleBase.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RoleBase.Controllers
{
    [Authorize(Roles = "NormalUser" )]
    public class NormalUsersController : Controller
    {
        //
        // GET: /NormalUsers/
        public ActionResult Index()
        {
            return View();
        }

        //Change the absolute full path to relative path
        private string Wrapfullpath(string fullpath)
        {
            int homeindex = fullpath.IndexOf("Uploads");
            string relative = fullpath.Substring(homeindex, fullpath.Length - homeindex);
            relative = "~\\" + relative;
            return relative;
        }

        //Search Files and return JSON
        #region
        private Files Searchfile(string path, int id)
        {
            DirectoryInfo dirinfo = new DirectoryInfo(path);
            Files dir = new Files(id, "", dirinfo.Name, Wrapfullpath(dirinfo.FullName));
            id = id * 10;
            foreach (DirectoryInfo directory in dirinfo.GetDirectories())
            {
                Files newdir = new Files(++id, "", directory.Name, Wrapfullpath(directory.FullName));
                newdir = Searchfile(directory.FullName, id);
                if (id > 1000) dir.state = "closed";
                dir.AddChild(newdir);

            }
            foreach (FileInfo file in dirinfo.GetFiles("*.*"))
            {
                Files newfile = new Files(++id, (file.Length / 1024).ToString() + "KB", file.Name, Wrapfullpath(file.FullName));
                //dir.state = "closed";
                dir.AddChild(newfile);
            }
            return dir;
        }

        // 
        // GET Home/Getjson
        [AllowAnonymous]
        public ActionResult Getjson()
        {
            List<Files> dataset = new List<Files>();
            Files f;
            f = new Files();
            int index = 1;
            Dictionary<String, Object> result = new Dictionary<string, object>();
            string rootpath = Server.MapPath("~/Uploads/");
            f = Searchfile(rootpath, index);
            dataset.Add(f);
            return Json(dataset, JsonRequestBehavior.AllowGet);
        }
        #endregion

        //View & Download
        #region
        //GET
        //View text
        [AllowAnonymous]
        public ActionResult viewFile(string fullpath)
        {
            fullpath = Server.MapPath(fullpath);
            FileStream fs = new FileStream(fullpath, FileMode.Open, FileAccess.Read);
            return File(fs, "application/text");
           
        }

        


        //GET
        //Download
        [AllowAnonymous]
        public ActionResult Download(string fullpath)
        {
            //multiple files
            if (fullpath.IndexOf(",") != -1)
            {
               
                    string[] patharray = fullpath.Split(',');
                    string[] folderarray=new string[patharray.Length];
                    string[] filearrary = new string[patharray.Length];
                    int i = 0;

                    foreach (string file in patharray)
                    {
                        int folderindex=0;
                        if (file.IndexOf("[folder]") != -1)
                        {
                            int foldermark = file.IndexOf("[folder]") + 9;
                            string _folderpath = file.Substring(foldermark, file.Length - foldermark);
                            _folderpath = Server.MapPath(_folderpath);
                            folderarray[folderindex++] = _folderpath;

                        }
                        else
                        {
                            int divide = file.LastIndexOf("\\") + 1;
                            string _fullpath = Server.MapPath(file);
                            string filename = file.Substring(divide, file.Length - divide);
                            filearrary[i++] = _fullpath;
                            
                        }
                    }
                    var zipResult = new ZipResult(folderarray, filearrary);

                    zipResult.FileName = "download.zip";

                    return zipResult;
                
            }
            else  //single folder
            {
                int folderindex = 0;
                string[] folderarray = new string[1];
                if (fullpath.IndexOf("[folder]") != -1)
                {
                    int foldermark = fullpath.IndexOf("[folder]") + 9;
                    string _folderpath = fullpath.Substring(foldermark, fullpath.Length - foldermark);
                    _folderpath = Server.MapPath(_folderpath);
                    folderarray[folderindex++] = _folderpath;
                    var zipResult = new ZipResult(folderarray);

                    zipResult.FileName = "download.zip";

                    return zipResult;
                }
                else //single file
                {
                    int dot = fullpath.LastIndexOf(".") + 1;
                    string pattern = fullpath.Substring(dot, fullpath.Length - dot);
                    string contentType = "application/" + pattern;
                    int divide = fullpath.LastIndexOf("\\") + 1;
                    string _fullpath = Server.MapPath(fullpath);
                    string filename = fullpath.Substring(divide, fullpath.Length - divide);
                    return File(_fullpath, contentType, filename);
                }
            }
            
        }
	}
}
        #endregion
    