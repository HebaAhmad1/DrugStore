using DrugStoreCore.ViewModel;
using DrugStoreInfrastructure.Services.Authenticate;
using DrugStoreInfrastructure.Services.Drugs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DrugStore.Controllers
{
    public class DrugController : BaseController
    {
        private readonly IDrugService _drugService;

        public DrugController(IDrugService drugService, IAuthenticationService authenticationService)
            : base(authenticationService)
        {
            _drugService = drugService;
        }


        // GET: DrugController
        public async Task<IActionResult> AllDrugs(string search)
        {
            var data = await _drugService.AllDrug();
            var dataResult = data.Data.Select(x => new { id = x.Id, text = x.DrugName })
                .Where(d => string.IsNullOrEmpty(search?.ToUpper()) ? true : d.text.Contains(search?.ToUpper()));

            return Json(new {results = dataResult });
        }

        // GET: DrugController/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var data =  await _drugService.DrugDetails(id);
            return Json(new { data = data });
        }
    }
}
