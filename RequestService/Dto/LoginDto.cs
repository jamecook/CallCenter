using System;

namespace RequestServiceImpl.Dto
{
    public class LoginDto
    {
        public WebUserDto UserInfo { get; set; }
        public Guid Token { get; set; }

    }
}