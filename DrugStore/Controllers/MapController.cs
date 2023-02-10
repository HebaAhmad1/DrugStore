using DrugStoreCore.Dto;
using DrugStoreCore.ViewModel;
using DrugStoreInfrastructure.Services.Authenticate;
using DrugStoreInfrastructure.Services.Maps;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DrugStore.Controllers
{
    public class MapController : BaseController
    {
        public readonly IMapsService _mapsService;
        public MapController(IAuthenticationService authenticationService, IMapsService mapsService) : base(authenticationService)
        {
            _mapsService = mapsService;
        }
        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<IActionResult> HomeMap()
        //{
        //    return Json(await _mapsService.Coordinates());
        //}
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminMap()
        {
            return Json(await _mapsService.PharmacyInfoWithCoordinates());
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddMapNotes(PharmacyNotesViewModel note)
        {
            return Json(await _mapsService.AddPharmacyNote(note));
        }
        [HttpPost]
        [Authorize(Policy = "PharmacyRole")]
        public async Task<IActionResult> ChangePharmacyCoordinates(ChangePharmacyCoordinatesDto coordinates)
        {
            return Json(await _mapsService.ChangePharmacyCoordinates(coordinates, PharmacyId));
        }
        [HttpGet]
        [Authorize(Policy = "PharmacyRole")]
        public async Task<IActionResult> PharmacyCoordinates()
        {
            return Json(await _mapsService.PharmacyCoordinates(PharmacyId));
        }
        //********************************
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SpecialCompanies()
        {
            var data = await _mapsService.GetSpecialCompanies();
            return Json(data);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ImportSpecialLocations(IFormFile file)
        {
            await _mapsService.ImportSpecialLocations(file);
            return Json("Index");
        }
    }
}
