using System;
using System.Threading.Tasks;
using CVaS.BL.Common;
using CVaS.BL.Core.Provider;
using CVaS.BL.Providers;
using CVaS.BL.Services.ApiKey;
using CVaS.BL.Services.Email;
using CVaS.DAL;
using CVaS.DAL.Model;
using CVaS.Web.Models.AccountViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CVaS.Web.Controllers.Web
{
    [Authorize(ActiveAuthenticationSchemes = "WebCookieScheme")]
    public class AccountController : WebController
    {
        private readonly ILogger<AccountController> _logger;
        private readonly AppSignInManager _signInManager;
        private readonly AppUserManager _userManager;
        private readonly IApiKeyGenerator _apiKeyGenerator;
        private readonly IEmailSender _emailSender;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public AccountController(ILogger<AccountController> logger, AppSignInManager signInManager, AppUserManager userManager,
            IApiKeyGenerator apiKeyGenerator, IEmailSender emailSender, ICurrentUserProvider currentUserProvider, IUnitOfWorkProvider unitOfWorkProvider)
            : base(currentUserProvider)
        {
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _apiKeyGenerator = apiKeyGenerator;
            _emailSender = emailSender;
            _currentUserProvider = currentUserProvider;
            _unitOfWorkProvider = unitOfWorkProvider;
        }

        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            var viewModel = new LoginViewModel()
            {
                ReturnUrl = returnUrl,
                Title = "Log In"
            };

            InitializeLayoutModel(viewModel);
            return View(viewModel);
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                using (_unitOfWorkProvider.Create())
                {
                    var user = await _userManager.FindByNameAsync(model.UserName);

                    if (user != null)
                    {
                        if (user.EmailConfirmed)
                        {
                            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, lockoutOnFailure: false);
                            if (result.Succeeded)
                            {
                                _logger.LogInformation(1, "User logged in.");
                                return RedirectToLocal(returnUrl);
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "You have to confirm email first.");
                        }
                    }
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            var viewModel = new RegisterViewModel()
            {
                Title = "Register"
            };

            InitializeLayoutModel(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                using (_unitOfWorkProvider.Create())
                {
                    var user = new AppUser
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        ApiKey = _apiKeyGenerator.Generate()
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                        // Send an email with this link
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var callbackUrl = Url.Action(nameof(ConfirmEmail), "Account",
                            new {userId = user.Id, code = code},
                            protocol: HttpContext.Request.Scheme);

                        await _emailSender.SendEmailAsync(model.Email, "Confirm your account",
                            $"Please confirm your account by clicking this link: <a href='{callbackUrl}'>link</a>");

                        _logger.LogInformation(3, "User created a new account with password.");

                        return Redirect(callbackUrl);
                    }
                    AddErrors(result);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return ErrorView();
            }

            using (_unitOfWorkProvider.Create())
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ErrorView();
                }
                var result = await _userManager.ConfirmEmailAsync(user, code);

                if (result.Succeeded)
                {
                    return View(InitializeLayoutModel("Confirm Email"));
                }
                else
                {
                    return ErrorView();
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            using (_unitOfWorkProvider.Create())
            {
                var viewModel = new SettingsViewModel()
                {
                    Title = "Settings",
                    ApiKey = (await _userManager.GetUserAsync(User)).ApiKey
                };

                InitializeLayoutModel(viewModel);

                return View(viewModel);
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> RevokeApiKey(SettingsViewModel viewModel)
        {
            using (var uow = _unitOfWorkProvider.Create())
            {
                var user = await _userManager.GetUserAsync(User);

                user.ApiKey = _apiKeyGenerator.Generate();
                await uow.CommitAsync();

                return View(nameof(Settings), viewModel);
            }
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            var viewModel = new ForgotPasswordViewModel()
            {
                Title = "Forgot your password?"
            };

            InitializeLayoutModel(viewModel);

            return View(viewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (_unitOfWorkProvider.Create())
                {
                    var user = await _userManager.FindByNameAsync(model.Email);
                    if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                    {
                        // Don't reveal that the user does not exist or is not confirmed
                        return RedirectToAction(nameof(ForgotPasswordConfirmation));
                    }

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                    // Send an email with this link
                    var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var callbackUrl = Url.Action(nameof(ResetPassword), "Account", new {userId = user.Id, code = code},
                        protocol: HttpContext.Request.Scheme);
                    await _emailSender.SendEmailAsync(model.Email, "Reset Password",
                        $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");

                    return RedirectToAction(nameof(Login));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View(InitializeLayoutModel("Forgot Password Confirmation"));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            if (code == null)
            {
                return ErrorView();
            }

            var layout = new ResetPasswordViewModel()
            {
                Title = "Reset Password"
            };

            InitializeLayoutModel(layout);

            return View(layout);
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

            using (_unitOfWorkProvider.Create())
            {
                var user = await _userManager.FindByNameAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist
                    return RedirectToAction(nameof(ResetPasswordConfirmation));
                }
                var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(ResetPasswordConfirmation));
                }
                AddErrors(result);

                return View(model);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View(InitializeLayoutModel("Reset Password Confirmation"));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(SettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(nameof(Settings), model);
            }

            using (_unitOfWorkProvider.Create())
            {
                var user = await _userManager.GetUserAsync(_currentUserProvider.GetClaims());
                if (user != null)
                {
                    var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, false);
                        _logger.LogInformation(3, "User changed their password successfully.");
                        return RedirectToAction(nameof(Settings), new {Message = ManageMessageId.ChangePasswordSuccess});
                    }
                    AddErrors(result);
                    return View(nameof(Settings), model);
                }
                return RedirectToAction(nameof(Settings), new {Message = ManageMessageId.Error});
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation(4, "User logged out.");

            return RedirectToAction(nameof(HomeController.Index), "Home");
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
                return RedirectToAction(nameof(Settings));
            }
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            AddLoginSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

        #endregion
    }
}
