using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using WebApi.Models;

namespace WebApi.Services
{
    public interface IAuthService
    {
        TokenDto GetToken(AuthDto authDto);
        TokenDto RefreshToken(Guid refreshToken);
    }
    public class AuthService: IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IConfiguration configuration, ILogger<AuthService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public TokenDto GetToken(AuthDto authDto)
        {
                var user = RequestService.WebLogin(authDto.Login, authDto.Password);
                if (user == null)
                {
                    return null;
                }
                var now = DateTime.UtcNow;
            var refreshToken = CreateRefreshToken(user, now);
            RequestService.AddRefreshToken(user.WorkerId,refreshToken, now.Add(TimeSpan.FromDays(_configuration.GetValue<int>("Auth:RefreshExpireDays"))));
                return new TokenDto
                {
                    Access = CreateAccessToken(user, now),
                    Refresh = refreshToken
                };
        }

        public TokenDto RefreshToken(Guid refreshToken)
        {
            var now = DateTime.UtcNow;

            var newExpireDate = now.Add(TimeSpan.FromDays(_configuration.GetValue<int>("Auth:RefreshExpireDays")));
            var user = RequestService.FindUserByToken(refreshToken, newExpireDate);
                if (user == null)
                {
                    return null;
                }
                 return new TokenDto
                {
                    Access = CreateAccessToken(user, now),
                    Refresh = refreshToken
                };
        }


        private string CreateAccessToken(WebUserDto user, DateTime start)
        {
            //todo: добавить роли
            var claims = new List<Claim>
            {
                new Claim("UserId", user.UserId.ToString()),
                new Claim("Login", user.Login??""),
                new Claim("SurName", user.SurName??""),
                new Claim("FirstName", user.FirstName??""),
                new Claim("PatrName", user.PatrName??""),
                new Claim("CanCreateRequestInWeb", user.CanCreateRequestInWeb.ToString()),
                new Claim("AllowStatistics", user.AllowStatistics.ToString()),
                new Claim("AllowCalendar", user.AllowCalendar.ToString()),
                new Claim("OnlyImmediate", user.OnlyImmediate.ToString()),
                new Claim("CanSetRating", user.CanSetRating.ToString()),
                new Claim("CanCloseRequest", user.CanCloseRequest.ToString()),
                new Claim("CanChangeStatus", user.CanChangeStatus.ToString()),
                new Claim("CanChangeImmediate", user.CanChangeImmediate.ToString()),
                new Claim("CanChangeChargeable", user.CanChangeChargeable.ToString()),
                new Claim("CanChangeAddress", user.CanChangeAddress.ToString()),
                new Claim("CanChangeServiceType", user.CanChangeServiceType.ToString()),
                new Claim("CanChangeExecuteDate", user.CanChangeExecuteDate.ToString()),
                new Claim("WorkerId", user.WorkerId.ToString()),
                new Claim("CanChangeExecutors", user.CanChangeExecutors.ToString()),
                new Claim("ServiceCompanyFilter", user.ServiceCompanyFilter.ToString()),
                new Claim("EnableAdminPage", user.EnableAdminPage.ToString()),
                new Claim("PushId", user.PushId),
            };
            var expires = start.Add(TimeSpan.FromMinutes(_configuration.GetValue<int>("Auth:AccessExpireMinutes")));
            var jwt = new JwtSecurityToken(
                issuer: _configuration["Auth:Issuer"],
                notBefore: start,
                claims: claims,
                expires: start.Add(TimeSpan.FromMinutes(_configuration.GetValue<int>("Auth:AccessExpireMinutes"))),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.Default.GetBytes(_configuration["Auth:Key"])), SecurityAlgorithms.HmacSha256));
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
        
        private Guid CreateRefreshToken(WebUserDto user, DateTime start)
        {
            var userToken = new UserToken
            {
                Token = Guid.NewGuid(),
                User = user
            };
            //UpdateRefreshTokenAttributes(userToken, start);
            return userToken.Token;
        }

        private static string ToHex(byte value)
        {
            var result = $"{value:X}";
            while (result.Length < 2)
            {
                result = "0" + result;
            }
            return result;
        }
    }
}
