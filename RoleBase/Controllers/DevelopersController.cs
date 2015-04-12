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
        public string Index(Fileset f,string path)
        {
            if (f.files.Count()>0)
            {
                string folderpath="";
                if (path == "")
                    folderpath = Server.MapPath("~/Uploads/" + GetUser());
                else
                    folderpath = Server.MapPath(path);
                if (!Directory.Exists(folderpath)) Directory.CreateDirectory(folderpath);
                foreach (var file in f.files)
                {
                    if (file!=null && file.ContentLength > 0 )
                    {
                        var filename = Path.GetFileName(file.FileName);
                        var savepath = Path.Combine(folderpath, filename);
                        file.SaveAs(savepath);
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

       
        private string[] GetFiles(string str,char spli)
        {
            string[] result;
            result = str.Split(spli);
            result=result.Take(result.Count() - 1).ToArray();
            return result;
        }
        public void SearchDepen(string filename,XDocument doc,ref List<Files>dataset){
            string fp = filename;
            bool ExistFile = false;
            filename = filename.Substring(filename.LastIndexOf("\\") + 1);
            var tmp = doc.Element("FileDependency");
            IEnumerable<XElement> fn =
    from el in tmp.Elements() select el;
    
            
            foreach (var f in fn)
            {
                if (f.Element("FullPath").Value==fp)
                {
                    ExistFile = true;
                    if (f.Element("Dependencies").Value == "")
                    {
                        Files newfile = new Files(++id, "0KB", "NULL", "NULL", null, "", filename, fp);
                        dataset.Add(newfile);
                        continue;
                    }
                    var denpendentFiles = f.Element("Dependencies").Elements("FileFullPath");
                    foreach (var defile in denpendentFiles)
                    {                        
                        string fileP = defile.Value;
                        FileInfo file = new FileInfo(Server.MapPath(fileP));
                        Files newfile = new Files(++id, (file.Length / 1024).ToString() + "KB", file.Name,fileP,null,"",filename,fp);
                        dataset.Add(newfile);
                    }
                }
            }
            if (!ExistFile)
            {
                Files newfile = new Files(++id,"0KB", "NULL", "NULL", null, "", filename, fp);
                dataset.Add(newfile);
            }

            id = 0;
    }
        //GET
        public ActionResult ShowDependency(string filename)
        {
            string[] fileset = GetFiles(filename,';');
            
            string path = System.Web.HttpContext.Current.Server.MapPath("~\\App_Data\\FileDependency.xml");
            XDocument doc;
            try
            {
               doc = XDocument.Load(path);
            }
            catch (System.IO.FileNotFoundException)
            {
                doc = new XDocument();
                XElement fd = new XElement("FileDependency");
                doc.Add(fd);
                doc.Save(path);
                doc = XDocument.Load(path);
            }
            List<Files> dataset = new List<Files>();
            foreach (string f in fileset)
            {
               SearchDepen(f,doc,ref dataset);
            }

            return Json(dataset, JsonRequestBehavior.AllowGet);
        }

        //GET
        public ActionResult ShowFiles(string files)
        {
            string[] fileset = GetFiles(files,';');          
            List<Files> dataset = new List<Files>();
            foreach (string fileP in fileset)
            {
                FileInfo file = new FileInfo(Server.MapPath(fileP));
                Files newfile = new Files(++id, (file.Length / 1024).ToString() + "KB", file.Name, fileP, null, "");
                dataset.Add(newfile);
            }
            id = 0;
            return Json(dataset, JsonRequestBehavior.AllowGet);
        }


        private void DeleteDepen(ref string filename, ref string deletefile, XDocument doc,ref string path)
        {
            var tmp = doc.Element("FileDependency");
            IEnumerable<XElement> fn =
    from el in tmp.Elements() select el;
            foreach (var f in fn)
            {
                if (f.Element("FullPath").Value == filename)
                {
                    var denpendentFiles = f.Element("Dependencies").Elements("FileFullPath");
                    foreach (var defile in denpendentFiles)
                    {
                        if (defile.Value == deletefile) 
                            defile.Remove();
                    }
                }
            }
            doc.Save(path);
            
        }

        //GET
        public string DeleteDependency(string files)
        {
            string result = "Error";
            string path = System.Web.HttpContext.Current.Server.MapPath("~\\App_Data\\FileDependency.xml");
            XDocument doc;
            try
            {
                doc = XDocument.Load(path);
            }
            catch (System.IO.FileNotFoundException)
            {
                return result;
            }
            string[] request = GetFiles(files,';');
            string filename;
            string deletefile;
            foreach (string re in request)
            {
                filename = re.Substring(0, re.IndexOf("::"));
                deletefile = re.Substring(re.IndexOf("::") + 2);
                DeleteDepen(ref filename, ref deletefile, doc,ref path);
                
            }
            result = "Success";
            return result;
        }

        private void SpecifyDepen(string filename, ref string Depenfile, XDocument doc, ref string path)
        {
            string[] Depenfiles = GetFiles(Depenfile, ';');
            var tmp = doc.Element("FileDependency");
            IEnumerable<XElement> fn =
    from el in tmp.Elements() select el;
            bool ExistFile = false;
            bool ExistDepen = false;
            foreach (var f in fn)
            {
                if (f.Element("FullPath").Value == filename)
                {
                    ExistFile = true;
                    var denpendentFiles = f.Element("Dependencies").Elements("FileFullPath");
                    var dependency = f.Element("Dependencies");
                    foreach (string df in Depenfiles)
                    {
                        foreach (var defile in denpendentFiles)
                        {
                            if (df == defile.Value)
                            {
                                ExistDepen = true;
                                break;
                            }
                        }
                        if (!ExistDepen && df != filename)
                        {
                            XElement newdepen = new XElement("FileFullPath");
                            newdepen.Value = df;
                            dependency.Add(newdepen);
                            
                        }
                        ExistDepen = false;
                    }


                   
                }
            }
            if (!ExistFile)
            {
                XElement newfile = new XElement("File");
                XElement newfileName = new XElement("FileName");
                XElement newfileFullPath = new XElement("FullPath");
                XElement depen = new XElement("Dependencies");
                foreach (string df in Depenfiles)
                {
                    if (df != filename)
                    {
                        XElement newdepen = new XElement("FileFullPath");
                        newdepen.Value = df;
                        
                        
                        depen.Add(newdepen);
                        

                    }
                }
                newfileFullPath.Value = filename;
                newfileName.Value = filename.Substring(filename.IndexOf("\\") + 1);
                newfile.Add(newfileName);
                newfile.Add(newfileFullPath);
                newfile.Add(depen);
                tmp.Add(newfile);
            }
            doc.Save(path);
        }

        //GET

        public string SpecifyDependency(string files)
        {
            string result = "Successful";
            string path = System.Web.HttpContext.Current.Server.MapPath("~\\App_Data\\FileDependency.xml");
            XDocument doc;
            try
            {
                doc = XDocument.Load(path);
            }
            catch (System.IO.FileNotFoundException)
            {
                return "Error";
            }
            string SpeFile = files.Substring(0, files.IndexOf("::"));
            string SpeDepen = files.Substring(files.IndexOf("::") + 2);
            string[] SpeFiles = GetFiles(SpeFile, '|');
            foreach (string f in SpeFiles)
            {
                SpecifyDepen(f, ref SpeDepen, doc, ref path);
            }
            
            return result;
        }
        //GET
        public string CreateDir(string foldername,string path="DefaultPath")
        {
            string result = "Error";
            if (path == "DefaultPath")
            {
                path = Server.MapPath("~/Uploads/");
                
            }
            try
            {
                path = Server.MapPath(path);
                Directory.CreateDirectory(path+"\\" +foldername);
            }
            catch
            {
                return result;
            }
            result = "Successful";
            return result;
        }

	}
}