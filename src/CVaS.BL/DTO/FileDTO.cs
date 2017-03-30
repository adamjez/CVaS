using System;
using System.Reflection;
using CVaS.DAL.Model;

namespace CVaS.BL.DTO
{
    public class FileDTO
    {
        public Guid Id { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public string Extension { get; set; }
        public FileType Type { get; set; }

        public static FileDTO FromEntity(File file)
        {
            return new FileDTO()
            {
                Id = file.Id,
                ContentType = file.ContentType,
                Extension = file.Extension,
                FileSize = file.FileSize,
                Type = file.Type
            };
        }

    }
}