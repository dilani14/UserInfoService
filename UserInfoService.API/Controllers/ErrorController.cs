using UserInfoService.Core.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IdentityDataService.API.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    public class ErrorController : ControllerBase
    {
        [Route("/error")]
        public IActionResult Error(
        [FromServices] IWebHostEnvironment webHostEnvironment)
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();

            int statusCode = GetStatusCodeForHttpContextResponse(context);

            return Problem(
                statusCode: statusCode,
                detail: context?.Error.Message);
        }

        private int GetStatusCodeForHttpContextResponse(IExceptionHandlerFeature? errorContext) 
        {
            if (errorContext?.Error is InValidRequestDataException)
            {
                var error = (InValidRequestDataException)errorContext.Error;
                return error.StatusCode;
            }

            return (int)HttpStatusCode.InternalServerError;
        }
    }
}
