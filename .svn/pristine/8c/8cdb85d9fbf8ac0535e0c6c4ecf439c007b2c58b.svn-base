using DrugStoreCore.ViewModel;
using DrugStoreInfrastructure.Services.Drugs;
using DrugStoreInfrastructure.Services.Maps;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DrugStoreAPI.Controllers
{
    public class MapsController : BaseController
    {
        private readonly IMapsService _mapsService;
        public MapsController(IMapsService mapsService)
        {
            _mapsService = mapsService;
        }
        // <summary>
        //  Display All Pharmacys Coordinates
        // </summary>
        //<returns>ResponseViewModel<List<CoordinatesViewModel>></returns>
        [HttpGet]
        public async Task<ActionResult> Coordinates()
        {
            return Ok(await _mapsService.Coordinates());
        }
        // <summary>
        //  Display All Pharmacys Coordinates With Some Information
        // </summary>
        //<returns>ResponseViewModel<List<PharmacyInfoWithCoordinatesViewModel>></returns>
        [HttpGet]
        public async Task<ActionResult> PharmacyInfoWithCoordinates()
        {
            return Ok(await _mapsService.PharmacyInfoWithCoordinates());
        }

        // <summary>
        //  Add Note To Pharmacy
        // </summary>
        // <param name="PharmacyNotesViewModel">note</param>
        //<returns>ResponseViewModel<PharmacyNotesViewModel></returns>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult> AddPharmacyNote([FromBody] PharmacyNotesViewModel note)
        {
            return Ok(await _mapsService.AddPharmacyNote(note));
        }
    }
}
