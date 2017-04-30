using System;
using System.Collections.Generic;

namespace CVaS.Web.Models.FileViewModels
{
    public struct UploadFileResult
    {
        public Guid Id { get; set; }

        public string FileName { get; set; }

        public UploadFileResult(Guid id, string fileName)
        {
            Id = id;
            FileName = fileName;
        }
    }
}