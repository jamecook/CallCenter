﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using WebApi.Models;

namespace WebApi.Services
{
    public interface IClientAuthService
    {
        TokenDto GetToken(ClientAuthDto authDto);
        TokenDto RefreshToken(Guid refreshToken);
        void ValidatePhone(string phone);
        string ValidTest(string phone);
    }

    public class ClientAuthService : IClientAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public ClientAuthService(IConfiguration configuration, ILogger<AuthService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public TokenDto GetToken(ClientAuthDto authDto)
        {
            var user = RequestService.ClientLogin(authDto.Phone, authDto.Code, authDto.DeviceId);
            if (user == null)
            {
                return null;
            }
            var now = DateTime.UtcNow;
            var refreshToken = CreateRefreshToken(user, now);
            RequestService.AddClientRefreshToken(user.Id,refreshToken, now.Add(TimeSpan.FromDays(_configuration.GetValue<int>("Auth:RefreshExpireDays"))),user.DeviceId);
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
            var user = RequestService.ClientFindByToken(refreshToken, newExpireDate);
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

        public void ValidatePhone(string phone)
        {
            RequestService.ClientValidatePhone(phone);
        }
        public string ValidTest(string phone)
        {
            return RequestService.ClientValidTest(phone);
        }

        private string CreateAccessToken(ClientUserDto user, DateTime start)
        {
            //todo: добавить роли
            var claims = new List<Claim>
            {
                new Claim("ClientId", user.Id.ToString()),
                new Claim("Phone", user.Phone),
                new Claim("Name", user.Name??""),
                new Claim("PushId", user.PushId),
                new Claim("DeviceId", user.DeviceId),
                //new Claim("icom", "573b5bde.eyJhbGciOiJIUzI1NiJ9.eyJlbnRpdHlfdXVpZCI6IjI1MiIsImVudGl0eV90eXBlIjoidXNlciIsImVudGl0eV9yZWZfdXVpZCI6ImQ0ODVlZDExLWUxMGEtNDU5Yy04YjcyLTI5NWZlNmY4MWUzZCIsImVudGl0eV9uYW1lIjpudWxsLCJzYWx0IjoiNTczYjViZGUifQ.guurFCBoX4p-lVyXrEGWlJ7QbPCYJP1kbIKmzuGjOWk"),

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
        
        private Guid CreateRefreshToken(ClientUserDto user, DateTime start)
        {
            var userToken = new ClientToken
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