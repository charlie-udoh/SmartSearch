using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartSearch.Core.Interfaces;
using SmartSearch.Web;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Smartdata.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : Controller
    {
        private readonly IDataService _dataService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DataController(IDataService dataService, IWebHostEnvironment webHostEnvironment)
        {
            _dataService = dataService;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: api/data/search
        [HttpGet("search")]
        public async Task<IActionResult> Get([FromQuery]string searchPhrase, [FromQuery] List<string> market, [FromQuery] int skip = 1, [FromQuery] int limit = 25)
        {
            return Ok(await _dataService.SearchData(searchPhrase, market, limit, skip));
        }

        // POST: api/data
        [HttpPost]
        public async Task<IActionResult> Post([FromForm] string documentType, [FromForm] IFormFile file)
        {
            if (file == null)
            {
                return BadRequest(ModelState);
            }
            var fileInfo = new FileInfo(file.FileName);
            if (fileInfo.Extension != ".json")
            {
                return BadRequest("Invalid file type");
            }
            if (!_dataService.GetAllowedDocumentTypes().Any(s => s == documentType))
            {
                return BadRequest("Document Type is not known");
            }
            var savedFile = await Helper.SaveFile(file, "Documents", _webHostEnvironment);
            var result = await _dataService.SaveData(savedFile, documentType);
            if (!result)
                return StatusCode(500, "Upload Failed");
            return Ok("Successful");
        }
    }
}