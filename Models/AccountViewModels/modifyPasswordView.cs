using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CPSSnew.Models.AccountViewModels
{
    public class modifyPasswordView
    {
       
        [Display(Name = "用户名")]
        public string UserName { get; set; }

        [Display(Name = "邮箱")]
        public string Email { get; set; }

        [Display(Name = "原始密码")]
        public string oldPassword { get; set; }


        [Display(Name = "新密码")]
        public string newPassword { get; set; }
        
      
    }
}


