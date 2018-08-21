using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using RequestWebService.Dto;
using RequestWebService.Services;

namespace RequestWebService.Controllers
{
    [Route("[controller]")]
    public class UserController : Controller
    {
        public void Log(string method)
        {
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress.ToString();
            var str = string.Empty;
            var bufLen = HttpContext.Request.Method != "GET" ? (int?)HttpContext.Request.Body?.Length : null;
            if (bufLen != null)
            {
                var buf = new byte[bufLen.Value];
                HttpContext.Request.Body.Position = 0;
                HttpContext.Request.Body.Read(buf, 0, bufLen.Value);
                str = Encoding.UTF8.GetString(buf, 0, bufLen.Value);
            }
            RequestService.LogOperation(remoteIpAddress, method, str);

        }
        [HttpPost("add")]
        public DefaultResult AddUser([FromBody]UserDto userInfo)
        {
            try
            {
                Log("AddUser");
                var requestId = RequestService.AddUser(userInfo.BitrixId, userInfo.SurName, userInfo.FirstName, userInfo.PatrName, userInfo.Phone, userInfo.Email,
                        userInfo.Login, userInfo.Password, userInfo.DefaultServiceCompany, userInfo.IsMaster);
                return
                    new DefaultResult { ResultCode = 0, ResultDescription = requestId };
            }
            catch (Exception ex)
            {
                return
                    new DefaultResult { ResultCode = -1, ResultDescription = ex.ToString()};
            }
        }

        [HttpPut("{id}")]
        public DefaultResult Put(string id, [FromBody]UserDto userInfo)
        {
            try
            {
                Log("UpdateUser");
                RequestService.UpdateUser(id, userInfo.SurName, userInfo.FirstName, userInfo.PatrName, userInfo.Phone, userInfo.Email,
                        userInfo.Login, userInfo.Password, userInfo.DefaultServiceCompany, userInfo.IsMaster);
                return
                    new DefaultResult { ResultCode = 0, ResultDescription = "User Updated" };
            }
            catch (Exception ex)
            {
                return
                    new DefaultResult { ResultCode = -1, ResultDescription = ex.ToString() };
            }
        }

        [HttpGet("{id}")]

        public UserDto Get(string id)
        {
            Log($"GetUser. {HttpContext.Request.Path}");
            return RequestService.GetUser(id);
        }
        [HttpGet]
        public UserDto[] Get()
        {
            Log("GetAllUsers");
            return RequestService.GetAllUsers();
        }

        /*
        [HttpPost("login")]
        public LoginResult Login([FromBody]LoginDto value)
        {
            var connectionString = string.Format("server={0};uid={1};pwd={2};database={3};charset=utf8", "192.168.1.130", "dispex", "mysqlfispex", "asterisk");
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("select 1 id;", conn))
                {
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var t = dataReader.GetInt32("id");
                        }
                    }
                }
                conn.Close();
            }

            var token = Guid.NewGuid().ToString();
            return value.Login == "test" ? 
                new LoginResult { ResultCode = 0,Token = token, UserName = "Testoviy Test Testovish"} : 
                new LoginResult { ResultCode = 1, ResultDescription = "Access Denied." };
        }

        [HttpPost("logout")]
        public DefaultResult Logout([FromBody]string token)
        {
            return
                new DefaultResult { ResultCode = 0, };
        }

        [HttpPost("register")]
        public DefaultResult Register([FromBody]UserDto userInfo)
        {
            return
                new DefaultResult { ResultCode = 0, };
        }
        */
    }
}