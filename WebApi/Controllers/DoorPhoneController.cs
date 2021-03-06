﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.Extensions.Logging;
using WebApi.Models;
using WebApi.Models.Parameters;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("[controller]")]
    public class DoorPhoneController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<DoorPhoneController> _logger;

        public DoorPhoneController(IAuthService authService, ILogger<DoorPhoneController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost, AllowAnonymous]
        public ActionResult BindDoorPhone([FromBody] DoorPhone doorDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != "a921d6c2-8162-4912-a8b5-ab36b4bbf020")
            {
                return BadRequest("Authorization error!");
            }
            RequestService.BindDoorPhone(doorDto.Phone, doorDto.DoorUid, doorDto.DeviceId, doorDto.AddressId);
            return Ok();
        }
        [HttpGet, AllowAnonymous]
        public ActionResult<bool> GetBindDoorPhone([FromQuery] string phone, [FromQuery] string doorUid,[FromQuery] int addressId, [FromQuery] string deviceId)
        {
                var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value == "a921d6c2-8162-4912-a8b5-ab36b4bbf020")
            {
                return RequestService.GetDoorPhone(phone, doorUid, addressId, deviceId);
            }
            return BadRequest("Authorization error!");
        }

        [HttpGet("exists_sip"), AllowAnonymous]
        public ActionResult<bool> ExistsSipDoorPhone([FromQuery] int addressId)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value == "a921d6c2-8162-4912-a8b5-ab36b4bbf020")
            {
                return RequestService.ExistsSipPhone(addressId);
            }
            return BadRequest("Authorization error!");
        }

        [HttpGet("get_sip_phones"), AllowAnonymous]
        public ActionResult<string> GetSipByFlat([FromQuery] string account)
        {
            _logger.LogInformation($"---------- get_sip_phones({account})");
            var result = RequestService.VoIpPush(account);
            return Ok(result);
        }

        [HttpGet("bindedPushId"), AllowAnonymous]
        public ActionResult<PushIdsAndAddressDto[]> GetBindDoorPushIds([FromQuery] string flat, [FromQuery] string doorUid)
        {
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value == "a921d6c2-8162-4912-a8b5-ab36b4bbf020")
            {
                return RequestService.GetBindDoorPushIds(flat, doorUid);
            }
            return BadRequest("Authorization error!");
        }
        [HttpPost("bindDoorPhoneToHouse"), AllowAnonymous]
        public ActionResult BindDoorToHouse([FromBody] BindDoorPhone doorDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value != "a921d6c2-8162-4912-a8b5-ab36b4bbf020")
            {
                return BadRequest("Authorization error!");
            }
            RequestService.BindDoorPhoneToHouse(doorDto.HouseId, doorDto.DoorUid,doorDto.DoorNumber, doorDto.FromFlat, doorDto.ToFlat);
            return Ok();
        }

    }
}