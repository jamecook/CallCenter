using System;

namespace CRMPhone.Dto
{
    public class RequestInfoDto
    {
        public int Id { get; set; }
        public DateTime CreateTime { get; set; }
        public AddressDto Address { get; set; }
        public string Description { get; set; }
        public RequestTypeDto Type { get; set; }
        public RequestStateDto State { get; set; }
        public RequestUserDto CreateUser { get; set; }
        public ContactDto[] Contacts { get; set; }
        public DateTime ExecuteDate { get; set; }
        public int ExecutorId { get; set; }
    }
}