using System.Collections.Generic;
using CVaS.DAL.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CVaS.Web.Models
{
    public class AlgorithmOptions
    {
        public IList<AlgorithmArgument> Arguments { get; set; }
    }

    public class AlgorithmArgument
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ArgumentType Type { get; set; }
        public string Content { get; set; }
    }
}
