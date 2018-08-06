using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using RequestWebService.Dto;
using RequestWebService.Services;

namespace RequestWebService.Controllers
{
    [Route("[controller]")]
    public class RequestController : Controller
    {
        public void Log(string method)
        {
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress.ToString();
            var bufLen = (int)HttpContext.Request.Body.Length;
            var buf = new byte[bufLen];
            HttpContext.Request.Body.Position = 0;
            HttpContext.Request.Body.Read(buf, 0, bufLen);
            var str = Encoding.UTF8.GetString(buf, 0, bufLen);
            RequestService.LogOperation(remoteIpAddress, method, str);

        }

        [HttpPost("add")]
        public DefaultResult AddRequest([FromBody]RequestDto request)
        {
            try
            {
                Log("AddRequest");
                var requestId = RequestService.AddRequest(request.BitrixId, request.CreaterPhone, request.CreateTime, request.StreetName, request.Building, request.Corpus, request.Flat, request.ServiceId, request.ServiceFullName, request.Description, request.Status, request.ExecuterName, request.ExecuteTime, request.Cost);
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
                RequestService.UpdateRequest(request.BitrixId, request.CreaterPhone, request.CreateTime, request.StreetName, request.Building, request.Corpus, request.Flat, request.ServiceId, request.ServiceFullName, request.Description, request.Status, request.ExecuterName, request.ExecuteTime, request.Cost);
                return
                    new DefaultResult { ResultCode = 0, ResultDescription = "Request Updated" };
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
            return RequestService.GetRequest(id);
        }
        /*
                [HttpGet("all")]//http://localhost:61174/request/all?token=qwe12312321qeweq-768tuj6570-ghji
                public IEnumerable<RequestDto> Get(string token)
                {
                    return new [] {new RequestDto()
                    {
                        Number = "M-1",
                        Description = "�������� ������",
                        Status = "�������",
                        Address = "������, ����� ����������� 8",
                        CreateTime = DateTime.Now,
                        Flat = "18",
                        ServiceFullName = "����������������->������ ��������",
                        ServiceId = 123,
                        ContainsPhotos = false,
                        Cost = 1000.24,
                        ExecuteTime = DateTime.Now.AddDays(1),
                        ServiceCompany = "\"������\"���",
                        ExecuterName = "�������� ����"

                    },
                        new RequestDto()
                    {
                        Number = "M-2",
                        Description = "�������� ������",
                        Status = "�������",
                        Address = "������, ����� 50 ��� ����� 83",
                        CreateTime = DateTime.Now,
                        Flat = "18",
                        ServiceFullName = "�����������, ��������->�����������->�� ��������",
                        ServiceId = 1123,
                        ContainsPhotos = false,
                        Cost = 100.24,
                        ExecuteTime = DateTime.Now.AddDays(1),
                        ServiceCompany = "\"������\"���",
                        ExecuterName = "���� ����������"

                    }};
                }
                [HttpGet("byId")]//http://localhost:61174/request/byId?token=qwe12312321qeweq-768tuj6570-ghji&number=M-43412412
                public IEnumerable<RequestDto> Get([FromQuery]string number)
                {
                    return new[] {new RequestDto()
                    {
                        Number = number,
                        Description = "�������� ������",
                        Status = "�������",
                        Address = "������, ����� 50 ��� ����� 83",
                        CreateTime = DateTime.Now,
                        Flat = "18",
                        ServiceFullName = "�����������, ��������->�����������->�� ��������",
                        ServiceId = 1123,
                        ContainsPhotos = false,
                        Cost = 100.24,
                        ExecuteTime = DateTime.Now.AddDays(1),
                        ServiceCompany = "\"������\"���",
                        ExecuterName = "���� ����������"

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