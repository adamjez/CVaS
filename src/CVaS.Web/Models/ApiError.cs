using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CVaS.Web.Models
{
    [DataContract]
    public class ApiError
    {
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public bool IsError { get; set; }
        [DataMember]
        public string Detail { get; set; }

        public ApiError(string message)
        {
            Message = message;
            IsError = true;
        }

    }
}
