using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using WebApi.Models;
using WebApi.Models.Parameters;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("[controller]")]
    public class DoorPhoneController : Controller
    {
        private readonly IAuthService _authService;

        public DoorPhoneController(IAuthService authService)
        {
            _authService = authService;
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
            RequestService.BindDoorPhone(doorDto.Phone, doorDto.DoorUid);
            return Ok();
        }
        [HttpGet, AllowAnonymous]
        public ActionResult<bool> GetBindDoorPhone([FromQuery] string phone, [FromQuery] string doorUid)
        {
                var auth = Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            if (auth.Value == "a921d6c2-8162-4912-a8b5-ab36b4bbf020")
            {
                return RequestService.GetDoorPhone(phone, doorUid);
            }
            return BadRequest("Authorization error!");
        }
        [HttpGet("bindedPushId"), AllowAnonymous]
        public ActionResult<PushIdAndAddressDto[]> GetBindDoorPushIds([FromQuery] string flat, [FromQuery] string doorUid)
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