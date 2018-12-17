using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebApi.Models;
using WebApi.Models.Parameters;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("[controller]")]
    public class TermController : Controller
    {
        public IConfiguration Configuration { get; }
        private readonly ILogger<RequestController> _logger;

        public TermController(IConfiguration configuration, ILogger<RequestController> logger)
        {
            Configuration = configuration;
            _logger = logger;
        }

        [HttpPost, AllowAnonymous]
        public ActionResult SaveTerm([FromBody]TermParameter parameter)
        {
            _logger.LogInformation($"{0} {1} {2}",parameter.Temperature,parameter.Pressure, parameter.Humidity);
            return Ok();
        }
    }
}