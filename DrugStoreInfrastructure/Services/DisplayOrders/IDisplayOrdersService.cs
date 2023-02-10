using DrugStoreCore.Dto;
using DrugStoreCore.ViewModel;
using DrugStoreInfrastructure.PaginationHelpers;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugStoreInfrastructure.Services.DisplayOrders
{
    public interface IDisplayOrdersService
    {
        Task<ResponseViewModel<ResponseDto>> AdminCurrentOrders
                    (PagingWithQueryDto pagination);
        Task<ResponseViewModel<ResponseDto>> AdminArchivedOrders
                    (PagingWithQueryDto pagination);
        Task<ResponseViewModel<byte[]>> ProcessPendingOrdersAndExportExcel(int[] UnProcessedOrders, Query query);
        Task<ResponseViewModel<byte[]>> ExportArchivedOrders(Query query);
        Task<ResponseViewModel<List<OrderViewModel>>> UpdateOrderByAdmin(int OrderId);
        Task<ResponseViewModel<ValidAndInvalidOrdersViewModel>> UpdateOrderByAdmin(List<UpdateOrderDto> OrdersDto);
        Task<ResponseViewModel<int?>> ImportDrugsInfoAsExcel
            (IFormFile excelFile);
        Task<ResponseViewModel<int?>> ImportLocationsInfoAsExcel
            (IFormFile excelFile);
        Task<ResponseViewModel<int?>> ImportPharmacysInfoAsExcel
            (IFormFile excelFile);
        Task<ResponseViewModel<ResponseDto>> AllCustomers
            (DataTableDto pagination);
        Task<ResponseViewModel<List<AdminNotifications>>> AdminNotifications();
        Task<ResponseViewModel<bool>> ChangeAdminNotificationStatus();

        //Code Mohammed
        Task<DataTableOutput<List<GetOrdersDataTable>>> GetAdminCurrentOrders(DataTableDto dto, int pharmacyOrderId);
        Task<DataTableOutput<List<GetOrdersDataTable>>> GetAdminArchivedOrders(DataTableDto dto);
    }
}
