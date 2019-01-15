using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("[controller]")]
    [Produces("application/json")]
    [Consumes("application/json", "multipart/form-data")]
    public class WarrantyController : Controller
    {
        public IConfiguration Configuration { get; }
        private readonly ILogger<RequestController> _logger;

        public WarrantyController(IConfiguration configuration, ILogger<RequestController> logger)
        {
            Configuration = configuration;
            _logger = logger;
        }

        [HttpGet("doc_types")]
        public IEnumerable<WarrantyTypeDto> GetDocTypes()
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            return RequestService.WarrantyGetDocTypes(workerId);
        }
        [HttpGet("docs/{id}")]
        public IEnumerable<WarrantyDocDto> GetDocs(int id)
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            return RequestService.WarrantyGetDocs(workerId, id);
        }
        [HttpPost("docs/{id}")]
        public IActionResult AddDoc(int id, [FromForm] IFormFile file, [FromForm] int typeId, [FromForm] int? orgId,
           [FromForm] string name, [FromForm] DateTime docDate, [FromForm] string direction)
        {
            if (id == 0 || file == null || file.Length == 0 || typeId == 0)
            {
                return BadRequest();
            }
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            if (int.TryParse(workerIdStr, out int workerId))
            {
                var uploadFolder = Path.Combine(GetRootFolder(), id.ToString());
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }
                var fileExtension = Path.GetExtension(file.FileName);
                var fileName = Guid.NewGuid() + fileExtension;
                using (var fileStream = new FileStream(Path.Combine(uploadFolder, fileName), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
                RequestService.WarrantyAddDoc(id, orgId, typeId, name, docDate, fileName, direction, workerId, fileExtension?.TrimStart('.'));
                return Ok();
            }
            return BadRequest();
        }
        [HttpDelete("docs/{id}")]
        public IActionResult DeleteDoc(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            if (int.TryParse(workerIdStr, out int workerId))
            {
                RequestService.WarrantyDeleteDoc(id, workerId);
                return Ok();
            }
            return BadRequest();
        }
        [HttpGet("info/{id}")]
        public WarrantyInfoDto GetInfo(int id)
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            return RequestService.WarrantyGetInfo(workerId, id);
        }

        [HttpPut("info/{id}")]
        public IActionResult SetInfo(int id,[FromBody]WarrantyInfoDto info)
        {
            try
            {
                var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
                int.TryParse(workerIdStr, out int workerId);
                RequestService.WarrantySetInfo(workerId, info);
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpGet("orgs")]
        public IEnumerable<WarrantyOrganizationDto> GetOrganizations()
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            return RequestService.WarrantyGetOrganizations(workerId);
        }
        [HttpPost("orgs")]
        public IActionResult AddOrganization([FromBody]WarrantyOrganizationDto org)
        {
            try
            {
                var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
                int.TryParse(workerIdStr, out int workerId);
                RequestService.WarrantyAddOrg(workerId, org);
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }
        [HttpPut("orgs/{id}")]
        public IActionResult EditOrganization(int id,[FromBody]WarrantyOrganizationDto org)
        {
            try
            {
                var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
                int.TryParse(workerIdStr, out int workerId);
                org.Id = id;
                RequestService.WarrantyEditOrg(workerId, org);
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }
        [HttpPut("set_state/{id}")]
        public IActionResult SetWarrantyState(int id, [FromForm] IFormFile file, [FromForm] int typeId, [FromForm] int newState,
           [FromForm] string name, [FromForm] DateTime docDate)
        {
            if (id == 0 || file == null || file.Length == 0 || typeId == 0)
            {
                return BadRequest();
            }
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            if (int.TryParse(workerIdStr, out int workerId))
            {
                var uploadFolder = Path.Combine(GetRootFolder(), id.ToString());
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }
                var fileExtension = Path.GetExtension(file.FileName);
                var fileName = Guid.NewGuid() + fileExtension;
                using (var fileStream = new FileStream(Path.Combine(uploadFolder, fileName), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
                RequestService.WarrantyAddDoc(id, null, typeId, name, docDate, fileName, "in", workerId, fileExtension?.TrimStart('.'));
                RequestService.WarrantySetState(id, newState, workerId);
                return Ok();
            }
            return BadRequest();
        }
        [HttpGet("docs/file/{id}")]
        public byte[] GetDocAttachment(int id)
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            if (int.TryParse(workerIdStr, out int workerId))
            {

                var rootFolder = GetRootFolder();
                var fileInfo = RequestService.WarrantyGetDocFileName(workerId, id);
                return RequestService.DownloadFile(fileInfo.RequestId, fileInfo.FileName, rootFolder);
            }
            return null;
        }
        private string GetRootFolder()
        {
            return Configuration.GetValue<string>("Settings:RootFolder");
        }
    }
}