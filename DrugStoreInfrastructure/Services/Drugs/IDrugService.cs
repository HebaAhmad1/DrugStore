using DrugStoreCore.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugStoreInfrastructure.Services.Drugs
{
    public interface IDrugService
    {
        Task<ResponseViewModel<List<GetAllDrugs>>> AllDrug();
        Task<ResponseViewModel<DrugDetailsViewModel>> DrugDetails(int DrugId);
        Task<ResponseViewModel<string>> SaveMessage(string email, string message);
    }
}
