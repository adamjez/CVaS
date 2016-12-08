using System;
using System.Threading.Tasks;
using CVaS.BL.Services.ApiKey;
using CVaS.DAL;
using CVaS.DAL.Model;
using CVaS.Web.Models.AccountViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace CVaS.Web.Controllers
{
    [Authorize(ActiveAuthenticationSchemes = "WebCookieScheme")]
    [Route("[controller]")]
    public class AccountController : WebController
    {
        private readonly ILogger<AccountController> _logger;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;
        private readonly IApiKeyGenerator _apiKeyGenerator;

        public AccountController(ILogger<AccountController> logger, SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, AppDbContext context,
            IApiKeyGenerator apiKeyGenerator)
        {
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _apiKeyGenerator = apiKeyGenerator;
        }

        // GET: /Account/Login
        [HttpGet("login")]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost("login")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation(1, "User logged in.");
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet("api-key")]
        public IActionResult ManageApiKey()
        {
            return View();
        }

        [HttpPost("api-key/revoke")]
        public async Task<IActionResult> RevokeApiKey()
        {
            var user = await _userManager.GetUserAsync(User);
            do
            {

                user.ApiKey = _apiKeyGenerator.Generate();
                await _context.SaveChangesAsync();
            } while (await _context.Users.Where(u => u.ApiKey == user.ApiKey).CountAsync() > 1);

            return View("ManageApiKey");
        }


        //
        // POST: /Account/Register
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        //{
        //    ViewData["ReturnUrl"] = returnUrl;
        //    if (ModelState.IsValid)
        //    {
        //        var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
        //        var result = await _userManager.CreateAsync(user, model.Password);
        //        if (result.Succeeded)
        //        {
        //            // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
        //            // Send an email with this link
        //            //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        //            //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
        //            //await _emailSender.SendEmailAsync(model.Email, "Confirm your account",
        //            //    $"Please confirm your account by clicking this link: <a href='{callbackUrl}'>link</a>");
        //            await _signInManager.SignInAsync(user, isPersistent: false);
        //            _logger.LogInformation(3, "User created a new account with password.");
        //            return RedirectToLocal(returnUrl);
        //        }
        //        AddErrors(result);
        //    }

        //    // If we got this far, something failed, redisplay form
        //    return View(model);
        //}

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation(4, "User logged out.");

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(ManageApiKey));
            }
        }
    }
}
