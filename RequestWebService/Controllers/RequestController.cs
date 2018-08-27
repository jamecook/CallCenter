using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RequestWebService.Dto;
using RequestWebService.Services;

namespace RequestWebService.Controllers
{
    [Route("[controller]")]
    public class RequestController : Controller
    {
        private readonly IHostingEnvironment _environment;

        public RequestController(IHostingEnvironment environment)
        {
            _environment = environment;//?? throw new ArgumentNullException(nameof(environment));
        }
        public void Log(string method)
        {
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress.ToString();
            var str = string.Empty;
            var bufLen = HttpContext.Request.Method!="GET"?(int?)HttpContext.Request.Body?.Length:null;
            if (bufLen != null)
            {
                var buf = new byte[bufLen.Value];
                HttpContext.Request.Body.Position = 0;
                HttpContext.Request.Body.Read(buf, 0, bufLen.Value);
                str = Encoding.UTF8.GetString(buf, 0, bufLen.Value);
            }
            RequestService.LogOperation(remoteIpAddress, method, str);

        }
        [HttpPost("uploadImage/{id}")]
        public async Task<IActionResult> Post(string id, [FromBody]IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Content("file not selected");
            var uploads = Path.Combine(_environment.WebRootPath, "uploads");

            return Content("file uploaded");
            if (file.Length > 0)
            {
                using (var fileStream = new FileStream(Path.Combine(uploads, file.FileName), FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }
        }

        [HttpPost("add")]
        public DefaultResult AddRequest([FromBody]RequestDto request)
        {
            try
            {
                Log("AddRequest");
                var requestId = RequestService.AddRequest(request.BitrixId, request.CreaterPhone, request.CreateTime, request.StreetName, request.Building, request.Corpus, request.Flat, request.ServiceId, request.ServiceFullName, request.Description, request.Status, request.ExecuterName, request.ServiceCompany, request.ExecuteTime, request.Cost);
                return
                    new DefaultResult { ResultCode = 0, ResultDescription = requestId };
            }
            catch (Exception ex)
            {
                return
                    new DefaultResult { ResultCode = -1, ResultDescription = ex.ToString() };
            }
        }
        [HttpPut("{id}")]
        public DefaultResult Put(string bitrixId, [FromBody]RequestDto request)
        {
            try
            {
                Log("UpdateRequest");
                var affectedRows = RequestService.UpdateRequest(request.BitrixId, request.CreaterPhone, request.CreateTime, request.StreetName, request.Building, request.Corpus, request.Flat, request.ServiceId, request.ServiceFullName, request.Description, request.Status, request.ExecuterName,request.ServiceCompany, request.ExecuteTime, request.Cost, request.Hash);
                return
                    new DefaultResult { ResultCode = affectedRows, ResultDescription = affectedRows==1?"Request Updated":"Hash was changed" };
            }
            catch (Exception ex)
            {
                return
                    new DefaultResult { ResultCode = -1, ResultDescription = ex.ToString() };
            }

        }

        [HttpGet("{id}")]
        public RequestDto Get(string id)
        {
            Log($"GetRequest. {HttpContext.Request.Path}");
            return RequestService.GetRequest(id);
        }

        [HttpGet]
        public RequestDto[] Get()
        {
            Log("GetAllRequests");
            return RequestService.GetAllRequests();
        }
        /*
                [HttpGet("all")]//http://localhost:61174/request/all?token=qwe12312321qeweq-768tuj6570-ghji
                public IEnumerable<RequestDto> Get(string token)
                {
                    return new [] {new RequestDto()
                    {
                        Number = "M-1",
                        Description = "Тестовая заявка",
                        Status = "Открыта",
                        Address = "Тюмень, улица Мельникайте 8",
                        CreateTime = DateTime.Now,
                        Flat = "18",
                        ServiceFullName = "Электроснабжение->Замена автомата",
                        ServiceId = 123,
                        ContainsPhotos = false,
                        Cost = 1000.24,
                        ExecuteTime = DateTime.Now.AddDays(1),
                        ServiceCompany = "\"Домово\"ООО",
                        ExecuterName = "Электрик Петя"

                    },
                        new RequestDto()
                    {
                        Number = "M-2",
                        Description = "Тестовая заявка",
                        Status = "Открыта",
                        Address = "Тюмень, улица 50 лет ВЛКСМ 83",
                        CreateTime = DateTime.Now,
                        Flat = "18",
                        ServiceFullName = "Телевидение, Интернет->Телевидение->Не работает",
                        ServiceId = 1123,
                        ContainsPhotos = false,
                        Cost = 100.24,
                        ExecuteTime = DateTime.Now.AddDays(1),
                        ServiceCompany = "\"Домово\"ООО",
                        ExecuterName = "Иван Васильевич"

                    }};
                }
                [HttpGet("byId")]//http://localhost:61174/request/byId?token=qwe12312321qeweq-768tuj6570-ghji&number=M-43412412
                public IEnumerable<RequestDto> Get([FromQuery]string number)
                {
                    return new[] {new RequestDto()
                    {
                        Number = number,
                        Description = "Тестовая заявка",
                        Status = "Открыта",
                        Address = "Тюмень, улица 50 лет ВЛКСМ 83",
                        CreateTime = DateTime.Now,
                        Flat = "18",
                        ServiceFullName = "Телевидение, Интернет->Телевидение->Не работает",
                        ServiceId = 1123,
                        ContainsPhotos = false,
                        Cost = 100.24,
                        ExecuteTime = DateTime.Now.AddDays(1),
                        ServiceCompany = "\"Домово\"ООО",
                        ExecuterName = "Иван Васильевич"

                    }};
                }

                [HttpPost]
                public DefaultResult Post([FromBody]RequestDto request)
                {
                    return 
                        new DefaultResult { ResultCode = 0,};
                }
                */
    }
}