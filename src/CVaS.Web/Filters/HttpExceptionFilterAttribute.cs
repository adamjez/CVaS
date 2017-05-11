using System;
using System.Net;
using CVaS.Shared.Exceptions;
using CVaS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CVaS.Web.Filters
{
    /// <summary>
    /// Exception filter that catch exception of known type
    /// and transform them into specific HTTP status code with 
    /// error message
    /// </summary>
    public class HttpExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            switch (context.Exception)
            {
                case ApiException apiException:
                    var apiError = new ApiError(apiException.Message);
                    context.ExceptionHandled = true;
                    context.HttpContext.Response.StatusCode = apiException.StatusCode;
                    context.Result = new ObjectResult(apiError);
                    break;

                case NotImplementedException _:
                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                    context.ExceptionHandled = true;
                    break;

                case UnauthorizedAccessException _:
                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    context.ExceptionHandled = true;
                    break;
            }


            base.OnException(context);
        }
    }
}
