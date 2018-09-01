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
    public class AuthController: Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost, AllowAnonymous]
        public ActionResult<TokenModel> Auth([FromBody]AuthParameter authParameter)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var authDto = new AuthDto {Login = authParameter.Login, Password = authParameter.Password};
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
