using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoleBase.Models
{
    public class Files
    {
      
        public IEnumerable<HttpPostedFileBase> files { get; set; }
    }
}