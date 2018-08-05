using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using RequestWebService.Dto;

namespace RequestWebService.Controllers
{
    [Route("[controller]")]
    public class RequestController : Controller
    {
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
        public IEnumerable<RequestDto> Get([FromQuery]string token, [FromQuery]string number)
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
        public DefaultResult Post([FromBody]CreateRequestDto request)
        {
            return 
                new DefaultResult { ResultCode = 0,};
        }
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]RequestDto value)
        {
        }
    }
}