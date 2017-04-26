using System;
using System.Collections.Generic;
using System.Linq;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CVaS.Web.Swagger
{
    public class AddFileParamsFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var fileParams = context.ApiDescription
                .ActionAttributes()
                .OfType<FileParamsAttribute>();

            if (fileParams.Any())
            {
                //operation.Consumes.Add("multipart/form-data");

                if(operation.Parameters == null)
                    operation.Parameters = new List<IParameter>();

                foreach (var param in fileParams)
                {
                    operation.Parameters.Add(new NonBodyParameter()
                    {
                        Name = param.Name,
                        Required = param.IsRequired,
                        Description = param.Description,
                        In = "formData",
                        Type = "file"
                    });
                }
            }
        }
    }
}
