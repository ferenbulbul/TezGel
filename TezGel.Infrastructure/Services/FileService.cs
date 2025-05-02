using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using TezGel.Application.Interfaces.Services;

namespace TezGel.Infrastructure.Services
{
     public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;

        public FileService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveImageAsync(IFormFile file)
        {
            var imageName = $"{Guid.NewGuid()}_{file.FileName}";
            var imagePath = Path.Combine(_env.WebRootPath, "images", imageName);

            Directory.CreateDirectory(Path.GetDirectoryName(imagePath)!);

            using var stream = new FileStream(imagePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/images/{imageName}";
        }

    }
}