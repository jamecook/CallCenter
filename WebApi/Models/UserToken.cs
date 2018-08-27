using System;

namespace WebApi.Models
{
    public class UserToken
    {
        public virtual Guid Token { get; set; }
        public virtual DateTime ExpirationDate { get; set; }
        public virtual WebUserDto User { get; set; }
    }
}