using System.Collections.Generic;
using System.Linq;
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
    }
}