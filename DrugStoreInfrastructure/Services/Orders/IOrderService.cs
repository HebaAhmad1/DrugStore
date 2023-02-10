using DrugStoreCore.Dto;
using DrugStoreCore.Enums;
using DrugStoreCore.ViewModel;
using DrugStoreInfrastructure.PaginationHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugStoreInfrastructure.Services.Orders
{
    public interface IOrderService
    {
        Task<ResponseViewModel<ResponseDto>> PharmacyCurrentOrders
                             (PagingWithOutQueryDto pagination, string PharmacyId);
        Task<ResponseViewModel<ResponseDto>> PharmacyArchivedOrders
                                    (PagingWithOutQueryDto pagination, string PharmacyId);
        Task<ResponseViewModel<List<OrderViewModel>>> CreateOrder(List<CreateOrderDto> OrderDto, string PharmacyId);
        Task<ResponseViewModel<List<OrderViewModel>>> UpdateOrderByPharmacy(int OrderId, string PharmacyId);
        Task<ResponseViewModel<List<OrderViewModel>>> PharmacyReorder(int OrderId, string PharmacyId);

        Task<ResponseViewModel<ValidAndInvalidOrdersViewModel>>
            UpdateOrderByPharmacy(List<UpdateOrderDto> OrdersDto, string PharmacyId);
        Task<ResponseViewModel<OrderViewModel>> DeleteOrder(int OrderId, string PharmacyId);
        Task<ResponseViewModel<PharmacyAccountInformationViewModel>> AccountInformation(string PharmacyId);
        Task<ResponseViewModel<string>> EditAccountInformation
            (string PharmacyName, string PharmacyId);
        Task<ResponseViewModel<EditPharmacyPasswordDto>> EditPassword
            (EditPharmacyPasswordDto editPharmacyPassword, string PharmacyId);
        Task<ResponseViewModel<EditPharmacyLocation>> EditPharmacyLocation
            (EditPharmacyLocation editPharmacyLocation, string PharmacyId);
        Task<ResponseViewModel<List<GetAllPharmacys>>> AllPharmacys();
        Task<ResponseViewModel<List<PharmacyNotifications>>> PharmacyNotifications(string PharmacyId);
        Task<ResponseViewModel<bool>> ChangePharmacyNotificationStatus(string PharmacyId);

        //By Mohammed
        Task<DataTableOutput<List<GetOrdersDataTable>>> GetPharmacyCurrentOrders(DataTableDto dto,string PharmacyId);

        Task<DataTableOutput<List<GetOrdersDataTable>>> GetPharmacyArchivedOrders(DataTableDto dto, string PharmacyId, int pharmacyOrdersId);
        Task<List<Status>> GetStatusForPharmacy(string id);

    }
}