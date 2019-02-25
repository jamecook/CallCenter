using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebApi.Models;
using WebApi.Models.Parameters;
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
            [FromQuery] bool? badWork,
            [FromQuery] bool? garanty,
            [FromQuery] bool? onlyRetry,
            [FromQuery] bool? chargeable,
            [FromQuery] string clientPhone,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] warranties,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] immediates,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] regions
            )

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
                streets, houses, addresses, parentServices, services, statuses, workers, executors, ratings,companies,warranties, immediates, regions,
                badWork ?? false,
                garanty ?? false, onlyRetry ?? false,chargeable ?? false, clientPhone);
        }

        [HttpGet("get_tasks")]
        public IEnumerable<ScheduleTaskDto> GetTasks([FromQuery]int? workerId,
            [FromQuery] DateTime fromDate,[FromQuery] DateTime toDate)
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int currentWorkerId);
            return RequestService.GetScheduleTask(currentWorkerId,workerId,fromDate,toDate);
        }
        [HttpGet("get_all_tasks")]
        public IEnumerable<ScheduleTaskDto> GetAllTasks([FromQuery]int workerId,
            [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int currentWorkerId);
            return RequestService.GetAllScheduleTask(currentWorkerId, workerId, fromDate, toDate);
        }

        [HttpPost("add_task")]
        public ActionResult<string> AddTask([FromBody]AddTaskDto taskDto)
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int currentWorkerId);
            return RequestService.AddScheduleTask(currentWorkerId, taskDto.WorkerId, taskDto.RequestId, taskDto.FromDate, taskDto.ToDate, null);
        }

        [HttpPut("edit_task/{id}")]
        public ActionResult UpdateTask(int id, [FromBody]AddTaskDto taskDto)
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int currentWorkerId);
            RequestService.UpdateScheduleTask(currentWorkerId, id, taskDto.WorkerId, taskDto.RequestId, taskDto.FromDate, taskDto.ToDate, null);
            return Ok();
        }
        [HttpDelete("drop_task/{id}")]
        public ActionResult DropTask(int id)
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int currentWorkerId);
            RequestService.DeleteScheduleTask(currentWorkerId, id);
            return Ok();
        }

        [HttpGet("get_pdf")]
        public byte[] GetPdf([ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] requestIds)

        {
            //var ttt = User.Claims.ToArray();
            //var login = User.Claims.FirstOrDefault(c => c.Type == "Login")?.Value;
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            return RequestService.GetRequestActs(workerId, requestIds);
        }

        [HttpGet("get_excel")]
        public byte[] GetExcel([ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] requestIds)

        {
            //var ttt = User.Claims.ToArray();
            //var login = User.Claims.FirstOrDefault(c => c.Type == "Login")?.Value;
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            return RequestService.GetRequestExcel(workerId, requestIds);
        }

        [HttpPost]
        public string Post([FromBody]CreateRequestDto value)
        {
            _logger.LogDebug("Create Request: "+JsonConvert.SerializeObject(value));
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            return RequestService.CreateRequest(workerId, value.Phone, value.Name, value.AddressId, value.TypeId, value.MasterId, value.ExecuterId, value.Description,value.IsChargeable ?? false, value.ExecuteDate, value.WarrantyId ?? 0);
        }

        [HttpGet("workers")]
        public IEnumerable<WorkerDto> GetWorkers()
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            return RequestService.GetWorkersByWorkerId(workerId);
        }

        [HttpGet("executers")]
        public IEnumerable<WorkerDto> GetExecuters()
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            return RequestService.GetExecutersByWorkerId(workerId);
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
        [HttpGet("address_ids")]
        public IEnumerable<int> GetAddressIds()
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            return RequestService.GetAddressesId(workerId);
        }
        [HttpGet("house_ids")]
        public IEnumerable<int> GetHouseIds()
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            return RequestService.GetHousesId(workerId);
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

        [HttpGet("request_notes/{id}")]
        public IEnumerable<NoteDto> GetNotes(int id)
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            if (int.TryParse(workerIdStr, out int workerId))
            {
                return RequestService.GetNotes(workerId, id);
            }
            return null;
        }
        [HttpGet("city_regions")]
        public IEnumerable<CityRegionDto> GetCityRegions()
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            if (int.TryParse(workerIdStr, out int workerId))
            {
                return RequestService.GetRegions(workerId);
            }
            return null;
        }

        [HttpPut("status/{id}")]
        public IActionResult SetStatus(int id, [FromBody]int statusId)
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            if(int.TryParse(workerIdStr, out int workerId))
            {
                RequestService.AddNewState(id,statusId, workerId);
                return Ok();
            }
            return BadRequest();
        }
        [HttpPut("set_service/{id}")]
        public IActionResult SetService(int id, [FromBody]int serviceId)
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            if(int.TryParse(workerIdStr, out int workerId))
            {
                RequestService.SetNewService(id, serviceId, workerId);
                return Ok();
            }
            return BadRequest();
        }
        [HttpPut("set_master/{id}")]
        public IActionResult SetMaster(int id, [FromBody]int masterId)
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            if(int.TryParse(workerIdStr, out int workerId))
            {
                RequestService.SetNewMaster(id, masterId, workerId);
                return Ok();
            }
            return BadRequest();
        }
        [HttpPut("set_executer/{id}")]
        public IActionResult SetExecuter(int id, [FromBody]int executerId)
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            if(int.TryParse(workerIdStr, out int workerId))
            {
                RequestService.SetNewExecuter(id, executerId, workerId);
                return Ok();
            }
            return BadRequest();
        }
        [HttpPost("add_note/{id}")]
        public IActionResult AddNote(int id, [FromBody]string note)
        {
            if (string.IsNullOrEmpty(note))
                return BadRequest();
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            if(int.TryParse(workerIdStr, out int workerId))
            {
                RequestService.AddNewNote(id, note, workerId);
            }
            return Ok();
        }
        [HttpPut("set_rating/{id}")]
        public IActionResult SetRating(int id,[FromBody]int ratingId, [FromBody]string description)
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            if (int.TryParse(workerIdStr, out int workerId))
            {
                RequestService.SetRating(workerId, id, ratingId, description);
                return Ok();
            }
            return BadRequest();
        }

        [HttpPut("set_execute_date/{id}")]
        public IActionResult SetExecuteDate(int id,[FromBody]SetExecuteDateParams param)
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            if (int.TryParse(workerIdStr, out int workerId))
            {
                RequestService.SetExecuteDate(workerId, id, param.ExecuteDate, param.Note);
                return Ok();
            }
            return BadRequest();
        }

        //[HttpPut("set_garanty_state/{id}")]
        //public IActionResult SetGarantyState(int id, [FromForm] IFormFile file, [FromForm] int type, [FromForm] int newState,
        //    [FromForm] string name, [FromForm] DateTime docDate)
        //{
        //    if (id == 0 || file == null || file.Length == 0 || type == 0)
        //    {
        //        return BadRequest();
        //    }
        //    var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
        //    if (int.TryParse(workerIdStr, out int workerId))
        //    {
        //        var uploadFolder = Path.Combine(GetRootFolder(), id.ToString());
        //        if (!Directory.Exists(uploadFolder))
        //        {
        //            Directory.CreateDirectory(uploadFolder);
        //        }
        //        var fileExtension = Path.GetExtension(file.FileName);
        //        var fileName = Guid.NewGuid() + fileExtension;
        //        using (var fileStream = new FileStream(Path.Combine(uploadFolder, fileName), FileMode.Create))
        //        {
        //            file.CopyTo(fileStream);
        //        }
        //        RequestService.SetGarantyState(id, newState, type, name, docDate, fileName, workerId);
        //        return Ok();
        //    }
        //    return BadRequest();
        //}
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