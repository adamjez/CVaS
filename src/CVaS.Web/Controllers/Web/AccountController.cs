using System.Threading.Tasks;
using CVaS.BL.Facades;
using CVaS.BL.Services.Email;
using CVaS.DAL.Model;
using CVaS.Shared.Core.Provider;
using CVaS.Web.Models.AccountViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CVaS.BL.Providers;
using CVaS.Web.Authentication;

namespace CVaS.Web.Controllers.Web
{
    public class AccountController : WebController
    {
        private readonly ILogger<AccountController> _logger;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly RuleFacade _ruleFacade;
        private readonly AccountFacade _accountFacade;

        public AccountController(ILogger<AccountController> logger, SignInManager<AppUser> signInManager, UserManager<AppUser> userManager,
            IEmailSender emailSender, ICurrentUserProvider currentUserProvider, IUnitOfWorkProvider unitOfWorkProvider,
            RuleFacade ruleFacade, AccountFacade accountFacade)
            : base(currentUserProvider)
        {
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _emailSender = emailSender;
            _unitOfWorkProvider = unitOfWorkProvider;
            _ruleFacade = ruleFacade;
            _accountFacade = accountFacade;
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
                var result = await _accountFacade.SignInAsync(model.UserName, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation(1, "User logged in.");
                    return RedirectToLocal(returnUrl);
                }
                else if (result.IsNotAllowed)
                {
                    ModelState.AddModelError(string.Empty, "You have to confirm email first.");
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

            var allowedEmail = await _ruleFacade.Validate(model.Email);
            if (ModelState.IsValid && allowedEmail)
            {
                var result = await _accountFacade.CreateUserAsync(model.Email, model.Password);

                if (result.Suceeded)
                {
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(result.User);

                    var callbackUrl = Url.Action<AccountController>(
                        c => c.ConfirmEmail(With.No<string>(), With.No<string>()),
                        new { userId = result.User.Id, code = code },
                        protocol: HttpContext.Request.Scheme);

                    await _emailSender.SendEmailAsync(result.User.Email, "Confirm your account",
                        $"Please confirm your account by clicking this link: <a href='{callbackUrl}'>link</a>");
                    return Redirect(callbackUrl);
                }
                else
                {
                    AddErrors(result.Identity);
                }
            }
            else if (!allowedEmail)
            {
                ModelState.AddModelError(nameof(model.Email), "Email Address is not in allowed email address list.");
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
        [AllowAnonymous]
        public ViewResult ForgotPassword()
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
                    if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
                    {
                        // Don't reveal that the user does not exist or is not confirmed
                        return RedirectToAction(nameof(ForgotPasswordConfirmation));
                    }

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                    // Send an email with this link
                    var code = await _userManager.GeneratePasswordResetTokenAsync(user);

                    var callbackUrl = Url.Action<AccountController>(c => c.ResetPassword(With.No<string>()),
                        new { userId = user.Id, code = code },
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
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation(4, "User logged out.");

            return this.RedirectToActionPermanent<HomeController>(c => c.Index());
        }

        #region Helpers

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return this.RedirectToAction<ManageController>(c => c.Settings());
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
            Error,
            RevokeApiKey
        }

        #endregion
    }
}
