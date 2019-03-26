using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
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
                return BadRequest();
            return RequestService.GetStreetsByClient(clientId);
        }
        [HttpGet("houses/{id}")]
        public ActionResult<WebHouseDto[]> GetHouses(int id)
        {
            var clientIdStr = User.Claims.FirstOrDefault(c => c.Type == "ClientId")?.Value;
            int.TryParse(clientIdStr, out int clientId);
            if (clientId == 0)
                return BadRequest();

            return RequestService.GetHousesByStreetAndWorkerId(id, clientId);
        }
        [HttpGet("house_flats/{id}")]
        public ActionResult<FlatDto[]> GetHouseFlats(int id)
        {
            var clientIdStr = User.Claims.FirstOrDefault(c => c.Type == "ClientId")?.Value;
            int.TryParse(clientIdStr, out int clientId);
            if (clientId == 0)
                return BadRequest();
            return RequestService.GetFlatsForClient(id, clientId);
        }

        [HttpGet("parent_services")]
        public ActionResult<ServiceDto[]> GetParrentServices()
        {
            var clientIdStr = User.Claims.FirstOrDefault(c => c.Type == "ClientId")?.Value;
            int.TryParse(clientIdStr, out int clientId);
            if (clientId == 0)
                return BadRequest();

            return RequestService.GetParentServicesForClient(clientId, null);
        }
        [HttpGet("services")]
        public ActionResult<ServiceDto[]> GetServices([ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] parentIds)
        {
            var clientIdStr = User.Claims.FirstOrDefault(c => c.Type == "ClientId")?.Value;
            int.TryParse(clientIdStr, out int clientId);
            if (clientId == 0)
                return BadRequest();

            return RequestService.GetServicesForClient(clientId, parentIds);
        }

        [HttpPost]
        public string Post([FromBody]CreateRequestDto value)
        {
            _logger.LogDebug("Create Request: " + JsonConvert.SerializeObject(value));
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            return RequestService.CreateRequest(workerId, value.Phone, value.Name, value.AddressId, value.TypeId, value.MasterId, value.ExecuterId, value.Description, value.IsChargeable ?? false, value.ExecuteDate, value.WarrantyId ?? 0, value.IsImmediate ?? false);
        }
    }
}