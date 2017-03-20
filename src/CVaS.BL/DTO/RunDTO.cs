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

        public RunResultType Status { get; set; }

        public DateTime? FinishedAt { get; set; }

        public int? Duration => FinishedAt.HasValue ? (int?)(FinishedAt.Value - CreatedAt).TotalMilliseconds : null;

        public string Zip { get; set; }
    }
}