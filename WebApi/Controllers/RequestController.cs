using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
    [Produces("application/json")]
    [Consumes("application/json","multipart/form-data")]
    public class RequestController : Controller
    {
        public IConfiguration Configuration { get; }
        private readonly ILogger<RequestController> _logger;
        public RequestController(IConfiguration configuration, ILogger<RequestController> logger)
        {
            Configuration = configuration;
            _logger = logger;
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
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] companies,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]string[] flats,
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
                streets, houses, addresses, parentServices, services, statuses, workers, executors, ratings,companies,flats,
                badWork ?? false,
                garanty.HasValue && garanty.Value, clientPhone);
        }

        [HttpPost]
        public string Post([FromBody]CreateRequestDto value)
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            return RequestService.CreateRequest(workerId, value.Phone, value.Name, value.AddressId, value.TypeId, value.MasterId, value.ExecuterId, value.Description);
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
            return RequestService.GetStatusesAll(workerId);
        }
        [HttpGet("statuses_for_set")]
        public IEnumerable<StatusDto> GetStatusesForSet()
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
       [HttpGet("companies")]
        public IEnumerable<ServiceCompanyDto> GetCompanies()
        {
            return RequestService.GetServicesCompanies();
        }
        [HttpGet("request_records/{id}")]
        public IEnumerable<WebCallsDto> GetRequestRecords(int id)
        {
            return RequestService.GetWebCallsByRequestId(id);
        }
        [HttpGet("house_flats/{id}")]
        public IEnumerable<FlatDto> GetHouseFlats(int id)
        {
            return RequestService.GetFlats(id);
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

        [HttpPost("add_file/{id}")]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> AddFileToRequest(int id, IFormFile file)
        {
            var files = Request?.Form?.Files;
            if (file == null || file.Length == 0)
                return Content("file not selected");
            _logger.LogDebug($"FileLen: {file.Length}, Name: {file.Name}, FileName: {file.FileName}");
            var uploads = Path.Combine(GetRootFolder(), "uploads");

            if (file.Length > 0)
            {
                using (var fileStream = new FileStream(Path.Combine(uploads, file.FileName), FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }
            return Content("file uploaded");
        }

        [HttpGet("request_notes/{id}")]
        public IEnumerable<NoteDto> GetNotes(int id)
        {
            return RequestService.GetNotes(id);
        }

        [HttpPut("status/{id}")]
        public void SetStatus(int id, [FromBody]int statusId)
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

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class DisableFormValueModelBindingAttribute : Attribute, IResourceFilter
    {
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            var formValueProviderFactory = context.ValueProviderFactories
                    .OfType<FormValueProviderFactory>()
                    .FirstOrDefault();
            if (formValueProviderFactory != null)
            {
                context.ValueProviderFactories.Remove(formValueProviderFactory);
            }

            var jqueryFormValueProviderFactory = context.ValueProviderFactories
                .OfType<JQueryFormValueProviderFactory>()
                .FirstOrDefault();
            if (jqueryFormValueProviderFactory != null)
            {
                context.ValueProviderFactories.Remove(jqueryFormValueProviderFactory);
            }
        }

        public void OnResourceExecuted(ResourceExecutedContext context)
        {
        }
    }
}