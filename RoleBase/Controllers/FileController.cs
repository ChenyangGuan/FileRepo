using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using System.Xml.Linq;

namespace FileRepository.Controllers
{
   
        // GET api/<controller>
         public class FileController : ApiController
    {
        //----< GET api/File - get list of available files >---------------

        public string Get()
        {
            // available files
            string path = System.Web.HttpContext.Current.Server.MapPath("~/Uploads/");
            
            return path;

        }

        //public IEnumerable<string> Get(string fullpath,bool ford)
        //{
        //    // available Dir
            
        //       string path = System.Web.HttpContext.Current.Server.MapPath(fullpath);
        //       if (!ford)
        //       {
        //           string[] directories = Directory.GetDirectories(path);
        //           for (int i = 0; i < directories.Length; ++i)
        //               directories[i] = directories[i].Substring(directories[i].LastIndexOf("\\") + 1);
        //           return directories;
        //       }
        //       else
        //       {
        //           string[] files = Directory.GetFiles(path);
        //           for (int i = 0; i < files.Length; ++i)
        //               files[i] = Path.GetFileName(files[i]);
        //           return files;
        //       }
            
            
        //}

        //----< attempt to Get File Dependencies >-------------------------
        public IEnumerable<string> Get(string Fullpath)
        {
            string path = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/FileDependency.xml");
            List<string> fileset=new List<string>();
            XDocument doc;
            try
            {
                doc = XDocument.Load(path);
            }
            catch (System.IO.FileNotFoundException)
            {
                return fileset;
            }
            string fp = Fullpath;
            string filename = Fullpath.Substring(Fullpath.LastIndexOf("\\") + 1);
            var tmp = doc.Element("FileDependency");
            IEnumerable<XElement> fn =
    from el in tmp.Elements() select el;
            foreach (var f in fn)
            {
                string fullpath = f.Element("FullPath").Value.Remove(0, 2);               
                fullpath=System.Web.HttpContext.Current.Server.MapPath("~/" +fullpath);
                if ( fullpath== fp)
                {

                    if (f.Element("Dependencies").Value == "")
                    {
                        continue;
                    }
                    var denpendentFiles = f.Element("Dependencies").Elements("FileFullPath");
                    string folderpath = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/");
                    string FileRelativePath =fp.Substring(fp.IndexOf("Uploads") + 8, fp.LastIndexOf("\\") - fp.IndexOf("Uploads") - 8);
                    folderpath += FileRelativePath +"\\"+Path.GetFileName(fp)+ "_dependencies";
                    Directory.CreateDirectory(folderpath);
                    DirectoryInfo depenency = new DirectoryInfo(folderpath);
                    foreach (var defile in denpendentFiles)
                    {
                        string oldpath = defile.Value.Remove(0, 2);
                        oldpath = "../../" + oldpath;
                        string dirpath = depenency.FullName;
                        string newpath = dirpath + "\\" + defile.Value.Substring(defile.Value.LastIndexOf("\\") + 1);
                        System.IO.File.Copy(System.Web.HttpContext.Current.Server.MapPath(oldpath), newpath, true);

                    }
                    fileset.Add(depenency.FullName+"\\");
                }
            }
            return fileset;

        }

        //----< GET api/File?fileName=foobar.txt&open=true >---------------
        //----< attempt to open or close FileStream >----------------------

        public HttpResponseMessage Get(string fileName, string open)
        {
            string sessionId;
            var response = new HttpResponseMessage();
            Models.Session session = new Models.Session();

            CookieHeaderValue cookie = Request.Headers.GetCookies("session-id").FirstOrDefault();
            if (cookie == null)
            {
                sessionId = session.incrSessionId();
                cookie = new CookieHeaderValue("session-id", sessionId);
                cookie.Expires = DateTimeOffset.Now.AddDays(1);
                cookie.Domain = Request.RequestUri.Host;
                cookie.Path = "/";
            }
            else
            {
                //sessionId = session.incrSessionId();
                sessionId = cookie["session-id"].Value;
            }
            try
            {
                FileStream fs;
                string path = System.Web.HttpContext.Current.Server.MapPath("~/");
                if (open == "download")  // attempt to open requested fileName
                {
                    string currentFileSpec = fileName;
                    fs = new FileStream(currentFileSpec, FileMode.Open);
                    session.saveStream(fs, sessionId);
                }
                else if (open == "upload")
                {

                    string dirname = fileName.Substring(0, fileName.LastIndexOf("\\"));                   
                    DirectoryInfo dir = new DirectoryInfo(dirname);
                    if(dir.Exists==false) Directory.CreateDirectory(dirname);
                    
                    fs = new FileStream(fileName, FileMode.OpenOrCreate);
                    session.saveStream(fs, sessionId);
                }
                else  // close FileStream
                {
                    fs = session.getStream(sessionId);
                    session.removeStream(sessionId);
                    fs.Close();
                }
                response.StatusCode = (HttpStatusCode)200;
            }
            catch
            {
                response.StatusCode = (HttpStatusCode)400;
            }
          finally  // return cookie to save current sessionId
            {
                response.Headers.AddCookies(new CookieHeaderValue[] { cookie });
            }
            return response;
        }

        //----< GET api/File?blockSize=2048 - get a block of bytes >-------

        public async Task<HttpResponseMessage> Get(int blockSize)
        {
            // get FileStream and read block

            Models.Session session = new Models.Session();
            CookieHeaderValue cookie = Request.Headers.GetCookies("session-id").FirstOrDefault();
            string sessionId = cookie["session-id"].Value;
            FileStream down = session.getStream(sessionId);
            byte[] Block = new byte[blockSize];
            int bytesRead = down.Read(Block, 0, blockSize);
            if (bytesRead < blockSize)  // compress block
            {
                byte[] returnBlock = new byte[bytesRead];
                for (int i = 0; i < bytesRead; ++i)
                    returnBlock[i] = Block[i];
                Block = returnBlock;
            }
            // make response message containing block and cookie

            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
            message.Headers.AddCookies(new CookieHeaderValue[] { cookie });
            message.Content = new ByteArrayContent(Block);
            return message;
        }

        // POST api/file
        public HttpResponseMessage Post(int blockSize)
        {
            Task<byte[]> task = Request.Content.ReadAsByteArrayAsync();
            byte[] Block = task.Result;
            Models.Session session = new Models.Session();
            CookieHeaderValue cookie = Request.Headers.GetCookies("session-id").FirstOrDefault();
            string sessionId = cookie["session-id"].Value;
            FileStream up = session.getStream(sessionId);
            up.Write(Block, 0, Block.Count());
            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
            message.Headers.AddCookies(new CookieHeaderValue[] { cookie });
            return message;
        }
    }
}