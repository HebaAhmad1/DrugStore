using DrugStoreCore.Constant;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace DrugStoreAPI.Controllers
{ 
    [ApiController]
    [Route("api/[controller]/[action]")]
    [Authorize]//(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)
    public class BaseController : Controller
    {
        protected string PharmacyId;
        public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (User.Identity.IsAuthenticated)
            {
                PharmacyId = User.FindFirst(PharmacyTokenInfo.PharmacyId).Value;
            }
            return base.OnActionExecutionAsync(context, next);
        }
    }
}
