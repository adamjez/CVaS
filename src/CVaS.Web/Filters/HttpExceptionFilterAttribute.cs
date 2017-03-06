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
            if (context.Exception is ApiException)
            {
                var ex = (ApiException) context.Exception;
                var apiError = new ApiError(ex.Message);

                context.ExceptionHandled = true;

                context.HttpContext.Response.StatusCode = ex.StatusCode;
                context.Result = new ObjectResult(apiError);
            }
            else if (context.Exception is NotImplementedException)
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                context.ExceptionHandled = true;
            }
            else if (context.Exception is UnauthorizedAccessException)
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.ExceptionHandled = true;
            }

            base.OnException(context);
        }
    }
}
