using System;
using System.Collections.Generic;

namespace CVaS.Web.Models.FileViewModels
{
    public class UploadFilesResult
    {
        public IEnumerable<Guid> Ids { get; set; }
        public bool Success { get; set; } = true;
    }
}