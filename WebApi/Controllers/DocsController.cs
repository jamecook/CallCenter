using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("[controller]")]
    [Produces("application/json")]
    [Consumes("application/json", "multipart/form-data")]
    public class DocsController : Controller
    {
        public IConfiguration Configuration { get; }
        private readonly ILogger<RequestController> _logger;

        public DocsController(IConfiguration configuration, ILogger<RequestController> logger)
        {
            Configuration = configuration;
            _logger = logger;
        }

        [HttpGet("agents")]
        public IEnumerable<DocOrgDto> GetAgents()
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            return RequestService.DocsGetAgents(workerId);
        }

        [HttpGet("types")]
        public IEnumerable<DocTypeDto> GetTypes()
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            return RequestService.DocsGetTypes(workerId);
        }
        [HttpGet("statuses")]
        public IEnumerable<DocStatusDto> GetStatuses()
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            return RequestService.DocsGetStatuses(workerId);
        }
        [HttpGet]
        public IEnumerable<DocDto> GetDocs([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate,
            [FromQuery] string inNumber, [FromQuery] string outNumber,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] orgs,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] statuses,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] types)
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            return RequestService.DocsGetList(workerId, fromDate ?? DateTime.Today, toDate ?? DateTime.Today.AddDays(1),
                inNumber, outNumber, orgs, statuses, types);
        }
        [HttpPost]
        public string Post([FromBody]CreateOrUpdateDocDto value)
        {
            _logger.LogInformation("---------- Create Doc: " + JsonConvert.SerializeObject(value));
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);

            return RequestService.CreateDoc(workerId,  value.TypeId, value.Topic, value.DocNumber, value.DocDate, value.InNumber, value.InDate, value.OutNumber,
                value.OutDate, value.OrgId, value.OrganizationalTypeId, value.Description);
        }
        [HttpPut("{id}")]
        public string Post(int id, [FromBody]CreateOrUpdateDocDto value)
        {
            _logger.LogInformation($"---------- Update Doc with id ({id}): " + JsonConvert.SerializeObject(value));
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            return "";
                //RequestService.CreateDoc(workerId, id, value.CreateDate, value.InNumber, value.OutNumber, value.InDate, value.OutDate, value.AgentId, value.StatusId, value.KindId, value.TypeId, value.Description);
        }

        [HttpPost("add_file/{id}")]
        public async Task<IActionResult> AddFileToRequest(int id, [FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest();
            _logger.LogDebug($"FileLen: {file.Length}, FileName: {file.FileName}");
            var uploadFolder = Path.Combine(GetRootFolder(), id.ToString());
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }
            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = Guid.NewGuid() + fileExtension;
            using (var fileStream = new FileStream(Path.Combine(uploadFolder, fileName), FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            RequestService.AttachFileToRequest(workerId, id, file.FileName, fileName);
            return Ok();
        }

        private string GetRootFolder()
        {
            return Configuration.GetValue<string>("Settings:DocRootFolder");
        }

    }
}