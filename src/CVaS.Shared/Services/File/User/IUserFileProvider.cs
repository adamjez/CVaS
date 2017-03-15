﻿using System.IO;
using System.Threading.Tasks;

namespace CVaS.Shared.Services.File.Providers
{
    public interface IUserFileProvider
    {
        Task<string> SaveAsync(string filePath, string contentType);

        Task<string> SaveAsync(Stream stream, string fileName, string contentType);

        Task<FileResult> Get(string id);

        Task DeleteAsync(string id);
    }
}