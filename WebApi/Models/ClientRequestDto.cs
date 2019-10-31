using System;

namespace WebApi.Models
{
    public class ClientRequestDto
    {
        public int AddressId { get; set; }
        public int TypeId { get; set; }
        public string Description { get; set; }
        public string Origin { get; set; }
    }
}