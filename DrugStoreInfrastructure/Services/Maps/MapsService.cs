using AutoMapper;
using DrugStore.Data;
using DrugStoreCore.Dto;
using DrugStoreCore.Enums;
using DrugStoreCore.ViewModel;
using DrugStoreData.Models;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugStoreInfrastructure.Services.Maps
{
    public class MapsService : IMapsService
    {
        private readonly DrugStoreDbContext _db;
        public MapsService(DrugStoreDbContext db)
        {
            _db = db;
        }
        // Get Pharmacys Coordinates
        public async Task<ResponseViewModel<List<CoordinatesViewModel>>> Coordinates()
        {
            var coordinates = await _db.Users.Select(phar => new CoordinatesViewModel
            {
                Latitude = phar.Latitude,
                Longitude = phar.Longitude
            }).ToListAsync();
            if (coordinates.Count == 0)
            {
                return new ResponseViewModel<List<CoordinatesViewModel>>
                    (false, "Not Found Any Pharmacys", null);
            }
            return new ResponseViewModel<List<CoordinatesViewModel>>
                    (true, "All Coordinates", coordinates);
        }

        // Get Pharmacys Information With Coordinates
        public async Task<ResponseViewModel<List<PharmacyInfoWithCoordinatesViewModel>>> PharmacyInfoWithCoordinates()
        {
            var statusData = _db.PharmacysOrders.Include(x => x.Orders).Select(x => new { Id = x.PharmacyId, Status = x.Orders.OrderBy(y => x.CreatedAt).FirstOrDefault().Status, date = x.CreatedAt });
            var pharmacyInfo = await _db.Users.Include(supplier => supplier.Supplier)
                                              .Include(pharNote => pharNote.PharmacyNotes)
                .Select(phar => new PharmacyInfoWithCoordinatesViewModel
                {
                    PharmacyId = phar.Id,
                    PharmacyName = phar.PharmacyName,
                    SupplierName = phar.Supplier.Name,
                    Latitude = phar.Latitude,
                    Longitude = phar.Longitude,
                    Status = (int)statusData.OrderByDescending(x => x.date).Where(x => x.Id == phar.Id).Select(x => x.Status).FirstOrDefault(),
                    PharmacyNotes = phar.PharmacyNotes.Select(note =>
                            new PharmacyNotesViewModel { NoteTitle = note.Title, NoteBody = note.Body }).ToList()
                }).ToListAsync();

            if (pharmacyInfo.Count == 0)
            {
                return new ResponseViewModel<List<PharmacyInfoWithCoordinatesViewModel>>
                    (false, "Not Found Any Pharmacys", null);
            }
            return new ResponseViewModel<List<PharmacyInfoWithCoordinatesViewModel>>
                    (true, "All Coordinates", pharmacyInfo);
        }

        // Add Note To Pharmacy
        public async Task<ResponseViewModel<PharmacyNotesViewModel>>
                            AddPharmacyNote(PharmacyNotesViewModel note)
        {
            if (note is null || string.IsNullOrEmpty(note.NoteTitle) || string.IsNullOrEmpty(note.NoteBody) || string.IsNullOrEmpty(note.PharmacyId))
            {
                return new ResponseViewModel<PharmacyNotesViewModel>
                 (false, "Error, Note Title And Body Is Required!", null);
            }
            var pharmacy = await _db.Users.FindAsync(note.PharmacyId);
            if (pharmacy is null)
            {
                return new ResponseViewModel<PharmacyNotesViewModel>
                (false, "Error, Pharmacy Not Found!", null);
            }
            await _db.PharmacyNotes.AddAsync(new PharmacyNotes
            { Title = note.NoteTitle, Body = note.NoteBody, PharmacyId = note.PharmacyId });
            await _db.SaveChangesAsync();
            return new ResponseViewModel<PharmacyNotesViewModel>
              (true, "Added Note Succeeded", note);
        }




        // Change Pharmacy Coordinates
        public async Task<ResponseViewModel<ChangePharmacyCoordinatesDto>>
                            ChangePharmacyCoordinates(ChangePharmacyCoordinatesDto coordinates, string PharmacyId)
        {
            if (coordinates.Latitude is null || coordinates.Longitude is null)
            {
                return new ResponseViewModel<ChangePharmacyCoordinatesDto>
                 (false, "Error, Latitude And Longitude Is Required!", null);
            }
            var pharmacy = await _db.Users.FindAsync(PharmacyId);
            if (pharmacy is null || pharmacy.PharmacyStatus == PharmacyStatus.NonActive)
            {
                return new ResponseViewModel<ChangePharmacyCoordinatesDto>
                (false, "Error, Pharmacy Not Found!", null);
            }
            pharmacy.Latitude = coordinates.Latitude;
            pharmacy.Longitude = coordinates.Longitude;
            _db.Users.Update(pharmacy);
            await _db.SaveChangesAsync();
            return new ResponseViewModel<ChangePharmacyCoordinatesDto>
              (true, "Change Pharmacy Coordinates Succeeded", coordinates);
        }

        public async Task<ResponseViewModel<ChangePharmacyCoordinatesDto>> PharmacyCoordinates(string PharmacyId)
        {
            var result = new ResponseViewModel<ChangePharmacyCoordinatesDto>();
            if (_db.Users.Where(x => x.Id == PharmacyId).FirstOrDefault() is Pharmacy user)
            {
                result.Status = true;
                result.Message = "Pharmacy Location";
                result.Data = new ChangePharmacyCoordinatesDto
                {
                    Latitude = user.Latitude,
                    Longitude = user.Longitude
                };
            }
            else
            {
                result.Status = false;
                result.Message = "Something Went Error!";
            }
            return result;
        }
        //Get Special Companies
        public async Task<List<SpecialLocations>> GetSpecialCompanies()
        {
            var locations = await _db.SpecialLocations.ToListAsync();
            return locations;
        }
        //Import Special Locations
        public async Task ImportSpecialLocations(IFormFile file)
        {
            var stream = new MemoryStream();
            file.CopyTo(stream);
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var model = new List<SpecialLocations>();
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                do
                {
                    while (reader.Read())
                    {
                        var id = reader.GetValue(0)?.ToString().Trim();

                        if (id == null || reader.Depth == 0)
                        {
                            continue;
                        }
                        else
                        {
                            model.Add(new SpecialLocations
                            {
                                Id = reader.GetValue(0)?.ToString().Trim(),
                                Username = reader.GetValue(1)?.ToString().Trim(),
                                LocationName = reader.GetValue(2)?.ToString().Trim(),
                                Latitude = reader.GetValue(3)?.ToString().Trim(),
                                Longitude = reader.GetValue(4)?.ToString().Trim(),
                                CID = reader.GetValue(6)?.ToString().Trim(),
                            });
                        }
                    }
                }
                while (reader.NextResult());
            }

            var companies = _db.SpecialCompanies.ToList();
            var company = new SpecialCompany();
            var listLocations = model.Select(x => x.CID).Distinct().ToList();
            foreach (var comp in listLocations)
            {
                company = companies.FirstOrDefault(u => u.CID == comp);
                if (company == null)
                {
                    var locations = model.Where(x => x.CID == comp).ToList();
                    var newCompany = new SpecialCompany()
                    {
                        CID = comp,
                        Name = model.FirstOrDefault(x => x.CID == comp).Username,
                        SpecialLocations = locations
                    };
                    await _db.SpecialCompanies.AddAsync(newCompany);
                }
            }
            await _db.SaveChangesAsync();
        }
    }

}
