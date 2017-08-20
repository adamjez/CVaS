using System.Threading.Tasks;
using CVaS.BL.Providers;
using CVaS.BL.Services.ApiKey;
using CVaS.DAL.Model;
using CVaS.Shared.Core.Provider;
using CVaS.Web.Models.ManageViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CVaS.Web.Controllers.Web
{
    public class ManageController : WebController
    {
        private readonly ILogger<AccountController> _logger;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly IApiKeyManager _apiKeyManager;

        public ManageController(ILogger<AccountController> logger, SignInManager<AppUser> signInManager, UserManager<AppUser> userManager,
            ICurrentUserProvider currentUserProvider, IUnitOfWorkProvider unitOfWorkProvider, IApiKeyManager apiKeyManager)
            : base(currentUserProvider)
        {
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _currentUserProvider = currentUserProvider;
            _unitOfWorkProvider = unitOfWorkProvider;
            _apiKeyManager = apiKeyManager;
        }

        [HttpGet]
        public async Task<ViewResult> Settings()
        {
            var message = (ManageMessageId?)(int?)TempData[nameof(ManageMessageId)];

            var viewModel = new SettingsViewModel()
            {
                Title = "Settings",
                ApiKey = await _apiKeyManager.GetApiKey(CurrentUserProvider.Id),
                StatusMessage = message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.RevokeApiKey ? "Your api key has been resetted."
                : ""
            };

            InitializeLayoutModel(viewModel);

            return View(nameof(Settings), viewModel);
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
                        return RedirectToSettingsPernament(ManageMessageId.ChangePasswordSuccess);
                    }
                    AddErrors(result);
                    return View(nameof(Settings), model);
                }

                return RedirectToSettingsPernament(ManageMessageId.Error);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<RedirectToActionResult> RevokeApiKey()
        {
            await _apiKeyManager.RevokeAsync(CurrentUserProvider.Id);

            return RedirectToSettingsPernament(ManageMessageId.RevokeApiKey);
        }

        #region Helpers

        private RedirectToActionResult RedirectToSettingsPernament(ManageMessageId? message = null)
        {
            TempData[nameof(ManageMessageId)] = message;
            return RedirectToActionPermanent(nameof(Settings));
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
