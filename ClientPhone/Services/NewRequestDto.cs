using System;
using RequestServiceImpl.Dto;

namespace ClientPhone.Services
{
    public class NewRequestDto
    {
        public int UserId { get; set; }
        public string LastCallId { get; set; }
        public int AddressId { get; set; }
        public int RequestTypeId { get; set; }
        public ContactDto[] ContactList { get; set; }
        public string RequestMessage { get; set; }
        public bool Chargeable { get; set; }
        public bool Immediate { get; set; }
        public string CallUniqueId { get; set; }
        public string Entrance { get; set; }
        public string Floor { get; set; }
        public DateTime? AlertTime { get; set; }
        public bool IsRetry { get; set; }
        public bool IsBedWork { get; set; }
        public int? EquipmentId { get; set; }
        public int Warranty { get; set; }
    }
}