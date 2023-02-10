using DrugStoreCore.Dto;
using DrugStoreInfrastructure.PaginationHelpers;
using DrugStoreInfrastructure.Services.Orders;
using MAFAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DrugStoreAPI.Controllers
{
    public class OrderController : BaseController
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // <summary>
        //  Display All Pharmacya
        // </summary>
        //<returns>ResponseViewModel<List<GetAllPharmacys>>></returns>
        [HttpGet("getAllPharmacy")]
        public async Task<ActionResult> AllPharmacys()
        {
            return Ok(await _orderService.AllPharmacys());
        }

        // <summary>
        //  Display Pharmacy Current Orders
        // </summary>
        // <param name="PagingDto">pagination</param>
        //<returns>ResponseViewModel<ResponseDto></returns>
        [HttpPost]
        public async Task<ActionResult> PharmacyCurrentOrders
                             ([FromBody] PagingWithOutQueryDto pagination)
        {
            return Ok(await _orderService.PharmacyCurrentOrders(pagination, PharmacyId));
        }
        // <summary>
        //  Display Pharmacy Archived Orders
        // </summary>
        // <param name="PagingDto">pagination</param>
        //<returns>ResponseViewModel<ResponseDto></returns>
        [HttpPost]
        public async Task<ActionResult> PharmacyArchivedOrders
                             ([FromBody] PagingWithOutQueryDto pagination)
        {
            return Ok(await _orderService.PharmacyArchivedOrders(pagination, PharmacyId));
        }

        // <summary>
        //  Create Orders
        // </summary>
        // <param name="List<CreateOrderDto>">OrderDto</param>
        //<returns>ResponseViewModel<List<OrderViewModel>></returns>
        [HttpPost]
        public async Task<ActionResult> CreateOrder([FromBody] List<CreateOrderDto> OrderDto)
        {
            return Ok(await _orderService.CreateOrder(OrderDto, PharmacyId));
        }
        // <summary>
        //  Display Order To Updated
        // </summary>
        // <param name="int">OrderId</param>
        //<returns>ResponseViewModel<OrderViewModel></returns>
        [HttpGet]
        public async Task<ActionResult> UpdateOrder(int OrderId)
        {
            return Ok(await _orderService.UpdateOrderByPharmacy(OrderId, PharmacyId));
        }
        // <summary>
        //  Order Update
        // </summary>
        // <param name="UpdateOrderDto">OrderDto</param>
        //<returns>ResponseViewModel<OrderViewModel></returns>
        [HttpPut]
        public async Task<ActionResult> UpdateOrder([FromBody] List<UpdateOrderDto> OrderDto)
        {
            return Ok(await _orderService.UpdateOrderByPharmacy(OrderDto, PharmacyId));
        }
        // <summary>
        //  Order Delete
        // </summary>
        // <param name="int">OrderId</param>
        //<returns>ResponseViewModel<OrderViewModel></returns>
        [HttpDelete]
        public async Task<ActionResult> DeleteOrder(int OrderId)
        {
            return Ok(await _orderService.DeleteOrder(OrderId, PharmacyId));
        }

        // <summary>
        //  Display Pharmacy Account Information
        // </summary>
        //<returns>ResponseViewModel<PharmacyAccountInformationViewModel></returns>
        [HttpGet]
        public async Task<ActionResult> AccountInformation()
        {
            return Ok(await _orderService.AccountInformation(PharmacyId));
        }

        // <summary>
        //  Edit Pharmacy Name
        // </summary>
        // <param name="string">PharmacyName</param>
        //<returns>ResponseViewModel<EditPharmacyInfo></returns>
        [HttpGet]
        public async Task<ActionResult> EditAccountInformation(string PharmacyName)
        {
            return Ok(await _orderService.EditAccountInformation(PharmacyName, PharmacyId));
        }

        // <summary>
        //  Edit Pharmacy Password
        // </summary>
        // <param name="EditPharmacyPasswordDto">editPharmacyPassword</param>
        //<returns>ResponseViewModel<EditPharmacyPasswordDto></returns>
        [HttpPost]
        public async Task<ActionResult> EditPassword([FromBody] EditPharmacyPasswordDto editPharmacyPassword)
        {
            return Ok(await _orderService.EditPassword(editPharmacyPassword, PharmacyId));
        }

        // <summary>
        //  Edit Pharmacy Location
        // </summary>
        // <param name="int">OrderId</param>
        //<returns>ResponseViewModel<EditPharmacyLocation></returns>
        [HttpPost]
        public async Task<ActionResult> EditPharmacyLocation([FromBody] EditPharmacyLocation editPharmacyLocation)
        {
            return Ok(await _orderService.EditPharmacyLocation(editPharmacyLocation, PharmacyId));
        }
    }
}
