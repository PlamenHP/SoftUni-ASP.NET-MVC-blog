﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Blog.Models
{
    public class EditUserViewModel
    {
        public BlogUser User { get; set; }

        public string Password { get; set; }

        [DisplayName("Confirm Password")]
        [Compare("Password", ErrorMessage="Password does  not mathch.")]
        public string ConfirmPassword { get; set; }

        public IList<Role> Roles { get; set; }
    }
}