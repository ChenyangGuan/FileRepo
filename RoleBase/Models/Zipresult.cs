
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RoleBase.Models
{
    public class ZipResult : ActionResult
    {
        private IEnumerable<string> _files;
        private IEnumerable<string> _folders;
        private string _fileName;

        public string FileName
        {
            get
            {
                return _fileName ?? "file.zip";
            }
            set { _fileName = value; }
        }
        public ZipResult(string[] folders)
        {
            this._folders = folders;
        }
        public ZipResult(IEnumerable<string> folders)
        {
            this._folders = folders;
        }
        public ZipResult(string[] folders, params string[] files)
        {
            this._files = files;
            this._folders = folders;
        }

        public ZipResult(IEnumerable<string> files, IEnumerable<string> folders = null)
        {
            this._files = files;
            this._folders = folders;
        }
        public void AddFolder(string foldername){
            this._folders.ToList().Add(foldername);
        }
        public void AddFiles(string filename)
        {
            this._files.ToList().Add(filename);
        }
        public override void ExecuteResult(ControllerContext context)
        {
            using (ZipFile zf = new ZipFile())
            {
                if (_folders != null)
                    foreach (string folder in _folders)
                    {
                        if (folder != null)
                        {
                            string foldername = folder.Substring(folder.LastIndexOf("\\") + 1, folder.Length - folder.LastIndexOf("\\") - 1);
                            zf.AddDirectory(folder, foldername);
                        }
                    }
                if (_files != null)
                    foreach (string file in _files)
                    {
                        if (file != null)
                            zf.AddFile(file, "");
                    }
                context.HttpContext
                    .Response.ContentType = "application/zip";
                context.HttpContext
                    .Response.AppendHeader("content-disposition", "attachment; filename=" + FileName);
                zf.Save(context.HttpContext.Response.OutputStream);
            }
        }

    }
}