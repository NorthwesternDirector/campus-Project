﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CPSSnew.Models.AccountViewModels
{
    public class LoginViewModel
    {
        /*[Required]
        [EmailAddress]
        [Display(Name = "邮箱")]
        public string Email { get; set; }*/

        [Required]
        [Display(Name = "用户名")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Display(Name = "记住我？")]
        public bool RememberMe { get; set; }
    }
}
