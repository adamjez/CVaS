using System.Runtime.Serialization;
using CVaS.DAL.Model;
using System;

namespace CVaS.Web.Models
{
    public class AlgorithmResult
    {
        public string Title { get; set; }

        public string StdOut { get; set; }

        public string StdErr { get; set; }

        public string Zip { get; set; }

        public Guid RunId { get; set; }

        public RunResultType Status { get; set; }

        public double? Duration { get; set; }
    }
}
