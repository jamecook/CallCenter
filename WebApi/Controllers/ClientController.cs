using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
    public class ClientController : Controller
    {
        public IConfiguration Configuration { get; }
        private readonly ILogger<RequestController> _logger;

        public ClientController(IConfiguration configuration, ILogger<RequestController> logger)
        {
            Configuration = configuration;
            _logger = logger;
        }

        [HttpGet("streets")]
        public ActionResult<StreetDto[]> GetStreets()
        {
            var clientIdStr = User.Claims.FirstOrDefault(c => c.Type == "ClientId")?.Value;
            int.TryParse(clientIdStr, out int clientId);
            if (clientId == 0)
                return BadRequest("1000:Error in JWT");
            return RequestService.GetStreetsByClient(clientId);
        }
        [HttpGet("houses/{id}")]
        public ActionResult<WebHouseDto[]> GetHouses(int id)
        {
            var clientIdStr = User.Claims.FirstOrDefault(c => c.Type == "ClientId")?.Value;
            int.TryParse(clientIdStr, out int clientId);
            if (clientId == 0)
                return BadRequest("1000:Error in JWT");

            return RequestService.GetHousesByStreetAndClientId(clientId, id);
        }
        [HttpGet("house_flats/{id}")]
        public ActionResult<FlatDto[]> GetHouseFlats(int id)
        {
            var clientIdStr = User.Claims.FirstOrDefault(c => c.Type == "ClientId")?.Value;
            int.TryParse(clientIdStr, out int clientId);
            if (clientId == 0)
                return BadRequest("1000:Error in JWT");
            return RequestService.GetFlatsForClient(clientId, id);
        }

        [HttpGet("parent_services")]
        public ActionResult<ServiceDto[]> GetParrentServices()
        {
            var clientIdStr = User.Claims.FirstOrDefault(c => c.Type == "ClientId")?.Value;
            int.TryParse(clientIdStr, out int clientId);
            if (clientId == 0)
                return BadRequest("1000:Error in JWT");

            return RequestService.GetParentServicesForClient(clientId, null);
        }
        [HttpGet("services")]
        public ActionResult<ServiceDto[]> GetServices([ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] parentIds)
        {
            var clientIdStr = User.Claims.FirstOrDefault(c => c.Type == "ClientId")?.Value;
            int.TryParse(clientIdStr, out int clientId);
            if (clientId == 0)
                return BadRequest("1000:Error in JWT");

            return RequestService.GetServicesForClient(clientId, parentIds);
        }

        [HttpPost("request")]
        public ActionResult<string> Post([FromBody]ClientRequestDto value)
        {
            _logger.LogDebug("Create Request: " + JsonConvert.SerializeObject(value));
            var clientIdStr = User.Claims.FirstOrDefault(c => c.Type == "ClientId")?.Value;
            int.TryParse(clientIdStr, out int clientId);
            if (clientId == 0)
                return BadRequest("1000:Error in JWT");
            return RequestService.ClientCreateRequest(clientId, value.AddressId, value.TypeId, value.Description);
        }
        [HttpGet("request")]
        public ActionResult<ClientRequestForListDto[]> GetRequests([FromQuery]string requestId,
            [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] addresses)
        {
            var clientIdStr = User.Claims.FirstOrDefault(c => c.Type == "ClientId")?.Value;
            int.TryParse(clientIdStr, out int clientId);
            if (clientId == 0)
                return BadRequest("1000:Error in JWT");

            int? rId = null;
            if (!string.IsNullOrEmpty(requestId))
            {
                if (int.TryParse(requestId, out int parseId))
                {
                    rId = parseId;
                }
                else
                {
                    rId = -1;
                }
            }

            return RequestService.ClientRequestListArrayParam(clientId, rId,
                fromDate ?? DateTime.Today,
                toDate ?? DateTime.Today.AddDays(1),
                 addresses);
        }

        [HttpPost("address")]
        public ActionResult AddAddress([FromBody]AddressIdDto value)
        {
            var clientIdStr = User.Claims.FirstOrDefault(c => c.Type == "ClientId")?.Value;
            int.TryParse(clientIdStr, out int clientId);
            if (clientId == 0)
                return BadRequest("1000:Error in JWT");
            var result = RequestService.AddAddress(clientId, value.AddressId);
            if(result == 0)
                return Ok();
            return BadRequest(result);
        }
        [HttpGet("address")]
        public ActionResult<AddressDto[]> GetAddresses()
        {
            var clientIdStr = User.Claims.FirstOrDefault(c => c.Type == "ClientId")?.Value;
            int.TryParse(clientIdStr, out int clientId);
            if (clientId == 0)
                return BadRequest("1000:Error in JWT");
            return RequestService.GetAddresses(clientId);
        }
        [HttpDelete("address/{id}")]
        public ActionResult DeleteAddress(int id)
        {
            var clientIdStr = User.Claims.FirstOrDefault(c => c.Type == "ClientId")?.Value;
            int.TryParse(clientIdStr, out int clientId);
            if (clientId == 0)
                return BadRequest("1000:Error in JWT");
            RequestService.DeleteAddress(clientId, id);
            return Ok();
        }

        [HttpGet("attachments/{id}")]
        public ActionResult<AttachmentDto[]> GetAttachments(int id)
        {
            var clientIdStr = User.Claims.FirstOrDefault(c => c.Type == "ClientId")?.Value;
            int.TryParse(clientIdStr, out int clientId);
            if (clientId == 0)
                return BadRequest("1000:Error in JWT");
            return RequestService.ClientGetAttachments(clientId, id);
        }
        [HttpGet("attachment")]
        public byte[] GetAttachment([FromQuery]string requestId, [FromQuery]string fileName)
        {
            int? rId = null;
            if (!string.IsNullOrEmpty(requestId) && int.TryParse(requestId, out int parseId))
            {
                rId = parseId;
            }
            if (!rId.HasValue) return null;

            var rootFolder = GetRootFolder();
            return RequestService.DownloadFile(rId.Value, fileName, rootFolder);
        }

        [HttpPost("attachment/{id}")]
        public async Task<IActionResult> AddFileToRequest(int id, [FromForm(Name = "file")] IFormFile[] files)
        {
            var clientIdStr = User.Claims.FirstOrDefault(c => c.Type == "ClientId")?.Value;
            int.TryParse(clientIdStr, out int clientId);
            if (clientId == 0)
                return BadRequest("1000:Error in JWT");
            if (files == null || files.Length == 0)
                return BadRequest("2000:Empty file");
            foreach (var file in files)
            {
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
                RequestService.ClientAttachFileToRequest(clientId, id, file.FileName, fileName);
            }
            return Ok();
        }
        private string GetRootFolder()
        {
            return Configuration.GetValue<string>("Settings:RootFolder");
        }
    }
}