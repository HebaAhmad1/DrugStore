using DrugStoreCore.Dto;
using DrugStoreCore.ViewModel;
using DrugStoreData.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugStoreInfrastructure.Services.Maps
{
    public interface IMapsService
    {
        Task<ResponseViewModel<List<CoordinatesViewModel>>> Coordinates();
        Task<ResponseViewModel<List<PharmacyInfoWithCoordinatesViewModel>>> PharmacyInfoWithCoordinates();
        Task<ResponseViewModel<PharmacyNotesViewModel>>
                            AddPharmacyNote(PharmacyNotesViewModel note);
        Task<ResponseViewModel<ChangePharmacyCoordinatesDto>>
                            ChangePharmacyCoordinates(ChangePharmacyCoordinatesDto coordinates, string PharmacyId);
        Task<ResponseViewModel<ChangePharmacyCoordinatesDto>>
                            PharmacyCoordinates(string PharmacyId);
        Task<List<SpecialLocations>> GetSpecialCompanies();
        Task ImportSpecialLocations(IFormFile file);
    }
}
