using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("[controller]")]
    [Produces("application/json")]
    [Consumes("application/json", "multipart/form-data")]
    public class AdminController : Controller
    {
        public IConfiguration Configuration { get; }
        private readonly ILogger<RequestController> _logger;
        private readonly string _amiHost;
        private readonly int _amiPort;

        public AdminController(IConfiguration configuration, ILogger<RequestController> logger)
        {
            Configuration = configuration;
            _logger = logger;
            _amiHost = Configuration.GetValue<string>("Settings:AmiHost");
            var amiPort = Configuration.GetValue<string>("Settings:AmiPort");
            if (!int.TryParse(amiPort, out _amiPort))
            {
                _amiPort = 5038;
            }
        }

        [HttpGet("get_registry")]
        public RegistryDto[] GetPegistry()
        {
            var enableAdminPage = User.Claims.FirstOrDefault(c => c.Type == "EnableAdminPage")?.Value;
            if (enableAdminPage != bool.TrueString)
                return null;
            using (var amiService = new AmiService(_amiHost, _amiPort))
            {
                var result = amiService.LoginAndGetRegistry("zerg", "asteriskzerg");
                return result;
            }
        }
    }
}