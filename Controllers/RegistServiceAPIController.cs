using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.Collections;
using CPSSnew.Models;
using CPSSnew.Models.AccountViewModels;
using CPSSnew.Services;




namespace OUCMap_yushan_WebApi.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class RegistServiceAPIController : Controller
    {   
        private readonly UserManager<ApplicationUser> _userManager;
         private readonly SignInManager<ApplicationUser> _signInManager;
         public RegistServiceAPIController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager){
             _userManager = userManager;
            _signInManager = signInManager;
         }
        
        // GET api/values
         [HttpGet]
       public IActionResult Get()
        {      
            
             return Json("regist service");
        }
        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "userId - " + id;
        }

        //POST api/values
        [HttpPost] 
        //[ValidateAntiForgeryToken]
                public async Task<IActionResult> Post(string username,string email,string password)
        {   
           if(username != null && email != null && password != null){
                    var user = new ApplicationUser { 
                        UserName = username, 
                        Email = email
                        };
                        //return Json($"{username},{email},{password},{inviteCode}");
                        RegisterViewModel model = new RegisterViewModel();
                        model.Password = password;
                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        
                        //邮箱验证
                        //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        //var callbackUrl = Url.Action(nameof(ConfirmEmail), "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                        //await _emailSender.SendEmailAsync(model.Email, "Confirm your account",
                        //    $"Please confirm your account by clicking this link: <a href='{callbackUrl}'>link</a>");
                        await _signInManager.SignInAsync(user, isPersistent: false);
            
                        
                        return Json("注册成功");
                    }
                    foreach (IdentityError Error in result.Errors)
                {
                    if (Error.Code == "DuplicateEmail")
                    {
                        return Json("邮箱重复注册");
                    }
                    if (Error.Code == "DuplicateUserName")
                    {
                        return Json("用户名重复注册");
                    }
                    
                }
               
                    return Json("注册失败");
                
            
            }
            return Json("注册信息不足");
        }

        

        

        // public async Task<string> Post([FromForm]IOSLogin information)
        // {
            
        //     var passwordx = AccountController.DecryptoData(information.password);
        //     ApplicationUser signinUser = await _userManager.FindByEmailAsync(information.name);
        //     if(signinUser != null){
        //     var result =  await _signInManager.PasswordSignInAsync(information.name, passwordx, false, lockoutOnFailure: false);
        //     if (result.Succeeded){
        //         _logger.LogInformation(1, "User logged in.");
        //         var lastlogintime = DateTime.Now.ToString("yyyy:MM:dd HH:mm:ss");
        //                     signinUser.LoginTimes += 1;
        //                     signinUser.LastLoginTime = lastlogintime;
        //                     await _userManager.UpdateAsync(signinUser);
        //                     return "success";
        //     }
        //     else{
        //         return "failed";
        //     }
            
        //     }
        //     return "failed";
            
           

        // }

        // // PUT api/values/5
        // [HttpPut("{id}")]
        // public void Put(int id, [FromBody]string value)
        // {
        // }

        // // DELETE api/values/5
        // [HttpDelete("{id}")]
        // public void Delete(int id)
        // {
    }
}
