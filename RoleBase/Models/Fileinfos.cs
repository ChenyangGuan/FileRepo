using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoleBase.Models
{
    

    public class Files
    {
        public int id { get; set; }
        public string Size { get; set; }
        public string FileName { get; set; }
        public string FullPath { get; set; }
        public string LastAccessTime { get; set; }
        public List<Files> children { get; set; }
        public string state { get; set; }
        public string GroupBound { get; set; }
        public string GBFullPath { get; set; }
        public Files()
        {
            children = new List<Files>();
        }
        public Files(int i, string s, string n, string p, string actime,Files f = null, string st = "",string gb="",string gbfp="")
        {
            children = new List<Files>();           
            id = i;
            Size = s;
            FileName = n;
            FullPath = p;
            LastAccessTime = actime;
            if (f != null)
                children.Add(f);
            state = st;
            GroupBound = gb;
            GBFullPath = gbfp;
        }
        public void AddChild(Files f)
        {
            children.Add(f);
        }
        public bool hasChild()
        {
            if (children[0] == null) return false;
            else return true;
        }
    }

}