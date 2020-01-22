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
using RestSharp;
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

        [HttpGet("orgs")]
        public IEnumerable<DocOrgDto> GetOrganisations()
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            return RequestService.DocsGetOrganisations(workerId);
        }

        [HttpPost("orgs")]
        public IActionResult AddOrganisation([FromBody] DocOrgDto value)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            return Ok(RequestService.DocsAddOrganisations(workerId,value));
        }
        [HttpPut("orgs/{id}")]
        public IActionResult UpdateOrganisation(int id,[FromBody] DocOrgDto value)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            RequestService.DocsUpdateOrganisations(workerId, id, value);
            return Ok();
        }
        [HttpDelete("orgs/{id}")]
        public IActionResult DeleteOrganisation(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            RequestService.DocsDeleteOrganisations(workerId, id);
            return Ok();
        }


        [HttpGet("types")]
        public IEnumerable<DocTypeDto> GetTypes()
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            return RequestService.DocsGetTypes(workerId);
        }

        [HttpGet("ord_types")]
        public IEnumerable<DocTypeDto> GetOrdTypes()
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            return RequestService.DocsGetOrdTypes(workerId);
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
            [FromQuery] string inNumber,[FromQuery] int? documentId, [FromQuery] int? appointedWorkerId,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] orgs,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] statuses,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] types,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] streets,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] houses,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] addresses
            )
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            return RequestService.DocsGetList(workerId, fromDate ?? DateTime.Today, toDate ?? DateTime.Today.AddDays(1),
                inNumber,  orgs, statuses, types, documentId, appointedWorkerId,streets,houses,addresses);
        }

        [HttpPost]
        public IActionResult Post([FromBody]CreateDocDto value)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            _logger.LogInformation("---------- Create Doc: " + JsonConvert.SerializeObject(value));
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);

            return Ok(RequestService.CreateDoc(workerId,  value.TypeId, value.Topic, value.DocNumber, value.DocDate, value.InNumber, value.InDate, value.OrgId, value.Orgs, value.OrganizationalTypeId, value.Description, value.AppointedWorkerId,value.AddressId));
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody]CreateDocDto value)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            _logger.LogInformation($"---------- Update Doc with id ({id}): " + JsonConvert.SerializeObject(value));
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            RequestService.UpdateDoc(workerId,id, value.TypeId, value.Topic, value.DocNumber, value.DocDate,
                value.InNumber, value.InDate, value.OrgId, value.OrganizationalTypeId, value.Description,
                value.AppointedWorkerId, value.AddressId);
            return Ok();
                //RequestService.CreateDoc(workerId, id, value.CreateDate, value.InNumber, value.OutNumber, value.InDate, value.OutDate, value.AgentId, value.StatusId, value.KindId, value.TypeId, value.Description);
        }

        [HttpPost("addOrgToDoc/{id}")]
        public IActionResult AddOrgToDoc(int id, [FromBody] OrgDocDto value)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            _logger.LogInformation($"---------- AddOrgToDoc id ({id}): " + JsonConvert.SerializeObject(value));
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            RequestService.AddOrgToDoc(workerId, id, value.OrgId, value.InNumber, value.InDate);
            return Ok();
        }
        [HttpPost("addAddress/{id}")]
        public IActionResult AddAddressToDoc(int id, [FromBody] int addressId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            _logger.LogInformation($"---------- AddOrgToDoc id ({id}): AddressId {addressId}");
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            RequestService.AddAddressToDoc(workerId, id, addressId);
            return Ok();
        }
        [HttpPut("updateOrgInDoc/{id}")]
        public IActionResult UpdateOrgInDoc(int id, [FromBody] OrgDocDto value)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            _logger.LogInformation($"---------- UpdateOrgInDoc id ({id}): " + JsonConvert.SerializeObject(value));
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            RequestService.UpdateOrgInDoc(workerId, id, value.OrgId, value.InNumber, value.InDate);
            return Ok();
        }
        [HttpDelete("deleteOrgFromDoc")]
        public IActionResult DeleteOrgFromDoc([FromQuery] int docId, [FromQuery] int orgId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            _logger.LogInformation($"---------- DeleteOrgFromDoc id ({docId}), orgId ({orgId})");
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            RequestService.DeleteOrgFromDoc(workerId, docId, orgId);
            return Ok();
        }

        [HttpGet("workers")]
        public IEnumerable<WorkerDto> GetWorkers()
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            return RequestService.DocGetWorkers(workerId);
        }

        [HttpPost("attach_file/{id}")]
        public async Task<IActionResult> AttachFileToDoc(int id, [FromForm] IFormFile file)
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
            RequestService.AttachFileToDoc(workerId, id, file.FileName, fileName, fileExtension?.TrimStart('.'));
            return Ok();
        }
        [HttpDelete("deleteAttach")]
        public IActionResult DeleteAttach([FromQuery] int docId, [FromQuery] int attachId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            _logger.LogInformation($"---------- DeleteAttach id ({docId}), attactId ({attachId})");
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            RequestService.DeleteAttachFromDoc(workerId, docId, attachId);
            return Ok();
        }
        [HttpDelete("deleteDoc")]
        public IActionResult DeleteDoc([FromQuery] int docId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            _logger.LogInformation($"---------- DeleteDoc id ({docId})");
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            RequestService.DeleteDoc(workerId, docId);
            return Ok();
        }
        [HttpGet("attachments/{id}")]
        public IEnumerable<AttachmentToDocDto> GetAttachments(int id)
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            return RequestService.GetAttachmentsToDocs(workerId, id);
        }
        [HttpGet("attachment")]
        public byte[] GetAttachment([FromQuery]string docId, [FromQuery]string fileName)
        {
            int? id = null;
            if (!string.IsNullOrEmpty(docId) && int.TryParse(docId, out int parseId))
            {
                id = parseId;
            }
            if (!id.HasValue) return null;

            var rootFolder = GetRootFolder();
            return RequestService.DownloadFile(id.Value, fileName, rootFolder);
        }

        private string GetRootFolder()
        {
            return Configuration.GetValue<string>("Settings:DocRootFolder");
        }

    }
}