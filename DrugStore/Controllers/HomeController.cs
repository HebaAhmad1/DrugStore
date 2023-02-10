using DrugStoreInfrastructure.Services.Authenticate;
using DrugStoreInfrastructure.Services.Drugs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DrugStore.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IDrugService _drugService;
        public HomeController(IAuthenticationService authenticationService, IDrugService drugService)
                                : base(authenticationService)
        {
            _drugService = drugService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> LandingPage()
        {
            return View();
        }

        //Method To Change Language In Cookies And Website
        [AllowAnonymous]
        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );
            return LocalRedirect(returnUrl);
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> sendMessage(string email, string message)
        {
            await _drugService.SaveMessage(email,message);
            return Json("done");
        }

    }
}
