using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CPSSnew.Models;
using CPSSnew.Models.AccountViewModels;
using CPSSnew.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CPSSnew.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            RoleManager<ApplicationRole> roleManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _logger = logger;
        }


        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> UserManage()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            string role = Newtonsoft.Json.JsonConvert.SerializeObject(roles);
            ViewBag.roles = role;

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            
            var users = await _userManager.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).ToListAsync();
            string user = Newtonsoft.Json.JsonConvert.SerializeObject(users, settings);
            ViewBag.users = user;
            
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> UserCreate()
        {
            ViewBag.RolesList = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "NormalizedName");

            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserCreate(RegisterViewModel model, string returnUrl = null, params string[] selectedRoles)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    //_logger.LogInformation("User created a new account with password.");

                    //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                    //await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

                    //await _signInManager.SignInAsync(user, isPersistent: false);
                    //_logger.LogInformation("User created a new account with password.");

                    for (var i = 0; i < selectedRoles.Length; i++)
                    {
                        var roleCheck = await _roleManager.RoleExistsAsync(selectedRoles[i]);

                        if (!roleCheck)
                        {
                            IdentityResult roleResult;
                      
                            //create the roles and seed them to the database
                                roleResult = await _roleManager.CreateAsync(new ApplicationRole(selectedRoles[i]));
                        
                        }
                    }

                    if (selectedRoles != null)
                    {
                        var roleResult = await _userManager.AddToRolesAsync(user, selectedRoles);

                        if (roleResult.Succeeded)
                        {
                            //return RedirectToAction(nameof(Index));
                            //return RedirectToLocal(returnUrl);
                            return RedirectToAction(nameof(UsersController.UserManage));
                        }
                        foreach (var error in roleResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                    //return RedirectToLocal(returnUrl);
                }
                foreach (IdentityError error in result.Errors)
                {
                    if (error.Code == "DuplicateUserName")
                        error.Description = "用户名已存在,请重新输入";
                    if (error.Code == "DuplicateEmail")
                        error.Description = "邮箱已被注册,请重新输入";
                    if (error.Code == "PasswordTooShort")
                        error.Description = "密码最短需要8位";
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View();
        }

       


        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                //return RedirectToAction(nameof(HomeController.Index), "Home");
                return RedirectToAction(nameof(UsersController.UserCreate), "Users");
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }


        //用户删除
        [HttpPost]
        public async Task<IActionResult> UserDelete(string Email)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser deleteUser = await _userManager.FindByEmailAsync(Email);
                if (deleteUser == null)
                {
                    throw new ApplicationException();
                }

                var result = await _userManager.DeleteAsync(deleteUser);
                if (!result.Succeeded)
                {
                    AddErrors(result);
                    return View();
                }
                return RedirectToAction(nameof(UsersController.UserManage));
            }
            return View();
        }

        //用户密码修改
        /*[HttpPost]
        public async Task<IActionResult> UserPasswordEidt(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser PEUser = await _userManager.FindByEmailAsync(model.Email);
                if (PEUser == null)
                {
                    throw new ApplicationException();
                }
                var userRoles = await _userManager.GetRolesAsync(PEUser);
                var resultDelete = await _userManager.DeleteAsync(PEUser);

                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                var resultRole = await _userManager.AddToRolesAsync(user, userRoles.ToArray<string>());

                if (result.Succeeded && resultRole.Succeeded && resultDelete.Succeeded)
                {
                    return RedirectToAction(nameof(UsersController.UserManage));
                }

            }
            else {
                var msg = string.Empty;
                foreach (var value in ModelState.Values)
                {
                    if (value.Errors.Count > 0)
                    {
                        foreach (var error in value.Errors)
                        {
                            msg = msg + error.ErrorMessage;
                        }
                    }
                }
                return Json(msg);
            }
            return RedirectToAction(nameof(UsersController.UserManage));
        }*/

        //管理员修改密码
        public  async Task<IActionResult> ChangePasswordAsync(ApplicationUser user, string password)
        {
            ApplicationUser updateUser = await _userManager.FindByEmailAsync(user.Email);

            var type = _userManager.GetType();
            var storeProperty = type.GetProperty("Store", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var store = storeProperty.GetValue(_userManager) as IUserPasswordStore<ApplicationUser>;
            if (store == null)
            {
                //return IdentityResult.Failed(new IdentityError { Code = "NotImplements", Description = "持久化存储器没有实现IUserPasswordStore接口" });
                return RedirectToAction(nameof(UsersController.UserManage));
            }
            var passwordHash = _userManager.PasswordHasher.HashPassword(updateUser, password);
         
            await store.SetPasswordHashAsync(updateUser, passwordHash, System.Threading.CancellationToken.None);
            var result=await store.UpdateAsync(updateUser, System.Threading.CancellationToken.None);
            if (!result.Succeeded)
            {
                AddErrors(result);
                return RedirectToAction(nameof(UsersController.UserManage));
            };
            //return IdentityResult.Success;
            return RedirectToAction(nameof(UsersController.UserManage));
        }

        //用户修改密码
        [HttpPost]
        public async Task<ActionResult> ModifyPassword(string name,string oldPassword,string newPassword)
        {
            if (ModelState.IsValid)
            {
               
                string username = name;
                var user = _userManager.Users.FirstOrDefault(s => s.UserName == username);
                var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
                if (result.Succeeded)
                {
                    await _signInManager.SignOutAsync();
                    return Json("修改密码成功"); 
                }
                return Json("原始密码验证失败");
                //ModelState.AddModelError("", "原密码输入错误");
            }
            //return Json("Something failed.");
            return RedirectToAction("Login", "Account");
        }




        //用户编辑
        [HttpPost]
        public async Task<ActionResult> UserEdit(string Email, params string[] selectedRole)
        {
            if (ModelState.IsValid) 
            {
                for (var i = 0; i < selectedRole.Length; i++)
                {
                    var roleCheck = await _roleManager.RoleExistsAsync(selectedRole[i]);

                    if (!roleCheck)
                    {
                        IdentityResult roleResult;

                        //create the roles and seed them to the database
                        roleResult = await _roleManager.CreateAsync(new ApplicationRole(selectedRole[i]));

                    }
                }

                ApplicationUser EditUser = await _userManager.FindByEmailAsync(Email);
                if (EditUser == null)
                {
                    throw new ApplicationException();
                }
                
                var userRoles = await _userManager.GetRolesAsync(EditUser);

                selectedRole = selectedRole ?? new string[] { };//空合并运算符

                var result = await _userManager.AddToRolesAsync(EditUser, selectedRole.Except(userRoles).ToArray<string>());

                if (!result.Succeeded)
                {
                    AddErrors(result);
                    return View();
                }
                result = await _userManager.RemoveFromRolesAsync(EditUser, userRoles.Except(selectedRole).ToArray<string>());

                if (!result.Succeeded)
                {
                    AddErrors(result);
                    return View();
                }
                return RedirectToAction(nameof(UsersController.UserManage));
            }
            ModelState.AddModelError("", "Something failed.");
            return View();
        }

    }
}
