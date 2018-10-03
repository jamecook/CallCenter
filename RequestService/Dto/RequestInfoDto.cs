using System;

namespace RequestServiceImpl.Dto
{
    public class RequestInfoDto
    {
        public int Id { get; set; }
        public DateTime CreateTime { get; set; }
        public AddressDto Address { get; set; }
        public string Description { get; set; }
        public string Entrance { get; set; }
        public string Floor { get; set; }
        public RequestTypeDto Type { get; set; }
        public RequestStateDto State { get; set; }
        public RequestUserDto CreateUser { get; set; }
        public ContactDto[] Contacts { get; set; }
        public DateTime? ExecuteDate { get; set; }
        public int? PeriodId { get; set; }
        public int? MasterId { get; set; }
        public int? ExecuterId { get; set; }
        public int? ServiceCompanyId { get; set; }
        public bool IsImmediate { get; set; }
        public bool IsChargeable { get; set; }
        public bool IsBadWork { get; set; }
        public bool IsRetry { get; set; }
        public RequestRatingDto Rating { get; set; }
        public DateTime? FromTime { get; set; }
        public DateTime? ToTime { get; set; }
        public DateTime? AlertTime { get; set; }
        public DateTime? TermOfExecution { get; set; }
        public int GarantyId { get; set; }
        public EquipmentDto Equipment { get; set; }

    }
}