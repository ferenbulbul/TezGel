using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TezGel.Application.Interfaces.Services
{
    public interface IFileService
    {
         Task<string> SaveImageAsync(IFormFile file);
    }
}