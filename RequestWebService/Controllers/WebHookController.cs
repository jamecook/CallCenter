using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using RequestWebService.Dto;
using RequestWebService.Services;

namespace RequestWebService.Controllers
{
    [Route("[controller]")]
    public class WebHookController : Controller
    {
        public void Log(string method)
        {
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress.ToString();
            var bufLen = (int) HttpContext.Request.Body.Length;
            var buf = new byte[bufLen];
            HttpContext.Request.Body.Position = 0;
            HttpContext.Request.Body.Read(buf, 0, bufLen);
            var str = Encoding.UTF8.GetString(buf, 0, bufLen);
            RequestService.LogOperation(remoteIpAddress, method, str);

        }

        [HttpPost("register_request")]
        public DefaultResult AddWebHookForRequest([FromBody] string url)
        {
            try
            {
                Log("AddWebHook");
                return
                    new DefaultResult {ResultCode = 0};
            }
            catch (Exception ex)
            {
                return
                    new DefaultResult {ResultCode = -1, ResultDescription = ex.ToString()};
            }
        }[HttpPost("register_user")]
        public DefaultResult AddWebHookForUser([FromBody] string url)
        {
            try
            {
                Log("AddWebHook");
                return
                    new DefaultResult {ResultCode = 0};
            }
            catch (Exception ex)
            {
                return
                    new DefaultResult {ResultCode = -1, ResultDescription = ex.ToString()};
            }
        }
    }
}