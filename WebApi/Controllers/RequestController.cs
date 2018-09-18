using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    public class CommaDelimitedArrayModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var key = bindingContext.ModelName;
            var val = bindingContext.ValueProvider.GetValue(key);
            if (val != null)
            {
                var s = val.Values.ToString();
                if (!string.IsNullOrEmpty(s))
                {
                    var elementType = bindingContext.ModelType.GetElementType();
                    var converter = TypeDescriptor.GetConverter(elementType);
                    var values = Array.ConvertAll(s.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries),
                        x => { return converter.ConvertFromString(x != null ? x.Trim() : x); });

                    var typedValues = Array.CreateInstance(elementType, values.Length);

                    values.CopyTo(typedValues, 0);

                    //bindingContext.Model = typedValues;
                    bindingContext.Result = ModelBindingResult.Success(typedValues);
                }
                else
                {
                    // change this line to null if you prefer nulls to empty arrays 
                    bindingContext.Model = Array.CreateInstance(bindingContext.ModelType.GetElementType(), 0);
                }
            }
            return Task.CompletedTask;
        }
    }

    [Route("[controller]")]
    public class RequestController : Controller
    {
        public IConfiguration Configuration { get; }
        public RequestController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        [HttpGet]
        public IEnumerable<RequestForListDto> Get([FromQuery]string requestId,[FromQuery] bool? filterByCreateDate,
            [FromQuery] DateTime? fromDate,[FromQuery] DateTime? toDate,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] streets,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] houses,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] addresses,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] parentServices,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] services,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] statuses,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] workers,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] executors,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] ratings,
            [FromQuery] bool? badWork,
            [FromQuery] bool? garanty,
            [FromQuery] string clientPhone)

        {
            //var ttt = User.Claims.ToArray();
            //var login = User.Claims.FirstOrDefault(c => c.Type == "Login")?.Value;
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            int? rId=null;
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

            return RequestService.WebRequestListArrayParam(workerId, rId,
                filterByCreateDate ?? true,
                fromDate ?? DateTime.Today,
                toDate ?? DateTime.Today.AddDays(1),
                fromDate ?? DateTime.Today,
                toDate ?? DateTime.Today.AddDays(1),
                streets, houses, addresses, parentServices, services, statuses, workers, executors, ratings,
                badWork ?? false,
                garanty.HasValue && garanty.Value, clientPhone);
        }

        [HttpGet("workers")]
        public IEnumerable<WorkerDto> GetWorkers()
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            return RequestService.GetWorkersByWorkerId(workerId);
        }
        [HttpGet("statuses")]
        public IEnumerable<StatusDto> GetStatuses()
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            return RequestService.GetStatusesAllowedInWeb(workerId);
        }
        [HttpGet("streets")]
        public IEnumerable<StreetDto> GetStreets()
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            return RequestService.GetStreetsByWorkerId(workerId);
        }
        [HttpGet("houses/{id}")]
        public IEnumerable<WebHouseDto> GetHouses(int id)
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            return RequestService.GetHousesByStreetAndWorkerId(id, workerId);
        }
        [HttpGet("parent_services")]
        public IEnumerable<ServiceDto> GetParrentServices()
        {
            return RequestService.GetParentServices();
        }
        [HttpGet("services")]
        public IEnumerable<ServiceDto> GetServices([ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] parentIds)
        {
            return RequestService.GetServices(parentIds);
        }
        [HttpGet("request_records/{id}")]
        public IEnumerable<WebCallsDto> GetRequestRecords(int id)
        {
            return RequestService.GetWebCallsByRequestId(id);
        }
        [HttpGet("record/{id}")]
        public byte[] GetRecord(int id)
        {
            return RequestService.GetRecordById(id);
        }
        [HttpGet("request_attachments/{id}")]
        public IEnumerable<AttachmentDto> GetAttachments(int id)
        {
            return RequestService.GetAttachments(id);
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
        [HttpGet("request_notes/{id}")]
        public IEnumerable<NoteDto> GetNotes(int id)
        {
            return RequestService.GetNotes(id);
        }

        [HttpPut("status/{id}")]
        public void SetStatus(int id,[FromBody]int statusId)
        {
            var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (int.TryParse(userIdStr, out int userId))
            {
                RequestService.AddNewState(id,statusId, userId);
            }
        }
        private string GetRootFolder()
        {
            return Configuration.GetValue<string>("Settings:RootFolder");
        }
    }
}