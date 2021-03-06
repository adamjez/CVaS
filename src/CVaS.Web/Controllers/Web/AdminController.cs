﻿using System.Threading.Tasks;
using CVaS.BL.DTO;
using CVaS.BL.Facades;
using CVaS.DAL.Model;
using CVaS.Web.Authentication;
using CVaS.Web.Models;
using CVaS.Web.Models.AccountViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CVaS.BL.Providers;

namespace CVaS.Web.Controllers.Web
{
    [Authorize(Roles = Roles.Admin, AuthenticationSchemes = AuthenticationSchemes.WebCookie)]
    public class AdminController : WebController
    {
        private readonly AlgorithmFacade _algoFacade;
        private readonly RuleFacade _ruleFacade;
        private readonly StatsFacade _statsFacade;

        public AdminController(ICurrentUserProvider currentUserProvider, AlgorithmFacade algoFacade, RuleFacade ruleFacade, StatsFacade statsFacade)
            : base(currentUserProvider)
        {
            _algoFacade = algoFacade;
            _ruleFacade = ruleFacade;
            _statsFacade = statsFacade;
        }

        public async Task<IActionResult> Index()
        {
            var layout = new AdminSectionViewModel
            {
                Title = "Admin Section",
                Algorithms = await _algoFacade.GetAllWithStats(),
                Rules = await _ruleFacade.GetAll(),
                NewRule = new RuleViewModel(),
                Stats = await _statsFacade.CreateStats()
            };

            InitializeLayoutModel(layout);

            return View(layout);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRule(AdminSectionViewModel adminViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            await _ruleFacade.Insert(new RuleDTO() { Regex = adminViewModel.NewRule.Regex});

            return RedirectToActionPermanent(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRule(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            await _ruleFacade.Remove(id);

            return RedirectToActionPermanent(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnableRule(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            await _ruleFacade.SetEnabled(id, true);

            return RedirectToActionPermanent(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisableRule(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            await _ruleFacade.SetEnabled(id, false);

            return RedirectToActionPermanent(nameof(Index));
        }
    }
}
