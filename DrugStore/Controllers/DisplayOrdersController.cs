using DrugStore;
using DrugStore.Controllers;
using DrugStore.Data;
using DrugStoreCore.Dto;
using DrugStoreCore.Enums;
using DrugStoreCore.ViewModel;
using DrugStoreInfrastructure.PaginationHelpers;
using DrugStoreInfrastructure.Services.Authenticate;
using DrugStoreInfrastructure.Services.DisplayOrders;
using DrugStoreInfrastructure.Services.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DrugStoreAPI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DisplayOrdersController : BaseController
    {
        private readonly IDisplayOrdersService _displayOrdersService;
        private readonly IOrderService _orderService;


        public DisplayOrdersController(IDisplayOrdersService displayOrdersService,
                       IAuthenticationService authenticationService,
                        IOrderService orderService) : base(authenticationService)

        {
            _displayOrdersService = displayOrdersService;
            _orderService = orderService;
        }

        // <summary>
        //  to return all pharmacys 
        // </summary>
        // <param></param>
        //<returns>all pharmacys </returns>


        [HttpGet]
        public async Task<ActionResult> GetAllPharmacy(string search)
        {

            var data = await _orderService.AllPharmacys();
            var dataResult = data.Data.Select(data => new { id = data.Id, text = data.PharmacyName })
                .Where(d => string.IsNullOrEmpty(search) ? true : d.text.Contains(search));
            return Json(new { results = dataResult });
        }

        [HttpGet]
        public async Task<ActionResult> EditOrderByAdmin(int id)
        {

            var data = await _displayOrdersService.UpdateOrderByAdmin(id);

            var dataResult = data.Data.Select(d => new
            {
                drugId = d.Drug.DrugId,
                isHanging = d.IsHanging,
                drugIdParam = d.Drug.Id,
                orderId = d.Id,
                drugName = d.Drug.DrugName,
                quantity = d.Quantity,
                pricePerUnit = d.UnitPrice,
                totalPrice = d.Quantity * d.UnitPrice,
                is_hanging = d.IsHanging
            }).ToList();


            return Ok(dataResult);
        }


        [HttpPost]
        public async Task<ActionResult> UpdateOrderByAdmin(List<UpdateOrderDto> orderDtos)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            var data = await _displayOrdersService.UpdateOrderByAdmin(orderDtos);

            if (data.Status)
            {

                return Ok(data);
            }
            else
            {
                return BadRequest(data);
            }
        }


        // <summary>
        //  Process Pending Orders To Admin And Export Excel 
        // </summary>
        // <param name="int[]">pharmacyordersId</param>
        //<returns>ResponseViewModel<byte[]></returns>
        //<returns>File</returns>
        [HttpPost]
        public async Task<ActionResult> ProcessPendingOrdersAndExportExcel(int[] pharmacyordersId, Query query)
        {

            var result = await _displayOrdersService.ProcessPendingOrdersAndExportExcel(pharmacyordersId, query);
            var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm");


            if (result.Status)
            {
                return File(
               result.Data,
               "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
               $"{"report "}{time}.xlsx"
               );
            }
            else
            {
                return Ok(result.Message);
            }


        }


        [HttpPost]
        public async Task<ActionResult> ExportArchivedOrders(Query query)
        {
            var result = await _displayOrdersService.ExportArchivedOrders(query);
            var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

            if (result.Status)
            {
                return File(
              result.Data,
              "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
              $"{"report "}{time}.xlsx"
              );
            }
            else
            {
                return Ok(result.Message);
            }
        }

        // <summary>
        //  Import Drugs Information As Excel By Admin
        // </summary>
        // <param name="UploadFileDto">excelFile</param>
        //<returns>ResponseViewModel<int?></returns>
        [HttpPost]
        public async Task<ActionResult> ImportDrugsInfoAsExcel
            ([FromForm] UploadFileDto excelFile)
        {
            return Ok(await _displayOrdersService.ImportDrugsInfoAsExcel(excelFile.File));
        }

        // <summary>
        //  Import Pharmacy Information As Excel By Admin
        // </summary>
        // <param name="UploadFileDto">excelFile</param>
        //<returns>ResponseViewModel<int?></returns>
        [HttpPost]
        public async Task<ActionResult> ImportPharmacysInfoAsExcel
            ([FromForm] UploadFileDto excelFile)
        {
            return Ok(await _displayOrdersService.ImportPharmacysInfoAsExcel(excelFile.File));
        }

        // <summary>
        //  Import Locations Information As Excel By Admin
        // </summary>
        // <param name="UploadFileDto">excelFile</param>
        //<returns>ResponseViewModel<int?></returns>
        [HttpPost]
        public async Task<ActionResult> ImportLocationsInfoAsExcel
            ([FromForm] UploadFileDto excelFile)
        {
            return Ok(await _displayOrdersService.ImportLocationsInfoAsExcel(excelFile.File));
        }

        // <summary>
        //  Display All Customers 
        // </summary>
        // <param name="PagingWithOutQueryDto">pagination</param>
        //<returns>ResponseViewModel<ResponseDto></returns>
        [HttpPost]
        public async Task<ActionResult> AllCustomers(DataTableDto dto)
        {
            var result = await _displayOrdersService.AllCustomers(dto);
            var jsonData = new { recordsFiltered = result.Data.meta.total, result.Data.meta.total, result.Data.data };

            return Ok(jsonData);
        }

        [HttpGet]
        public async Task<ActionResult> AdminNotifications()
        {
            return Ok(await _displayOrdersService.AdminNotifications());
        }

        //************************* Code Mohammed *************************
        // <summary>
        //  Display Admin Current Orders
        // </summary>
        // <param name="PagingDto">pagination</param>
        //<returns>ResponseViewModel<ResponseDto></returns>
        [HttpGet]
        public async Task<ActionResult> AdminCurrentOrders(int pharmacyOrdersId = 0)
        {
            ViewBag.pharmacyOrdersId = 0;
            if (pharmacyOrdersId != 0)
            {
                ViewBag.pharmacyOrdersId = pharmacyOrdersId;
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AdminCurrentOrders(DataTableDto dto, int pharmacyOrdersId = 0)
        {
            return Ok(await _displayOrdersService.GetAdminCurrentOrders(dto, pharmacyOrdersId));
        }

        // <summary>
        //  Display Admin Archived Orders
        // </summary>
        // <param name="PagingDto">pagination</param>
        //<returns>ResponseViewModel<ResponseDto></returns>
        [HttpGet]
        public async Task<ActionResult> AdminArchivedOrders()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> AdminArchivedOrders(DataTableDto dto)
        {

            return Ok(await _displayOrdersService.GetAdminArchivedOrders(dto));
        }

        // <summary>
        //  Change Notification Status To Become IsReaded
        // </summary>
        //<returns>ResponseViewModel<bool></returns>
        [HttpGet]
        public async Task<ActionResult> ChangeAdminNotificationStatus()
        {
            return Ok(await _displayOrdersService.ChangeAdminNotificationStatus());
        }

    }
}
