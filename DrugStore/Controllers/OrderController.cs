using DrugStore.Controllers;
using DrugStore.Data;
using DrugStoreCore.Dto;
using DrugStoreCore.Enums;
using DrugStoreCore.ViewModel;
using DrugStoreInfrastructure.Services.Authenticate;
using DrugStoreInfrastructure.Services.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Drug_Store.Controllers
{
    public class OrderController : BaseController
    {
        private readonly IOrderService _orderService;
        public OrderController(IAuthenticationService authenticationService,
                                IOrderService orderService): base(authenticationService)
        {
            _orderService = orderService;
        }

        [Authorize(Policy = "PharmacyRole")]
        [HttpGet]
        public async Task<ActionResult> Create(int? PharmacyOrdersId, int? update)  //params for reorder and update order
        {

            if (update == 1)
                ViewBag.update = "update";

            if (PharmacyOrdersId is null)
            {
                return View();
            }
            else
            {

                var PharOrdsId = string.IsNullOrEmpty(PharmacyOrdersId.ToString()) ? "0" : PharmacyOrdersId.ToString();
                var result = update == 1 ? await _orderService.UpdateOrderByPharmacy(int.Parse(PharOrdsId), PharmacyId) : await _orderService.PharmacyReorder(int.Parse(PharOrdsId), PharmacyId);

                ViewBag.pharmacyOrdersId = PharOrdsId;
                return View(result);
            }
        }

        // POST: OrderController/Create
        [Authorize(Policy = "PharmacyRole")]
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(List<CreateOrderDto> OrderDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _orderService.CreateOrder(OrderDto, PharmacyId);
            return Ok(result);
        }


        // POST: OrderController/UpdateOrder
        [Authorize(Policy = "PharmacyRole")]
        [HttpPost]
        public async Task<ActionResult> UpdateOrder(List<UpdateOrderDto> OrderDto)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            return Ok(await _orderService.UpdateOrderByPharmacy(OrderDto, PharmacyId));
        }




        public async Task<IActionResult> PharmacyAccountInfo()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> PharmacyAccountInfEdit()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PharmacyAccountInfEdit(string PharmacyName)
        {
            return Ok(await _orderService.EditAccountInformation(PharmacyName, PharmacyId));
        }

        [HttpPost]
        public async Task<ActionResult> EditPassword(EditPharmacyPasswordDto editPharmacyPassword)
        {
            return Ok(await _orderService.EditPassword(editPharmacyPassword, PharmacyId));
        }





        [Authorize(Policy = "PharmacyRole")]
        //Code Mohammed
        [HttpGet]
        public async Task<IActionResult> PharmacyCurrentOrders()
        {
            return View();
        }

        [Authorize(Policy = "PharmacyRole")]
        [HttpPost]
        public async Task<IActionResult> PharmacyCurrentOrders(DataTableDto dto)
        {

            return Ok(await _orderService.GetPharmacyCurrentOrders(dto, PharmacyId));
        }

        [Authorize(Policy = "PharmacyRole")]
        [HttpGet]
        public async Task<IActionResult> PharmacyArchivedOrders(int pharmacyOrdersId = 0)
        {
            ViewBag.pharmacyOrdersId = 0;
            if (pharmacyOrdersId != 0)
            {
                ViewBag.pharmacyOrdersId = pharmacyOrdersId;
            }
            return View();
        }

        [Authorize(Policy = "PharmacyRole")]
        [HttpPost]
        public async Task<IActionResult> PharmacyArchivedOrders(DataTableDto dto, int pharmacyOrdersId = 0)
        {

            return Ok(await _orderService.GetPharmacyArchivedOrders(dto, PharmacyId, pharmacyOrdersId));
        }

        [Authorize(Policy = "PharmacyRole")]
        [HttpDelete]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            var data = await _orderService.DeleteOrder(orderId, PharmacyId);

            if (data.Status)
            {
                return Ok(data);
            }
            else
            {
                return BadRequest(data);
            }
        }

        [Authorize(Policy = "PharmacyRole")]
        [HttpGet]
        public async Task<ActionResult> PharmacyNotification()
        {
            return Ok(await _orderService.PharmacyNotifications(PharmacyId));
        }

        [Authorize(Policy = "PharmacyRole")]
        [HttpGet]
        public async Task<ActionResult> ChangePharmacyNotificationStatus()
        {
            return Ok(await _orderService.ChangePharmacyNotificationStatus(PharmacyId));
        }

    }
}
