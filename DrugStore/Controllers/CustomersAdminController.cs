using DrugStoreInfrastructure.Services.Authenticate;
using DrugStoreInfrastructure.Services.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace DrugStore.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CustomersAdminController : BaseController
    {
        private readonly IOrderService _orderService;
        public CustomersAdminController(IAuthenticationService authenticationService, IOrderService orderService) : base(authenticationService)
        {
            _orderService = orderService;
        }
        public async Task<IActionResult> GetAllCustomers()
        {
            return View();
        }
        public async Task<IActionResult> ChartPage()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetAllPharmacys()
        {
            return Json(await _orderService.AllPharmacys());
        }
        [HttpGet]
        public async Task<IActionResult> GetStatusForPharmacy(string id)
        {
            var status = await _orderService.GetStatusForPharmacy(id);
;
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            return Ok(JsonConvert.SerializeObject(status));
        }
    }
}
