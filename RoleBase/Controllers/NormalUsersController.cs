
using RoleBase.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace RoleBase.Controllers
{
    [Authorize(Roles = "NormalUser" )]
    public class NormalUsersController : AsyncController
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
            Files dir = new Files(id, "", dirinfo.Name, Wrapfullpath(dirinfo.FullName), dirinfo.LastAccessTime.ToString());
            id = id * 10;
            bool empty = true;
            foreach (DirectoryInfo directory in dirinfo.GetDirectories())
            {
                empty = false;
                Files newdir = new Files(++id, "", directory.Name, Wrapfullpath(directory.FullName),directory.LastAccessTime.ToString());
                newdir = Searchfile(directory.FullName, id);
                if (id > 1000) dir.state = "closed";
                dir.AddChild(newdir);

            }
            
            foreach (FileInfo file in dirinfo.GetFiles("*.*"))
            {
                empty = false;
                Files newfile = new Files(++id, (file.Length / 1024).ToString() + "KB", file.Name, Wrapfullpath(file.FullName), file.LastAccessTime.ToString());
                //dir.state = "closed";
                dir.AddChild(newfile);
            }
            if (empty)
            {
                Files newfile = new Files(++id,"0KB", "NULL","NULL","NULL");
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
            //Dictionary<String, Object> result = new Dictionary<string, object>();
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
        public string viewFile(string fullpath)
        {
            fullpath = Server.MapPath(fullpath);
            try
            {
                FileStream fs = new FileStream(fullpath, FileMode.Open, FileAccess.Read);
                StreamReader rd = new StreamReader(fullpath);
                string result="";
                string curline;
               while((curline = rd.ReadLine()) != null)
                {
                 result+=curline+"<br/>";
   
                 }

               rd.Close();
               return result;
                //return Content(result, "text/xml");
                //return File(fs, "application/text");
            }
            catch
            {
                return null;
            }
            
           
        }

        private void SearchDepen(string filename, XDocument doc, ref List<string> folder)
        {
            string fp = filename;           
            filename = filename.Substring(filename.LastIndexOf("\\") + 1);
            var tmp = doc.Element("FileDependency");
            IEnumerable<XElement> fn =
    from el in tmp.Elements() select el;


            foreach (var f in fn)
            {
                if (Server.MapPath(f.Element("FullPath").Value)== fp)
                {
                    
                    if (f.Element("Dependencies").Value == "")
                    {
                        
                        continue;
                    }
                    var denpendentFiles = f.Element("Dependencies").Elements("FileFullPath");
                    string folderpath = Server.MapPath("~/App_Data/");
                    folderpath +=Path.GetFileName(fp)  + "_dependencies";
                    Directory.CreateDirectory(folderpath);
                    DirectoryInfo depenency = new DirectoryInfo(folderpath);
                    foreach (var defile in denpendentFiles)
                    {
                        string dirpath = depenency.FullName;
                        string newpath = dirpath+"\\"+defile.Value.Substring(defile.Value.LastIndexOf("\\") + 1);
                        System.IO.File.Copy(Server.MapPath(defile.Value),newpath,true);
                        
                    }
                    folder.Add(depenency.FullName);
                }
            }
            
        }
        
        private void CheckDepen(List<string>fileset,ref List<string>folder)
        {
            string path = System.Web.HttpContext.Current.Server.MapPath("~\\App_Data\\FileDependency.xml");
            XDocument doc;
            try
            {
                doc = XDocument.Load(path);
            }
            catch (System.IO.FileNotFoundException)
            {
                return;
            }
            foreach (string f in fileset)
            {
                if(f!=null)
                SearchDepen(f, doc, ref folder);
            }
        }

        public void clearTrash()
        {
            DirectoryInfo app=new DirectoryInfo(Server.MapPath("~/App_Data"));
            foreach (DirectoryInfo d in app.GetDirectories())
            {

                Directory.Delete(d.FullName, true);
            }
        }
        //GET
        //Download
        [AllowAnonymous]
        public  ActionResult Download(string fullpath)
        {
            AsyncManager.OutstandingOperations.Increment();
            bool depen = false;
            if (fullpath.IndexOf("[depen]") != -1)
            {
                depen = true;
                fullpath = fullpath.Remove(0, 7);
            }
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
                    List<string> folderlist = new List<string>(folderarray);
                    List<string> filelist = new List<string>(filearrary);
                    folderlist.RemoveAll(item => item == null);
                    filelist.RemoveAll(item => item == null);
                    if (depen)
                    {
                        List<string> fileset = new List<string>();
                        foreach (string f in filearrary)
                        {
                            fileset.Add(f);
                        }
                        foreach (string d in folderarray)
                        {
                            if (d != null)
                            {
                                DirectoryInfo dir = new DirectoryInfo(d);
                                foreach (FileInfo f in dir.GetFiles())
                                {
                                    fileset.Add(f.FullName);
                                }
                            }
                        }
                        CheckDepen(fileset, ref folderlist);
                    }
                    
                    var zipResult = new ZipResult(filelist, folderlist);                  
                    zipResult.FileName = "download.zip";
                    
                    return zipResult;
                
            }
            else  
            {
                int folderindex = 0;
                string[] folderarray = new string[1];
                //single folder
                if (fullpath.IndexOf("[folder]") != -1)
                {
                    int foldermark = fullpath.IndexOf("[folder]") + 9;                 
                    string _folderpath = fullpath.Substring(foldermark, fullpath.Length - foldermark);
                    _folderpath = Server.MapPath(_folderpath);
                    folderarray[folderindex++] = _folderpath;
                    List<string> foldset = new List<string>(folderarray);
                    
                    if (depen)
                    {
                        DirectoryInfo dir = new DirectoryInfo(_folderpath);
                        List<string> files = new List<string>();
                        foreach (FileInfo f in dir.GetFiles())
                        {
                            files.Add(f.FullName);
                        }
                        CheckDepen(files, ref foldset);
                    }
                    var zipResult = new ZipResult(foldset);
                    zipResult.FileName = "download.zip";
                    
                    return zipResult;
                }
                else 
                {
                    //single file
                    if (!depen)
                    {
                        int dot = fullpath.LastIndexOf(".") + 1;
                        string pattern = fullpath.Substring(dot, fullpath.Length - dot);
                        string contentType = "application/" + pattern;
                        int divide = fullpath.LastIndexOf("\\") + 1;
                        string _fullpath = Server.MapPath(fullpath);
                        string filename = fullpath.Substring(divide, fullpath.Length - divide);
                        return File(_fullpath, contentType, filename);
                    }
                    else
                    {
                        List<string> fileset = new List<string>();
                        fileset.Add(Server.MapPath(fullpath));
                        List<string> folder=new List<string>();
                        CheckDepen(fileset, ref folder);
                        var zipResult = new ZipResult(fileset,folder);
                        zipResult.FileName = "download.zip";
                        
                        return zipResult;                        
                    }
                }
            }
            
        }
	}
}
        #endregion
    