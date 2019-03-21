using System;

namespace WebApi.Models
{
    public class ClientToken
    {
        public virtual Guid Token { get; set; }
        public virtual DateTime ExpirationDate { get; set; }
        public virtual ClientUserDto User { get; set; }
    }
}