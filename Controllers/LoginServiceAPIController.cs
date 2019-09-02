using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using CPSSnew.Models;
using CPSSnew.Models.AccountViewModels;
using CPSSnew.Services;





namespace OUCMap_yushan_WebApi.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class LoginServiceAPIController : Controller
    {   
         private readonly UserManager<ApplicationUser> _userManager;
         private readonly SignInManager<ApplicationUser> _signInManager;
         public LoginServiceAPIController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager){
             _userManager = userManager;
            _signInManager = signInManager;
         }
        // GET api/values
        [HttpGet]
       public IActionResult Get()
        {
             return Json("login service");
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "userId - " + id;
        }

        //POST api/values
        [HttpPost] 
        [AllowAnonymous]
        public async Task<IActionResult> Post(string Email,string password)
        {    
                 var backMsgDict = new Dictionary<string,string>();
            if(Email != null && password != null){
            ApplicationUser signinUser = await _userManager.FindByNameAsync(Email);
            if(signinUser != null)
            {
            var result =  await _signInManager.PasswordSignInAsync(signinUser.UserName,password, false, lockoutOnFailure: false);
            
                if (result.Succeeded)
                {
                // _logger.LogInformation(1, "User logged in.");
                            string username = signinUser.UserName;
                            string emailSearch = signinUser.Email;
                            Dictionary<string,string> dic = new Dictionary<string, string>();
                            dic.Add("Email",emailSearch);
                            dic.Add("UserName",username);                        
                            var userRoles = await _userManager.GetRolesAsync(signinUser);
                            List<string> typeList = new List<string>();
                            if(userRoles.Count == 0){
                                dic.Add("Type","visitor");
                            }
                            else{
                            foreach(string name in userRoles){
                                if(name == "顶级权限"){
                                    dic.Add("Type","ProManager");
                                    break;
                                }
                                if(name == "维修权限"){
                                    typeList.Add("维修权限");
                                    continue;
                                }
                                if(name == "基建权限"){
                                    typeList.Add("基建权限");
                                    continue;
                                }
                            }
                            if(typeList.Count > 0){
                                if(typeList.Count == 2){
                                    dic.Add("Type","ReportProjectManager");
                                }
                                else{
                                    if(typeList[0] == "维修权限"){
                                        dic.Add("Type","ReportManager");
                                    }
                                    else if(typeList[0] == "基建权限"){
                                        dic.Add("Type","ProjectManager");
                                    }
                                }
                            }
                            }
                            //backMsgDict.Add("success","登录成功");
                           
                            
                            return Json(dic);
                }
            
             else{
                //backMsgDict.Add("failed","用户名或密码错误");
                 return Json("用户名或密码错误");
                 }
            }
            }
            return Json("未注册的用户");
            // var result = await _signInManager.PasswordSignInAsync(Email, password, false, lockoutOnFailure: false);
            
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
