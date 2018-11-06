using System;

namespace WebApi.Models
{
    public class CreateRequestDto
    {
        public string Phone { get; set; }
        public string Name { get; set; }
        public int AddressId { get; set; }
        public int TypeId { get; set; }
        public int? MasterId { get; set; }
        public int? ExecuterId { get; set; }
        public bool? IsChargeable { get; set; }
        public string Description { get; set; }
        public DateTime? ExecuteDate { get; set; }
    }
}