using System;
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

            return RequestService.GetHousesByStreetAndWorkerId(id, clientId);
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
    }
}