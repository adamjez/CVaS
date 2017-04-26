using System;

namespace CVaS.Web.Swagger
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class FileParamsAttribute : Attribute
    {
        public string Name { get; set; }

        public bool IsRequired { get; set; }

        public string Description { get; set; }
        public FileParamsAttribute(string name, bool isRequired = true, string description = "")
        {
            Name = name;
            IsRequired = isRequired;
            Description = description;
        }
    }
}
