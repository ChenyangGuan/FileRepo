﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace FileRepository.Models
{

    public class IndexUserViewModel
    {
       public IEnumerable <ApplicationUser> user;
       public IEnumerable<IList<string>> RoleList;
    }
    public class RoleViewModel
    {
        public string Id { get; set; }
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "RoleName")]
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }
        [Display(Name = "nickname")]
        public string nickname { get; set; }
        
        public IEnumerable<SelectListItem> RolesList { get; set; }
    }
}