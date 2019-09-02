using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CPSSnew.Models.AccountViewModels
{
    public class RegisterViewModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0}不能为空")]
        [StringLength(20, ErrorMessage = "{0} 必须至少包含 {2} 个汉字,最多10个汉字。", MinimumLength = 2)]
        //[RegularExpression("^[a-zA-Z0-9_\u4e00-\u9fa5]{2,10}$", ErrorMessage = "用户名由字母、数字与汉字组成。")]
        [RegularExpression("^[\u4e00-\u9fa5]{2,10}$", ErrorMessage = "字数不在范围内或未使用汉字")]
        [Display(Name = "用户名")]
        public string UserName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "{0}不能为空")]
        [EmailAddress]
        [Display(Name = "邮箱")]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "{0}不能为空")]
        //[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [StringLength(100, ErrorMessage = "密码最短需要8位", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "{0}不能为空")]
        [DataType(DataType.Password)]
        [Display(Name = "确认密码")]
        [Compare("Password", ErrorMessage = "两次输入的密码不同")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "权限")]
        public IList<string> Roles { get; set; }
    }
}
