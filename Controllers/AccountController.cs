using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CPSSnew.Models;
using CPSSnew.Models.AccountViewModels;
using CPSSnew.Services;
using System.IO;
using System.DrawingCore.Imaging;
using System.DrawingCore;
using System.DrawingCore.Drawing2D;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace CPSSnew.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private IMemoryCache _memoryCache;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ILogger<AccountController> logger,
            IMemoryCache memoryCache)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        [TempData]
        public string ErrorMessage { get; set; }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult modifyPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> modifyPassword(modifyPasswordView model)
        {
            if (ModelState.IsValid)
            {
                if(model.oldPassword ==null || model.newPassword ==null|| model.UserName == null)
                {
                    ModelState.AddModelError("loginError", "输入信息不全");
                    return View(model);
                };
                if (model.newPassword.Length < 8)
                {
                    ModelState.AddModelError("loginError", "密码最短需要8位");
                    return View(model);
                };
                //string userEmail = model.Email;
                var user = _userManager.Users.FirstOrDefault(s => s.UserName == model.UserName);
                if (user == null)
                {
                    ModelState.AddModelError("loginError", "未找到用户");
                    return View(model);
                };
                /*if (user.Email != model.Email)
                {
                    ModelState.AddModelError("loginError", "用户名与邮箱不匹配");
                    return View(model);
                };*/
                var oldC = model.oldPassword;
                var newC = model.newPassword;
                var result = await _userManager.ChangePasswordAsync(user, oldC , newC);
                if (result.Succeeded)
                {
                    await _signInManager.SignOutAsync();
                    //return Json("修改密码成功");
                    return RedirectToAction("Login", "Account");
                }
                ModelState.AddModelError("loginError", "原始密码输入错误");
                return View(model);
            }
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string useCode, string returnUrl = null)
        {
            /*ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return RedirectToLocal(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, model.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToAction(nameof(Lockout));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "登录尝试无效.");
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);*/

            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true

                //VerifyUserInputCode(userToken, string userVerCode);

                string a = TempData["cacheKey"].ToString();
                var cacheKey = a;
                var vCode = "";
                if (!_memoryCache.TryGetValue(cacheKey, out vCode)) ModelState.AddModelError("loginError", "验证码错误");
                if (useCode == null)
                {
                    ModelState.AddModelError("loginError", "请输入验证码");
                    return View(model);
                }
                if (vCode.ToLower() != useCode.ToLower()) {
                    ModelState.AddModelError("loginError", "验证码错误");
                    return View(model);
                };
                _memoryCache.Remove(cacheKey);

                ApplicationUser signinUser =_userManager.Users.FirstOrDefault(s => s.UserName == model.UserName);
                //ApplicationUser signinUser = await _userManager.FindByEmailAsync(model.Email);
                if (signinUser != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(signinUser.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation(1, "User logged in.");
                        if (signinUser == null)
                        {
                            ModelState.AddModelError("loginError", "用户不存在");
                            return RedirectToAction("Login", "Account");
                        }
                        else
                        {
                            var lastlogintime = DateTime.Now.ToString("yyyy:MM:dd HH:mm:ss");
                            await _userManager.UpdateAsync(signinUser);
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("loginError", "用户名或密码错误");
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError("loginError", "用户不存在");
                    return View(model);
                }
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("loginError", "无效的登录");
            return View(model);

        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWith2fa(bool rememberMe, string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            var model = new LoginWith2faViewModel { RememberMe = rememberMe };
            ViewData["ReturnUrl"] = returnUrl;

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model, bool rememberMe, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, model.RememberMachine);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID {UserId} logged in with 2fa.", user.Id);
                return RedirectToLocal(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                _logger.LogWarning("Invalid authenticator code entered for user with ID {UserId}.", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
                return View();
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWithRecoveryCode(string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWithRecoveryCode(LoginWithRecoveryCodeViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            var recoveryCode = model.RecoveryCode.Replace(" ", string.Empty);

            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID {UserId} logged in with a recovery code.", user.Id);
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                _logger.LogWarning("Invalid recovery code entered for user with ID {UserId}", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
                return View();
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string useCode, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {

                string a = TempData["cacheKey"].ToString();
                var cacheKey = a;
                var vCode = "";
                if (!_memoryCache.TryGetValue(cacheKey, out vCode)) ModelState.AddModelError("loginError", "验证码错误");
                if (useCode == null)
                {
                    ModelState.AddModelError("loginError", "请输入验证码");
                    return View(model);
                }
                if (vCode.ToLower() != useCode.ToLower())
                {
                    ModelState.AddModelError("loginError", "验证码错误");
                    return View(model);
                };
                _memoryCache.Remove(cacheKey);

                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                    await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation("User created a new account with password.");
                    //return RedirectToLocal(returnUrl);
                    //Response.WriteAsync("<script>alert('提交成功');</script>");
                    return RedirectToAction("Login", "Account");
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
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToAction(nameof(Login));
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in with {Name} provider.", info.LoginProvider);
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                return View("ExternalLogin", new ExternalLoginViewModel { Email = email });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    throw new ApplicationException("Error loading external login information during confirmation.");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(nameof(ExternalLogin), model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userId}'.");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.ResetPasswordCallbackLink(user.Id, code, Request.Scheme);
                await _emailSender.SendEmailAsync(model.Email, "Reset Password",
                   $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            if (code == null)
            {
                throw new ApplicationException("A code must be supplied for password reset.");
            }
            var model = new ResetPasswordViewModel { Code = code };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(UsersController.UserManage));
            }
            AddErrors(result);
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        
        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        #endregion


        //生成图片验证
        [AllowAnonymous]
        public sealed class VerifyCodeHelper
        {
            #region 单例模式
            //创建私有化静态obj锁  
            private static readonly object _ObjLock = new object();
            //创建私有静态字段，接收类的实例化对象  
            private static VerifyCodeHelper _VerifyCodeHelper = null;
            //构造函数私有化  
            private VerifyCodeHelper() { }
            //创建单利对象资源并返回  
            public static VerifyCodeHelper GetSingleObj()
            {
                if (_VerifyCodeHelper == null)
                {
                    lock (_ObjLock)
                    {
                        if (_VerifyCodeHelper == null)
                            _VerifyCodeHelper = new VerifyCodeHelper();
                    }
                }
                return _VerifyCodeHelper;
            }
            #endregion

            #region 生产验证码
            public enum VerifyCodeType { NumberVerifyCode, AbcVerifyCode, MixVerifyCode };

            /// <summary>
            /// 1.数字验证码
            /// </summary>
            /// <param name="length"></param>
            /// <returns></returns>
            private string CreateNumberVerifyCode(int length)
            {
                int[] randMembers = new int[length];
                int[] validateNums = new int[length];
                string validateNumberStr = "";
                //生成起始序列值  
                int seekSeek = unchecked((int)DateTime.Now.Ticks);
                Random seekRand = new Random(seekSeek);
                int beginSeek = seekRand.Next(0, Int32.MaxValue - length * 10000);
                int[] seeks = new int[length];
                for (int i = 0; i < length; i++)
                {
                    beginSeek += 10000;
                    seeks[i] = beginSeek;
                }
                //生成随机数字  
                for (int i = 0; i < length; i++)
                {
                    Random rand = new Random(seeks[i]);
                    int pownum = 1 * (int)Math.Pow(10, length);
                    randMembers[i] = rand.Next(pownum, Int32.MaxValue);
                }
                //抽取随机数字  
                for (int i = 0; i < length; i++)
                {
                    string numStr = randMembers[i].ToString();
                    int numLength = numStr.Length;
                    Random rand = new Random();
                    int numPosition = rand.Next(0, numLength - 1);
                    validateNums[i] = Int32.Parse(numStr.Substring(numPosition, 1));
                }
                //生成验证码  
                for (int i = 0; i < length; i++)
                {
                    validateNumberStr += validateNums[i].ToString();
                }
                return validateNumberStr;
            }

            /// <summary>
            /// 2.字母验证码
            /// </summary>
            /// <param name="length">字符长度</param>
            /// <returns>验证码字符</returns>
            private string CreateAbcVerifyCode(int length)
            {
                char[] verification = new char[length];
                char[] dictionary = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'
            };
                Random random = new Random();
                for (int i = 0; i < length; i++)
                {
                    verification[i] = dictionary[random.Next(dictionary.Length - 1)];
                }
                return new string(verification);
            }

            /// <summary>
            /// 3.混合验证码
            /// </summary>
            /// <param name="length">字符长度</param>
            /// <returns>验证码字符</returns>
            private string CreateMixVerifyCode(int length)
            {
                char[] verification = new char[length];
                char[] dictionary = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
                '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'
            };
                Random random = new Random();
                for (int i = 0; i < length; i++)
                {
                    verification[i] = dictionary[random.Next(dictionary.Length - 1)];
                }
                return new string(verification);
            }

            /// <summary>
            /// 产生验证码（随机产生4-6位）
            /// </summary>
            /// <param name="type">验证码类型：数字，字符，符合</param>
            /// <returns></returns>
            public string CreateVerifyCode(VerifyCodeType type)
            {
                string verifyCode = string.Empty;
                Random random = new Random();
                //int length = random.Next(4, 6);
                int length = 4;
                switch (type)
                {
                    case VerifyCodeType.NumberVerifyCode:
                        verifyCode = GetSingleObj().CreateNumberVerifyCode(length);
                        break;
                    case VerifyCodeType.AbcVerifyCode:
                        verifyCode = GetSingleObj().CreateAbcVerifyCode(length);
                        break;
                    case VerifyCodeType.MixVerifyCode:
                        verifyCode = GetSingleObj().CreateMixVerifyCode(length);
                        break;
                }
                return verifyCode;
            }
            #endregion

            #region 验证码图片
            /// <summary>
            /// 验证码图片 => Bitmap
            /// </summary>
            /// <param name="verifyCode">验证码</param>
            /// <param name="width">宽</param>
            /// <param name="height">高</param>
            /// <returns>Bitmap</returns>
            public Bitmap CreateBitmapByImgVerifyCode(string verifyCode, int width, int height)
            {
                Font font = new Font("Arial", 14, (FontStyle.Bold | FontStyle.Italic));
                Brush brush;
                Bitmap bitmap = new Bitmap(width, height);
                Graphics g = Graphics.FromImage(bitmap);
                SizeF totalSizeF = g.MeasureString(verifyCode, font);
                SizeF curCharSizeF;
                PointF startPointF = new PointF(0, (height - totalSizeF.Height) / 2);
                Random random = new Random(); //随机数产生器
                g.Clear(Color.White); //清空图片背景色  
                for (int i = 0; i < verifyCode.Length; i++)
                {
                    brush = new LinearGradientBrush(new Point(0, 0), new Point(1, 1), Color.FromArgb(random.Next(255), random.Next(255), random.Next(255)), Color.FromArgb(random.Next(255), random.Next(255), random.Next(255)));
                    g.DrawString(verifyCode[i].ToString(), font, brush, startPointF);
                    curCharSizeF = g.MeasureString(verifyCode[i].ToString(), font);
                    startPointF.X += curCharSizeF.Width;
                }

                //画图片的干扰线  
                for (int i = 0; i < 10; i++)
                {
                    int x1 = random.Next(bitmap.Width);
                    int x2 = random.Next(bitmap.Width);
                    int y1 = random.Next(bitmap.Height);
                    int y2 = random.Next(bitmap.Height);
                    g.DrawLine(new Pen(Color.Silver), x1, y1, x2, y2);
                }

                //画图片的前景干扰点  
                for (int i = 0; i < 100; i++)
                {
                    int x = random.Next(bitmap.Width);
                    int y = random.Next(bitmap.Height);
                    bitmap.SetPixel(x, y, Color.FromArgb(random.Next()));
                }

                g.DrawRectangle(new Pen(Color.Silver), 0, 0, bitmap.Width - 1, bitmap.Height - 1); //画图片的边框线  
                g.Dispose();
                return bitmap;
            }

            /// <summary>
            /// 验证码图片 => byte[]
            /// </summary>
            /// <param name="verifyCode">验证码</param>
            /// <param name="width">宽</param>
            /// <param name="height">高</param>
            /// <returns>byte[]</returns>
            public byte[] CreateByteByImgVerifyCode(string verifyCode, int width, int height)
            {
                Font font = new Font("Arial", 14, (FontStyle.Bold | FontStyle.Italic));
                Brush brush;
                Bitmap bitmap = new Bitmap(width, height);
                Graphics g = Graphics.FromImage(bitmap);
                SizeF totalSizeF = g.MeasureString(verifyCode, font);
                SizeF curCharSizeF;
                PointF startPointF = new PointF(0, (height - totalSizeF.Height) / 2);
                Random random = new Random(); //随机数产生器
                g.Clear(Color.White); //清空图片背景色  
                for (int i = 0; i < verifyCode.Length; i++)
                {
                    brush = new LinearGradientBrush(new Point(0, 0), new Point(1, 1), Color.FromArgb(random.Next(255), random.Next(255), random.Next(255)), Color.FromArgb(random.Next(255), random.Next(255), random.Next(255)));
                    g.DrawString(verifyCode[i].ToString(), font, brush, startPointF);
                    curCharSizeF = g.MeasureString(verifyCode[i].ToString(), font);
                    startPointF.X += curCharSizeF.Width;
                }

                //画图片的干扰线  
                for (int i = 0; i < 10; i++)
                {
                    int x1 = random.Next(bitmap.Width);
                    int x2 = random.Next(bitmap.Width);
                    int y1 = random.Next(bitmap.Height);
                    int y2 = random.Next(bitmap.Height);
                    g.DrawLine(new Pen(Color.Silver), x1, y1, x2, y2);
                }

                //画图片的前景干扰点  
                for (int i = 0; i < 100; i++)
                {
                    int x = random.Next(bitmap.Width);
                    int y = random.Next(bitmap.Height);
                    bitmap.SetPixel(x, y, Color.FromArgb(random.Next()));
                }

                g.DrawRectangle(new Pen(Color.Silver), 0, 0, bitmap.Width - 1, bitmap.Height - 1); //画图片的边框线  
                g.Dispose();

                //保存图片数据  
                MemoryStream stream = new MemoryStream();
                bitmap.Save(stream, ImageFormat.Jpeg);
                //输出图片流  
                return stream.ToArray();

            }
            #endregion
        }

        [AllowAnonymous]
        public FileContentResult MixVerifyCode()
        {
            string code = VerifyCodeHelper.GetSingleObj().CreateVerifyCode(VerifyCodeHelper.VerifyCodeType.MixVerifyCode);
            var bitmap = VerifyCodeHelper.GetSingleObj().CreateBitmapByImgVerifyCode(code, 100, 40);
            MemoryStream stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Gif);

            code = code.ToLower();//验证码不分大小写
            Response.Body.Dispose();
            var token = Guid.NewGuid().ToString();
            ViewBag.token = token;
            var cacheKey = token;
            TempData["cacheKey"] = cacheKey;
            _memoryCache.Set<string>(cacheKey, code, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(20)));
            CookieOptions options = new CookieOptions();
            Response.Cookies.Append("validatecode", token);

            return File(stream.ToArray(), "image/gif");
        }
    }


}
