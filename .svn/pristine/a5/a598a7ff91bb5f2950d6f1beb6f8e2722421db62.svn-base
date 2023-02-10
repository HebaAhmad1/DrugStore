using DrugStoreInfrastructure.Services.Authenticate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DrugStore.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        protected string PharmacyId;
        public BaseController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (User.Identity.IsAuthenticated)
            {
                var pharmacy = _authenticationService.PharmaceInfo(User.Identity.Name);
                PharmacyId = pharmacy.PharmacyId;
                ViewBag.PharAccountNumber = pharmacy.AccountNumber;
                ViewBag.Address = pharmacy.Address;
                ViewBag.PahrmacyName = pharmacy.PharmacyName;
                ViewBag.PharmacyRole = pharmacy.PharmacyRole;
            }
            return base.OnActionExecutionAsync(context, next);
        }
    }
}
