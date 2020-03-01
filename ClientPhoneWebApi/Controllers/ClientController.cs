using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClientPhoneWebApi.Dto;
using ClientPhoneWebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ClientPhoneWebApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Consumes("application/json", "multipart/form-data")]
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
        [HttpGet("getDispatchers2")]
        public IActionResult GetDispatchers2([FromQuery]int companyId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetDispatchers2(companyId));
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
        [HttpGet("GetRequestStatuses")]
        public IActionResult GetRequestStatuses([FromQuery]int userId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetRequestStatuses(userId));
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
            return Ok(RequestService.GetCallList(userId, fromDate, toDate, requestId, operatorId, serviceCompanyId, phoneNumber));
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
        [HttpGet("GetSmsSettingsForServiceCompany")]
        public IActionResult GetSmsSettingsForServiceCompany([FromQuery]int userId, [FromQuery]int? serviceCompanyId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetSmsSettingsForServiceCompany(userId, serviceCompanyId));
        }
        [HttpGet("GetStatusHistoryByRequest")]
        public IActionResult GetStatusHistoryByRequest([FromQuery]int userId, [FromQuery]int requestId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetStatusHistoryByRequest(userId, requestId));
        }
        [HttpGet("GetExecutorHistoryByRequest")]
        public IActionResult GetExecutorHistoryByRequest([FromQuery]int userId, [FromQuery]int requestId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetExecutorHistoryByRequest(userId, requestId));
        }
        [HttpGet("GetMasterHistoryByRequest")]
        public IActionResult GetMasterHistoryByRequest([FromQuery]int userId, [FromQuery]int requestId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetMasterHistoryByRequest(userId, requestId));
        }
        [HttpGet("GetCallListByRequestId")]
        public IActionResult GetCallListByRequestId([FromQuery]int userId, [FromQuery]int requestId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetCallListByRequestId(userId, requestId));
        }
        [HttpGet("GetSmsByRequestId")]
        public IActionResult GetSmsByRequestId([FromQuery]int userId, [FromQuery]int requestId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetSmsByRequestId(userId, requestId));
        }
        [HttpGet("GetNotes")]
        public IActionResult GetNotes([FromQuery]int userId, [FromQuery]int requestId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetNotes(userId, requestId));
        }
        [HttpGet("GetAttachments")]
        public IActionResult GetAttachments([FromQuery]int userId, [FromQuery]int requestId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetAttachments(userId, requestId));
        }
        [HttpGet("GetRecordFileNameByUniqueId")]
        public IActionResult GetRecordFileNameByUniqueId([FromQuery]int userId, [FromQuery]string uniqueId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetRecordFileNameByUniqueId(userId, uniqueId));
        }
       [HttpGet("GetMeterCodes")]
        public IActionResult GetMeterCodes([FromQuery]int userId, [FromQuery]int addressId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetMeterCodes(userId, addressId));
        }
       [HttpGet("GetMetersByAddressId")]
        public IActionResult GetMetersByAddressId([FromQuery]int userId, [FromQuery]int addressId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetMetersByAddressId(userId, addressId));
        }
       [HttpGet("GetAlertServiceTypes")]
        public IActionResult GetAlertServiceTypes([FromQuery]int userId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetAlertServiceTypes(userId));
        }
       [HttpGet("GetAlertTypes")]
        public IActionResult GetAlertTypes([FromQuery]int userId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetAlertTypes(userId));
        }
       [HttpGet("GetAlertedRequests")]
        public IActionResult GetAlertedRequests([FromQuery]int userId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetAlertedRequests(userId));
        }
        [HttpPut("RequestChangeAddress")]
        public IActionResult RequestChangeAddress([FromBody]ChangeAddressDto value)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            RequestService.RequestChangeAddress(value.UserId, value.RequestId, value.AddressId);
            return Ok();
        }
        [HttpPut("AddCallToMeter")]
        public IActionResult AddCallToMeter([FromBody]AddCallToMeterDto value)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            RequestService.AddCallToMeter(value.UserId, value.MeterId, value.CallUniqueId);
            return Ok();
        }
        [HttpPut("SaveMeterCodes")]
        public IActionResult SaveMeterCodes([FromBody]SaveMeterCodesDto value)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }        
            RequestService.SaveMeterCodes(value.UserId, value.SelectedFlatId, value.PersonalAccount, value.Electro1Code, value.Electro2Code, value.HotWater1Code, value.ColdWater1Code,
            value.HotWater2Code, value.ColdWater2Code, value.HotWater3Code, value.ColdWater3Code, value.HeatingCode, value.Heating2Code, value.Heating3Code, value.Heating4Code);
            return Ok();
        }
        [HttpPut("SaveMeterValues")]
        public IActionResult SaveMeterValues([FromBody]SaveMeterValuesDto value)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }        
            var result = RequestService.SaveMeterValues(value.UserId, value.PhoneNumber, value.AddressId, value.Electro1, value.Electro2, value.HotWater1, value.ColdWater1,
            value.HotWater2, value.ColdWater2, value.HotWater3, value.ColdWater3, value.Heating, value.MeterId, value.PersonalAccount, value.Heating2, value.Heating3, value.Heating4);
            return Ok(result);
        }
        [HttpPut("AddNewState")]
        public IActionResult AddNewState([FromBody]NewStateDto value)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            RequestService.AddNewState(value.UserId, value.RequestId, value.StateId);
            return Ok();
        }
        [HttpPut("SendSms")]
        public IActionResult SendSms([FromBody]SendSmsDto value)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            RequestService.SendSms(value.UserId, value.RequestId, value.Sender,value.Phone,value.Message,value.IsClient);
            return Ok();
        }
        [HttpPut("AddNewMaster")]
        public IActionResult AddNewMaster([FromBody]AddNewWorkerDto value)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            RequestService.AddNewMaster(value.UserId, value.RequestId, value.WorkerId);
            return Ok();
        }
        [HttpPut("AddNewExecutor")]
        public IActionResult AddNewExecutor([FromBody]AddNewWorkerDto value)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            RequestService.AddNewExecutor(value.UserId, value.RequestId, value.WorkerId);
            return Ok();
        }
        [HttpPut("AddNewExecuteDate")]
        public IActionResult AddNewExecuteDate([FromBody]NewExecuteDateDto value)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }

            value.ExecuteDate = value.ExecuteDate.AddHours(5);
            RequestService.AddNewExecuteDate(value.UserId, value.RequestId, value.ExecuteDate,value.Period,value.Note);
            return Ok();
        }
        [HttpPut("AddNewTermOfExecution")]
        public IActionResult AddNewTermOfExecution([FromBody]NewTermOfExecutionDto value)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }

            value.TermOfExecution = value.TermOfExecution.AddHours(5);
            RequestService.AddNewTermOfExecution(value.UserId, value.RequestId, value.TermOfExecution,value.Note);
            return Ok();
        }
        [HttpPut("AddNewNote")]
        public IActionResult AddNewNote([FromBody]NewNoteDto value)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            RequestService.AddNewNote(value.UserId, value.RequestId, value.Note);
            return Ok();
        }
        [HttpPut("AddScheduleTask")]
        public IActionResult AddScheduleTask([FromBody]AddScheduleTaskDto value)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }

            value.FromDate = value.FromDate.AddHours(5);
            value.ToDate = value.ToDate.AddHours(5);
            RequestService.AddScheduleTask(value.UserId, value.WorkerId,value.RequestId, value.FromDate,value.ToDate,value.EventDescription);
            return Ok();
        }
        [HttpPut("SetRequestWorkingTimes")]
        public IActionResult SetRequestWorkingTimes([FromBody]SetRequestWorkingTimesDto value)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }

            value.FromDate = value.FromDate.AddHours(5);
            value.ToDate = value.ToDate.AddHours(5);
            RequestService.SetRequestWorkingTimes(value.UserId, value.RequestId, value.FromDate,value.ToDate);
            return Ok();
        }
        [HttpPut("EditRequest")]
        public IActionResult EditRequest([FromBody]EditRequestDto value)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }

            value.AlertTime = value.AlertTime?.AddHours(5);
            value.TermOfExecution = value.TermOfExecution?.AddHours(5);
            RequestService.EditRequest(value.UserId, value.RequestId, value.RequestTypeId,value.RequestMessage,value.Immediate,value.Chargeable,value.IsBadWork,
                value.Warranty,value.IsRetry,value.AlertTime,value.TermOfExecution);
            return Ok();
        }
        [HttpDelete("DeleteScheduleTask")]
        public IActionResult DeleteScheduleTask([FromQuery]int userId, [FromQuery]int taskId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            RequestService.DeleteScheduleTask(userId, taskId);
            return Ok();
        }
        [HttpDelete("DeleteAttachment")]
        public IActionResult DeleteAttachment([FromQuery]int userId, [FromQuery]int attachmentId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            RequestService.DeleteAttachment(userId, attachmentId);
            return Ok();
        }
        [HttpDelete("DeleteRequestRatingById")]
        public IActionResult DeleteRequestRatingById([FromQuery]int userId, [FromQuery]int itemId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            RequestService.DeleteRequestRatingById(userId, itemId);
            return Ok();
        }
        [HttpDelete("DeleteCallListRecord")]
        public IActionResult DeleteCallListRecord([FromQuery]int userId, [FromQuery]int itemId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            RequestService.DeleteCallListRecord(userId, itemId);
            return Ok();
        }
        [HttpPut("ChangeDescription")]
        public IActionResult ChangeDescription([FromBody]ChangeDescriptionDto value)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            RequestService.ChangeDescription(value.UserId, value.RequestId, value.Description);
            return Ok();
        }
        [HttpPut("SetRating")]
        public IActionResult SetRating([FromBody]SetRatingDto value)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            RequestService.SetRating(value.UserId, value.RequestId, value.RatingId, value.Description);
            return Ok();
        }
        [HttpPost("SaveNewRequest")]
        public IActionResult SaveNewRequest([FromBody]NewRequestDto value)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }

            value.AlertTime = value.AlertTime?.AddHours(5);
            var result = RequestService.SaveNewRequest(value.UserId, value.LastCallId, value.AddressId, value.RequestTypeId, value.ContactList, value.RequestMessage,
            value.Chargeable, value.Immediate, value.CallUniqueId, value.Entrance, value.Floor, value.AlertTime, value.IsRetry, value.IsBedWork, value.EquipmentId, value.Warranty);
            return Ok(result);
        }
        [HttpPost("SaveAlert")]
        public IActionResult SaveAlert([FromBody]SaveAlertDto value)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }

            value.StartDate = value.StartDate.AddHours(5);
            value.EndDate = value.EndDate?.AddHours(5);
            RequestService.SaveAlert(value);
            return Ok();
        }
        [HttpPost("SaveRingUpList")]
        public IActionResult SaveRingUpList([FromBody]RingUpListDto value)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            RequestService.SaveRingUpList(value.ConfigId,value.Records);
            return Ok();
        }
        [HttpPut("AbortRingUp")]
        public IActionResult AbortRingUp([FromBody]RingUpOperDto value)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            RequestService.AbortRingUp(value.UserId,value.RingUpId);
            return Ok();
        }
        [HttpPut("ContinueRingUp")]
        public IActionResult ContinueRingUp([FromBody]RingUpOperDto value)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            RequestService.ContinueRingUp(value.UserId,value.RingUpId);
            return Ok();
        }
        [HttpPut("AddCallHistory")]
        public IActionResult AddCallHistory([FromBody]AddCallHistoryDto value)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            RequestService.AddCallHistory( value.RequestId, value.CallUniqueId, value.UserId,value.CallId,value.MethodName);
            return Ok();
        }
        [HttpPut("AddCallToRequest")]
        public IActionResult AddCallToRequest([FromBody]AddCallToRequestDto value)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            RequestService.AddCallToRequest(value.UserId, value.RequestId, value.CallUniqueId);
            return Ok();
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
        [HttpGet("GetExecuteDateHistoryByRequest")]
        public IActionResult GetExecuteDateHistoryByRequest([FromQuery]int userId, [FromQuery]int requestId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetExecuteDateHistoryByRequest(userId, requestId));
        }
        [HttpGet("GetRingUpConfigs")]
        public IActionResult GetRingUpConfigs([FromQuery]int userId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetRingUpConfigs(userId));
        }
        [HttpGet("GetRequestRatings")]
        public IActionResult GetRequestRatings([FromQuery]int userId, [FromQuery]int requestId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {        
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetRequestRatings(userId, requestId));
        }
        [HttpGet("GetRequestRating")]
        public IActionResult GetRequestRating([FromQuery]int userId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {        
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetRequestRating(userId));
        }
