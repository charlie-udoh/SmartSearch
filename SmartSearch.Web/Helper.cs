using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SmartSearch.Web
{
    public static class Helper
    {
        public static async Task<string> SaveFile(IFormFile file, string folderName, IWebHostEnvironment _hostingEnvironment)
        {
            string webRootPath = _hostingEnvironment.ContentRootPath;
            string rootDirectory = Path.Combine(webRootPath, folderName);

            if (!Directory.Exists(rootDirectory))
            {
                Directory.CreateDirectory(rootDirectory);
            }

            var fileInfor = new FileInfo(file.FileName);
            string fullPath = Path.Combine(rootDirectory, fileInfor.Name);
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return fullPath;
        }
    }
}
