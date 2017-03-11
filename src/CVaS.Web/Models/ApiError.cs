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
        //[DataMember]
        //public List<ModelError> Errors { get; set; }

        public ApiError(string message)
        {
            this.Message = message;
            IsError = true;
        }

        //public ApiError(ModelStateDictionary modelState)
        //{
        //    this.IsError = true;
        //    if (modelState != null && modelState.Any(m => m.Value.Errors.Count > 0))
        //    {
        //        Message = "Please correct the specified errors and try again.";
        //        Errors = modelState.SelectMany(m => m.Value.Errors).ToList();
        //    }
        //}
    }
}
