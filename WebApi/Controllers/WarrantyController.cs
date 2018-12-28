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

        [HttpGet("types")]
        public IEnumerable<WarrantyTypesDto> GetTypes()
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            return RequestService.GetWarrantyTypes(workerId);
        }
        [HttpGet("orgs")]
        public IEnumerable<WarrantyOrganizationDto> GetOrganizations()
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            return RequestService.GetWarrantyOrganizations(workerId);
        }
        [HttpPost("orgs")]
        public IActionResult AddOrganization([FromBody]WarrantyOrganizationDto org)
        {
            try
            {
                var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
                int.TryParse(workerIdStr, out int workerId);
                RequestService.AddWarrantyOrg(workerId, org);
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
                RequestService.EditWarrantyOrg(workerId, org);
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }
        [HttpPut("set_state/{id}")]
        public IActionResult SetWarrantyState(int id, [FromForm] IFormFile file, [FromForm] int type, [FromForm] int newState,
           [FromForm] string name, [FromForm] DateTime docDate)
        {
            if (id == 0 || file == null || file.Length == 0 || type == 0)
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
                RequestService.SetGarantyState(id, newState, type, name, docDate, fileName, workerId);
                return Ok();
            }
            return BadRequest();
        }
        private string GetRootFolder()
        {
            return Configuration.GetValue<string>("Settings:RootFolder");
        }
    }
}