using System;
using CVaS.DAL.Model;

namespace CVaS.BL.DTO
{
    public class RunDTO
    {
        public int Id { get; set; }

        public string AlgorithmCode { get; set; }

        public int? FileId { get; set; }

        public DateTime CreatedAt { get; set; }

        public string StdOut { get; set; }

        public string StdErr { get; set; }

        public RunResultType Result { get; set; }
    }
}