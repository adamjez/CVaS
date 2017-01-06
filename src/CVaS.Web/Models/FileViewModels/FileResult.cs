using System.Collections.Generic;

namespace CVaS.Web.Models.FileViewModels
{
    public class UploadFilesResult
    {
        public IEnumerable<int> Ids { get; set; }
        public bool Success { get; set; } = true;
    }
}