using DrugStoreCore.Dto;
using DrugStoreCore.ViewModel;
using DrugStoreData.Models;
using System.Threading.Tasks;

namespace DrugStoreInfrastructure.Services.Authenticate
{
    public interface IAuthenticationService
    {
        Task<ResponseViewModel<PharmacyInfoViewModel>> Login(PharmacyLoginDto dto);
        PharmacyInfo PharmaceInfo(string userName);
    }
}
