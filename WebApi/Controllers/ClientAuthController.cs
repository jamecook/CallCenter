using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Models.Parameters;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("[controller]")]
    public class ClientAuthController : Controller
    {
        private readonly IClientAuthService _authService;

        public ClientAuthController(IClientAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("validate"), AllowAnonymous]
        public ActionResult ValidatePhone([FromBody]ClientValidateParameter param)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            _authService.ValidatePhone(param.Phone);
            return Ok();
        }
        [HttpPost("valid_test"), AllowAnonymous]
        public ActionResult<string> ValidTest([FromBody]ClientValidateParameter param)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var code = _authService.ValidTest(param.Phone);
            return Ok(code);
        }

        [HttpPost, AllowAnonymous]
        public ActionResult<TokenModel> Auth([FromBody]ClientAuthParameter authParameter)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var authDto = new ClientAuthDto {Phone = authParameter.Phone, Code = authParameter.Code, DeviceId = authParameter.DeviceId};
            var token = _authService.GetToken(authDto);
            if (token == null)
            {
                return Unauthorized();
            }
            return new TokenModel{Access = token.Access,Refresh = token.Refresh.ToString()};
        }

        [HttpPost("refresh"), AllowAnonymous]
        public ActionResult<TokenModel> Refresh([FromBody, Required]Guid refreshToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var token = _authService.RefreshToken(refreshToken);
            if (token == null)
            {
                return BadRequest();
            }
            return new TokenModel { Access = token.Access, Refresh = token.Refresh.ToString() };
        }
    }
}