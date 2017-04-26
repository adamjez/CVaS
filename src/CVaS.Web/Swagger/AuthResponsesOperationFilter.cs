using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CVaS.DAL.Model;
using CVaS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CVaS.Web.Swagger
{
    public class AuthResponsesOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var authAttributes = ControllerAttributes(context.ApiDescription, typeof(AuthorizeAttribute))
                .Union(context.ApiDescription.ActionAttributes().OfType<AuthorizeAttribute>());

            var allowAnonymouseAttribute = context.ApiDescription.ActionAttributes().OfType<AllowAnonymousAttribute>();

            if (authAttributes.Any() && !allowAnonymouseAttribute.Any())
            {
                operation.Responses.Add("401",
                    new Response
                    {
                        Description = "Unauthorized"
                    });

                SetAuthorization(operation);
            }
        }

        private static void SetAuthorization(Operation operation)
        {
            if (operation.Security == null)
                operation.Security = new List<IDictionary<string, IEnumerable<string>>>();

            var apiKeyRequirement = new Dictionary<string, IEnumerable<string>>
                {
                    { "ApiKey", new List<string>() { Roles.Developer } },
                    { "oauth2", new List<string>() { Roles.Developer }  }
                };

            operation.Security.Add(apiKeyRequirement);

            //  operation.Parameters.Add(new NonBodyParameter {
            //         Name = "Authorization",
            //         In = "header",
            //         Description = "Api key",
            //         Required = true,
            //         Type = "string"                    
            //     });
        }

        public static IEnumerable<object> ControllerAttributes(ApiDescription apiDescription, System.Type attributeType)
        {
            var controllerActionDescriptor = ControllerActionDescriptor(apiDescription);
            return (controllerActionDescriptor == null)
                ? Enumerable.Empty<object>()
                : controllerActionDescriptor.ControllerTypeInfo.GetCustomAttributes(attributeType, true);
        }

        private static ControllerActionDescriptor ControllerActionDescriptor(ApiDescription apiDescription)
        {
            var controllerActionDescriptor = apiDescription.GetProperty<ControllerActionDescriptor>();
            if (controllerActionDescriptor == null)
            {
                controllerActionDescriptor = apiDescription.ActionDescriptor as ControllerActionDescriptor;
                apiDescription.SetProperty(controllerActionDescriptor);
            }
            return controllerActionDescriptor; 
        }
    }
}
