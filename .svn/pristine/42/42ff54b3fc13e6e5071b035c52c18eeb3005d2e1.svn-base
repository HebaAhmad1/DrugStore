using DrugStoreInfrastructure.Services.Drugs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DrugStoreAPI.Controllers
{
    public class DrugController : BaseController
    {
        private readonly IDrugService _drugService;

        public DrugController(IDrugService drugService)
        {
            _drugService = drugService;
        }
        // <summary>
        //  Display All Drugs Names
        // </summary>
        //<returns>ResponseViewModel<List<GetAllDrugs>></returns>
        [HttpGet]
        public async Task<ActionResult> AllDrug()
        {
            return Ok(await _drugService.AllDrug());
        }
        // <summary>
        //  Display Drug Details
        // </summary>
        // <param name="int">DrugId</param>
        //<returns>ResponseViewModel<DrugDetailsViewModel></returns>
        [HttpGet]
        public async Task<ActionResult> DrugDetails(int DrugId)
        {
            return Ok(await _drugService.DrugDetails(DrugId));
        }
    }
}
