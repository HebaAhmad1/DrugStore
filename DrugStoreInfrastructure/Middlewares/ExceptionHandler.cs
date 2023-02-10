using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MAFInfrastructure.Middlewares
{
    public class ExceptionHandler
    {
        //private readonly RequestDelegate _next;
        //private readonly IServiceProvider _serviceProvider;

        //public ExceptionHandler(RequestDelegate next,
        //                        IServiceProvider serviceProvider)
        //{
        //    _next = next;
        //    _serviceProvider = serviceProvider;
        //}

        //public async Task Invoke(HttpContext context)
        //{
        //    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        //    var exception = exceptionHandlerPathFeature.Error;
        //    var response = new APIResponseViewModel();
        //    //context.Response.StatusCode = (int)HttpStatusCode.OK;
        //    switch (exception)
        //    {
        //        case EntityNotFoundException:
        //        case FaildCreateUserException:
        //        case FieldsIsRequiredException:
        //        case InvalidUsernameOrPasswordException:
        //        case ShortPasswordException:
        //        case FoundErrorInFileException:
        //            response.Message = exception.Message;
        //            response.Status = false;
        //            response.Data = null;
        //            break;
        //        default:
        //            response.Message = exception.Message;
        //            response.Status = false;
        //            response.Data = null;
        //            var requestBody = string.Empty;
        //            var req = context.Request;
        //            req.EnableBuffering();
        //            if (req.Body.CanSeek)
        //            {
        //                req.Body.Seek(0, SeekOrigin.Begin);
        //                using (var reader = new StreamReader(
        //                    req.Body,
        //                    Encoding.UTF8,
        //                    false,
        //                    8192,
        //                    true))
        //                {
        //                    requestBody = await reader.ReadToEndAsync();
        //                }
        //                req.Body.Seek(0, SeekOrigin.Begin);
        //            }
        //            break;
        //    }

        //    context.Response.ContentType = "application/json";
        //    await context.Response.WriteAsync(
        //        JsonConvert.SerializeObject(response)
        //    );
        //}

    }
}
