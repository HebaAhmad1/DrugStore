using DrugStoreAPI.Controllers;
using DrugStoreCore.Dto;
using DrugStoreInfrastructure.Services.Authenticate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAFAPI.Controllers
{
    public class AuthenticationController : BaseController
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        // <summary>
        //  Return Token To Login
        // </summary>
        // <param name="PharmacyLoginDto">dto</param>
        //<returns>ResponseViewModel<PharmacyInfoViewModel></returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] PharmacyLoginDto dto)
        {
            return Ok(await _authenticationService.Login(dto));
        }
    }
}
