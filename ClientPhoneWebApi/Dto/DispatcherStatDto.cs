using System;
using System.ComponentModel;

namespace ClientPhoneWebApi.Dto
{
    public class DispatcherStatDto
    {
        public int Id { get; set; }
        //public int? ServiceCompanyId { get; set; }
        //public string ServiceCompanyName { get; set; }
        public string SurName { get; set; }
        public string IpAddress { get; set; }
        public string FirstName { get; set; }
        public string PatrName { get; set; }
        public string SipNumber { get; set; }

        public string Version { get; set; }

        public bool? OnLine { get; set; }

        public string OnLineText => OnLine.HasValue ? OnLine.Value ? "מםכאים" : "מפפכאים" : "";
        public string Direction { get; set; }

        public string PhoneNumber { get; set; }

        public string UniqueId { get; set; }

        public DateTime AliveTime { get; set; }

        public int? TalkTime { get; set; }

        public int? WaitingTime { get; set; }

        public string FullName => $"{SurName} {FirstName} {PatrName}".TrimEnd();

    }
}