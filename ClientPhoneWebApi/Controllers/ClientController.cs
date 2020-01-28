﻿using System;
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
        [HttpGet("getAlertRequests")]
        public IActionResult GetAlertRequests([FromQuery]int userId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetAlertRequestList(userId));
        }
        [HttpGet("getRequests")]
        public IActionResult GetRequests([FromQuery]int userId, [FromQuery]string requestId, [FromQuery] bool? filterByCreateDate,
                [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate,
                [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] streets, 
                [FromQuery]int? houseId,
                [FromQuery]int? addressId,
                [FromQuery]int? serviceId,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] parentServices,
            //[ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] services,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] statuses,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] workers,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] executors,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] ratings,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] companies,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] users,
            [FromQuery] bool? badWork,
            [FromQuery] bool? garanty,
            [FromQuery] bool? onlyRetry,
            [FromQuery] int? chargeable,
            [FromQuery] bool? onlyExpired,
            [FromQuery] bool? onlyByClient,
            [FromQuery] bool? immediate,
            [FromQuery] string clientPhone)
            //[ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] warranties,
            //[ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] immediates,
            //[ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] regions)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            var result = RequestService.GetRequestList(userId, requestId,
                filterByCreateDate ?? true,
                fromDate ?? DateTime.Today,
                toDate ?? DateTime.Today.AddDays(1),
                fromDate ?? DateTime.Today,
                toDate ?? DateTime.Today.AddDays(1),
                streets, houseId, addressId, parentServices, serviceId, statuses, workers, executors, companies, users, ratings,// warranties, immediates, regions,
                chargeable, badWork ?? false, onlyRetry ?? false, clientPhone,
                garanty ?? false, immediate ?? false, onlyByClient ?? false);
            return Ok(result);
        }
        [HttpGet("getMeters")]
        public IActionResult GetMeters([FromQuery]int userId, [FromQuery]int? companyId, [FromQuery]DateTime fromDate, [FromQuery]DateTime toDate)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetMetersByDate(userId, companyId, fromDate, toDate));
        }
        [HttpGet("getStreets")]
        public IActionResult GetStreets([FromQuery]int userId, [FromQuery]int? cityId, [FromQuery]int? companyId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetStreets(userId, cityId??1, companyId));
        }
        [HttpGet("getServices")]
        public IActionResult GetServices([FromQuery]int userId, [FromQuery]int? parentId, [FromQuery]int? houseId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetServices(userId, parentId, houseId));
        }
        [HttpGet("getServiceById")]
        public IActionResult GetServiceById([FromQuery]int userId, [FromQuery]int serviceId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetServiceById(userId, serviceId));
        }
        [HttpGet("getWorkerById")]
        public IActionResult GetWorkerById([FromQuery]int userId, [FromQuery]int workerId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetWorkerById(userId, workerId));
        }
        [HttpGet("getScheduleTaskByRequestId")]
        public IActionResult GetScheduleTaskByRequestId([FromQuery]int userId, [FromQuery]int requestId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetScheduleTaskByRequestId(userId, requestId));
        }
        [HttpGet("getFlats")]
        public IActionResult GetFlats([FromQuery]int userId,  [FromQuery]int houseId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetFlats(userId, houseId));
        }
        [HttpGet("alertCountByHouseId")]
        public IActionResult AlertCountByHouseId([FromQuery]int userId,  [FromQuery]int houseId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.AlertCountByHouseId(userId, houseId));
        }
        [HttpGet("getServiceCompanyIdByHouseId")]
        public IActionResult GetServiceCompanyIdByHouseId([FromQuery]int userId,  [FromQuery]int houseId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetServiceCompanyIdByHouseId(userId, houseId));
        }
        [HttpGet("getHouseById")]
        public IActionResult GetHouseById([FromQuery]int userId,  [FromQuery]int houseId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetHouseById(userId, houseId));
        }
        [HttpGet("getActiveCallUniqueIdByCallId")]
        public IActionResult GetActiveCallUniqueIdByCallId([FromQuery]int userId,  [FromQuery]string callId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest(        "Authorization error!");
            }
            return Ok(RequestService.GetActiveCallUniqueIdByCallId(userId, callId));
        }
        [HttpGet("getAddressTypes")]
        public IActionResult GetAddressTypes([FromQuery]int userId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest(        "Authorization error!");
            }
            return Ok(RequestService.GetAddressTypes(userId));
        }
        [HttpGet("getCities")]
        public IActionResult GetCities([FromQuery]int userId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest(        "Authorization error!");
            }
            return Ok(RequestService.GetCities(userId));
        }
        [HttpGet("getStatuses")]
        public IActionResult GetStatuses([FromQuery]int userId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetStatuses(userId));
        }
        [HttpGet("getMasters")]
        public IActionResult GetMasters([FromQuery]int userId, [FromQuery]int? companyId, [FromQuery]bool? showOnlyExecutors)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetMasters(userId, companyId, showOnlyExecutors??true));
        }
        [HttpGet("getExecutors")]
        public IActionResult GetExecutors([FromQuery]int userId, [FromQuery]int? companyId, [FromQuery]bool? showOnlyExecutors)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetExecutors(userId, companyId, showOnlyExecutors??true));
        }
        [HttpGet("getRequest")]
        public IActionResult GetRequest([FromQuery]int userId, [FromQuery]int requestId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetRequest(userId, requestId));
        }
        [HttpGet("getHouses")]
        public IActionResult GetHouses([FromQuery]int userId, [FromQuery]int? companyId, [FromQuery]int streetId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetHouses(userId, companyId, streetId));
        }

        [HttpGet("getDispatcherStat")]
        public IActionResult GetDispatcherStat([FromQuery]int userId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetDispatcherStatistics(userId));
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