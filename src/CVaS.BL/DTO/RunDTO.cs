using System;
using CVaS.DAL.Model;

namespace CVaS.BL.DTO
{
    public class RunDTO
    {
        public Guid Id { get; set; }

        public string AlgorithmCode { get; set; }

        public Guid? FileId { get; set; }

        public DateTime CreatedAt { get; set; }

        public string StdOut { get; set; }

        public string StdErr { get; set; }

        public RunResultType Status { get; set; }

        public DateTime? FinishedAt { get; set; }

        public double? Duration => FinishedAt.HasValue ? (double?)(FinishedAt.Value - CreatedAt).TotalMilliseconds : null;

        public string Zip { get; set; }
    }
}