[HttpGet("getScheduleTasks")]
        public IActionResult GetScheduleTasks([FromQuery]int userId, [FromQuery]int workerId,[FromQuery]DateTime fromDate, [FromQuery]DateTime toDate)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }

            return Ok(RequestService.GetScheduleTasks(userId, workerId, fromDate, toDate));
        }
        [HttpGet("GetAlerts")]
        public IActionResult GetAlerts([FromQuery]int userId, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery]int? houseId,[FromQuery]bool? onlyActive)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetAlerts(userId, fromDate, toDate, houseId, onlyActive??true));
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
        [HttpGet("GetLastAddressByClientPhone")]
        public IActionResult GetLastAddressByClientPhone([FromQuery]int userId, [FromQuery]string phone)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest(        "Authorization error!");
            }
            return Ok(RequestService.GetLastAddressByClientPhone(userId, phone));
        }
        [HttpGet("GetWorkersByHouseAndService")]
        public IActionResult GetWorkersByHouseAndService([FromQuery]int userId, [FromQuery]int houseId, [FromQuery]int parentServiceTypeId, [FromQuery]bool showMasters)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest(        "Authorization error!");
            }
            return Ok(RequestService.GetWorkersByHouseAndService(userId, houseId,parentServiceTypeId,showMasters));
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
        [HttpGet("GetPeriods")]
        public IActionResult GetPeriods([FromQuery]int userId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetPeriods(userId));
        }
        [HttpGet("GetEquipments")]
        public IActionResult GetEquipments([FromQuery]int userId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetEquipments(userId));
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
        public IActionResult SendAlive([FromQuery]int userId, [FromQuery]string sipUser, [FromQuery]string version)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            RequestService.SendAlive(userId, sipUser, version);
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
        [HttpGet("GetAlertTimes")]
        public IActionResult GetAlertTimes([FromQuery]int userId, [FromQuery]bool isImmediate)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetAlertTimes(userId, isImmediate));
        }
        [HttpGet("GetOnlyActiveCallUniqueIdByCallId")]
        public IActionResult GetOnlyActiveCallUniqueIdByCallId([FromQuery]int userId, [FromQuery]string callId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetOnlyActiveCallUniqueIdByCallId(userId, callId));
        }
        [HttpGet("GetSipServer")]
        public IActionResult GetSipServer()
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetSipServer());
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
        [HttpGet("GetRingUpHistory")]
        public IActionResult GetRingUpHistory([FromQuery] int userId, [FromQuery] DateTime fromDate)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetRingUpHistory(userId, fromDate));
        }
        [HttpGet("GetRingUpInfo")]
        public IActionResult GetRingUpInfo([FromQuery] int userId, [FromQuery] int ringUpId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != ApiKey)
            {
                return BadRequest("Authorization error!");
            }
            return Ok(RequestService.GetRingUpInfo(userId, ringUpId));
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
        [HttpGet("GetFile")]
        public byte[] GetFile([FromQuery]int userId, [FromQuery]int requestId, [FromQuery]string fileName)
        {
            var rootFolder = GetRootFolder();
            return RequestService.DownloadFile(requestId, fileName, rootFolder);
        }

        [HttpPost("add_file")]
        public async Task<IActionResult> AddFileToRequest([FromQuery]int userId, [FromQuery]int requestId, [FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest();
            var uploadFolder = Path.Combine(GetRootFolder(), requestId.ToString());
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
            RequestService.AttachFileToRequest(userId, requestId, file.FileName, fileName);
            return Ok();
        }
        private string GetRootFolder()
        {
            return "F:\\RequestAttachments\\";
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