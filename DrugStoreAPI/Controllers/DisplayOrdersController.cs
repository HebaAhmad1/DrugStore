using DrugStoreCore.Dto;
using DrugStoreInfrastructure.PaginationHelpers;
using DrugStoreInfrastructure.Services.DisplayOrders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        public DisplayOrdersController(IDisplayOrdersService displayOrdersService)
        {
            _displayOrdersService = displayOrdersService;
        }

        // <summary>
        //  Display Admin Current Orders
        // </summary>
        // <param name="PagingDto">pagination</param>
        //<returns>ResponseViewModel<ResponseDto></returns>
        [HttpPost]
        public async Task<ActionResult> AdminCurrentOrders
            ([FromBody] PagingWithQueryDto pagination)
        {
            return Ok(await _displayOrdersService.AdminCurrentOrders(pagination));
        }

        // <summary>
        //  Display Admin Archived Orders
        // </summary>
        // <param name="PagingDto">pagination</param>
        //<returns>ResponseViewModel<ResponseDto></returns>
        [HttpPost]
        public async Task<ActionResult> AdminArchivedOrders
            ([FromBody] PagingWithQueryDto pagination)
        {
            return Ok(await _displayOrdersService.AdminArchivedOrders(pagination));
        }

        // <summary>
        //  Process Pending Orders To Admin And Export Excel 
        // </summary>
        // <param name="int[]">pharmacyordersId</param>
        //<returns>ResponseViewModel<byte[]></returns>
        //<returns>File</returns>
        [HttpPut]
        public async Task<ActionResult> ProcessPendingOrdersAndExportExcel([FromBody] int[] UnProcessedOrders, Query query)
        {
            var content = await _displayOrdersService.ProcessPendingOrdersAndExportExcel(UnProcessedOrders, query);
            var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            if(content.Data is null)
            {
                return Ok(content);
            }
            else
            {
                return File(
               content.Data,
               "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
               $"{"report "}{time}.xlsx"
               );
            }
        }

        // <summary>
        //  Admin Reports And Export Excel 
        // </summary>
        // <param name="Query">query</param>
        //<returns>ResponseViewModel<byte[]></returns>
        //<returns>File</returns>
        [HttpPost]
        public async Task<ActionResult> ExportArchivedOrders([FromBody] Query query)
        {
            var content = await _displayOrdersService.ExportArchivedOrders(query);
            var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            if (content.Data is null)
            {
                return Ok(content);
            }
            else
            {
                return File(
               content.Data,
               "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
               $"{"report "}{time}.xlsx"
               );
            }
        }

        // <summary>
        //  Get Orders To Admin To Update
        // </summary>
        // <param name="int">OrderId</param>
        //<returns>ResponseViewModel<List<OrderViewModel>></returns>
        [HttpGet]
        public async Task<ActionResult> UpdateOrderByAdmin(int OrderId)
        {
            return Ok(await _displayOrdersService.UpdateOrderByAdmin(OrderId));
        }

        // <summary>
        //  Post Orders By Admin To Update
        // </summary>
        // <param name="PagingDto">pagination</param>
        //<returns>ResponseViewModel<ValidAndInvalidOrdersViewModel></returns>
        [HttpPut]
        public async Task<ActionResult> UpdateOrderByAdmin([FromBody] List<UpdateOrderDto> OrdersDto)
        {
            return Ok(await _displayOrdersService.UpdateOrderByAdmin(OrdersDto));
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
        public async Task<ActionResult> AllCustomers
            ([FromBody] DataTableDto pagination)
        {
            return Ok(await _displayOrdersService.AllCustomers(pagination));
        }
    }
}
