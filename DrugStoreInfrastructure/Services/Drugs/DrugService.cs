using AutoMapper;
using DrugStore.Data;
using DrugStoreCore.Dto;
using DrugStoreCore.Enums;
using DrugStoreCore.ViewModel;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugStoreInfrastructure.Services.Drugs
{
    public class DrugService : IDrugService
    {
        private readonly DrugStoreDbContext _db;
        private readonly IMapper _mapper;
        public DrugService(DrugStoreDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<ResponseViewModel<List<GetAllDrugs>>> AllDrug()
        {
            var drugs = await _db.Drugs.Where(drug => drug.DrugStatus == DrugStatus.Active)
                .Select(d => new GetAllDrugs { Id = d.Id, DrugName = d.DrugName }).ToListAsync();
            if (drugs is null)
            {
                return new ResponseViewModel<List<GetAllDrugs>>(false, "Not Found Data", null);
            }
            return new ResponseViewModel<List<GetAllDrugs>>(true, "Succeeded Process", drugs);
        }

        public async Task<ResponseViewModel<DrugDetailsViewModel>> DrugDetails(int DrugId)
        {
            var drug = await _db.Drugs.Where(drug => drug.Id == DrugId 
                                                     && drug.DrugStatus == DrugStatus.Active).SingleOrDefaultAsync();
            if (drug is null)
            {
                return new ResponseViewModel<DrugDetailsViewModel>(false, "Drug Not Found", null);
            }
            //If Drug Quantity 0 In Stock Will Return SellPrice 0 (Not Display SellPrice To User) 
            if (drug.Stock < 1)
            {
                drug.SellPrice = 0;
            }
            var DrugDetails = _mapper.Map<DrugDetailsViewModel>(drug);
            return new ResponseViewModel<DrugDetailsViewModel>(true, "Drug Is Found", DrugDetails);
        }

        public async Task<ResponseViewModel<string>> SaveMessage(string email, string message)
        {
            await _db.NewConacts.AddAsync(new DrugStoreData.Models.NewConact { 
                Email = email,
                Message = message
            });
            _db.SaveChanges();
            return new ResponseViewModel<string>(true,"done","done");
        }
    }
}
