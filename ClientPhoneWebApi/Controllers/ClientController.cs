using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ClientPhoneWebApi.Dto;
using ClientPhoneWebApi.Repo;
using ClientPhoneWebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace ClientPhoneWebApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ClientController : ControllerBase
    {
        ILogger Logger { get; }
        private RequestService RequestService { get; }
        private static DateTime _lastGetActiveChannelsTime;
        private static ActiveChannelsDto[] _lastActiveChannels;
        private static readonly string ApiKey = "qwertyuiop987654321";
        public ClientController(RequestService requestService, ILogger<ProductsController> logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            RequestService = requestService ?? throw new ArgumentNullException(nameof(requestService));
            _lastGetActiveChannelsTime = _lastGetActiveChannelsTime > DateTime.MinValue
                ? _lastGetActiveChannelsTime
                : DateTime.MinValue;
        }

        /// <summary>
        /// Get all products
        /// </summary>
        /// <response code="200">List of all products</response>
        [HttpGet]
        public Dto.WebUserDto Get([FromQuery]string user,[FromQuery] string password)
        {
            var result = RequestService.WebLogin(user, password);
            return result;
        }
        [HttpGet("activeCalls")]
        public IActionResult GetActiveCalls([FromQuery]int userId)
        {
            if (userId == 0)
                return BadRequest();
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }

            if (_lastGetActiveChannelsTime.AddSeconds(1) < DateTime.Now)
            {
                _lastActiveChannels = RequestService.GetActiveChannels(userId);
                _lastGetActiveChannelsTime = DateTime.Now;
            }
            return Ok(_lastActiveChannels);
        }

        [HttpGet("getSipInfoByIp")]
        public IActionResult GetSipInfoByIp([FromQuery]string ipAddr)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetSipInfoByIp(ipAddr));
        }

        [HttpGet("getDispatchers")]
        public IActionResult GetDispatchers([FromQuery]int companyId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetDispatchers(companyId));
        }

        [HttpGet("getFilterDispatchers")]
        public IActionResult GetFilterDispatchers([FromQuery]int userId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetFilterDispatchers(userId));
        }

        [HttpGet("getFilterCompanies")]
        public IActionResult GetFilterCompanies([FromQuery]int userId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetFilterServiceCompanies(userId));
        }

        [HttpGet("getCompaniesForCall")]
        public IActionResult GetCompaniesForCall([FromQuery]int userId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetServiceCompaniesForCall(userId));
        }

        [HttpGet("getNotAnswered")]
        public IActionResult GetNotAnswered([FromQuery]int userId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetNotAnsweredCalls(userId));
        }
        [HttpGet("getCallList")]
        public IActionResult GetCallList([FromQuery]int userId, [FromQuery]DateTime fromDate, [FromQuery]DateTime toDate, [FromQuery]string requestId, [FromQuery]int? operatorId, [FromQuery]int? serviceCompanyId, [FromQuery]string phoneNumber)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetCallList(fromDate, toDate, requestId, operatorId, serviceCompanyId, phoneNumber));
        }
        [HttpGet("getRequestByPhone")]
        public IActionResult GetRequestByPhone([FromQuery]int userId, [FromQuery]string phoneNumber)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetRequestByPhone(userId, phoneNumber));
        }
        [HttpGet("sendAlive")]
        public IActionResult SendAlive([FromQuery]int userId, [FromQuery]string sipUser)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            RequestService.SendAlive(userId, sipUser);
            return Ok();
        }
        [HttpGet("logout")]
        public IActionResult Logout([FromQuery]int userId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            RequestService.Logout(userId);
            return Ok();
        }

        [HttpGet("increaseRingCount")]
        public IActionResult IncreaseRingCount([FromQuery]int userId,[FromQuery]string callId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            RequestService.IncreaseRingCount(userId,callId);
            return Ok();
        }
        [HttpGet("GetRecordById")]
        public IActionResult getRecordById([FromQuery]int userId,[FromQuery]string path)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetRecordById(userId, path));
        }

        [HttpGet("attachCall")]
        public IActionResult AttachCall([FromQuery]int userId, [FromQuery]int requestId, [FromQuery]string callId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            if(requestId == 0)
            {
                return BadRequest("RequestId mast be not null!");
            }

            RequestService.AddCallToRequest(userId, requestId, callId);
            return Ok();
        }

        [HttpGet("deleteByRingCount")]
        public IActionResult DeleteCallFromNotAnsweredListByTryCount([FromQuery]int userId,[FromQuery]string callId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            RequestService.DeleteCallFromNotAnsweredListByTryCount(userId,callId);
            return Ok();
        }

        [HttpGet("currentDate")]
        public IActionResult CurrentDate()
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetCurrentDate());
        }

        [HttpGet("getCallUniqueId")]
        public IActionResult GetCallUniqueId([FromQuery]int userId, [FromQuery]string callId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetUniqueIdByCallId(userId, callId));
        }

        [HttpGet("getTransferList")]
        public IActionResult GetTransferList([FromQuery] int userId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetTransferList(userId));
        }
        [HttpGet("login")]
        public IActionResult Login([FromQuery]string login,string password, string sipUser)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.Login(login, password, sipUser));
        }


        /*
        /// <summary>
        /// Get a product by id
        /// </summary>
        /// <param name="id">A product id</param>
        [ProducesResponseType(typeof(Dto.Product), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet]
        [Route("{id}")]
        public Dto.Product GetById(int id)
        {
            return Mapper.Map<Dto.Product>(ProductsRepo.GetById(id));
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        /// <param name="id">A new product id</param>
        /// <param name="newProductDto">New product data</param>
        /// <response code="201">The created product</response>
        [ProducesResponseType(typeof(Dto.Product), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        [Route("{id}")]
        public IActionResult Create(int id, [FromBody]Dto.UpdateProduct newProductDto)
        {
            var newProduct = new Model.Product(id);
            Mapper.Map(newProductDto, newProduct);
            ProductsRepo.Create(newProduct);

            var createdProduct = ProductsRepo.GetById(id);

            Logger.LogInformation("New product was created: {@product}", createdProduct);

            return Created($"{id}", Mapper.Map<Dto.Product>(createdProduct));
        }

        /// <summary>
        /// Update a product
        /// </summary>
        /// <param name="id">Id of the product to update</param>
        /// <param name="productDto">Product data</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut]
        [Route("{id}")]
        public IActionResult Update(int id, [FromBody]Dto.UpdateProduct productDto)
        {
            var product = ProductsRepo.GetById(id);
            Mapper.Map(productDto, product);
            ProductsRepo.Update(product);
            return Ok();
        }

        /// <summary>
        /// Delete a product
        /// </summary>
        /// <param name="id">Id of the product to delete</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete]
        [Route("{id}")]
        public IActionResult Delete(int id)
        {
            ProductsRepo.Delete(id);
            return Ok();
        }

        /// <summary>
        /// Example of an exception handling
        /// </summary>
        [HttpGet("ThrowAnException")]
        public IActionResult ThrowAnException()
        {
            throw new Exception("Example exception");
        }
    /**/
    }

}