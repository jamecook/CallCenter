using System;
using Microsoft.SqlServer.Server;

namespace RequestWebService.Dto
{
    public class RequestDto
    {
        public int? Id { get; set; }
        public string CreaterPhone { get; set; }
        public string BitrixId { get; set; }
        public DateTime CreateTime { get; set; }
        //public string Address { get; set; }
        public string StreetName { get; set; }
        public string Building { get; set; }
        public string Corpus { get; set; }
        public string Flat { get; set; }
        public string ServiceId { get; set; }
        public string ServiceFullName { get; set; }
        //public string ServiceCompany { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string ExecuterName { get; set; }
        public string ServiceCompany { get; set; }
        public DateTime? ExecuteTime { get; set; }
        public double? Cost { get; set; }
        public bool ContainsPhotos { get; set; }


        //public string FullAddress
        //{
        //    //get { return string.IsNullOrEmpty(Corpus)?$"{StreetPrefix} {StreetName}, {Building}, {AddressType} {Flat}"
        //    //        : $"{StreetPrefix} {StreetName}, {Building}/{Corpus}, {AddressType} {Flat}"; }
        //    get
        //    {
        //        return string.IsNullOrEmpty(Corpus) ? $"{StreetName}, {Building}, {Flat}"
        //          : $"{StreetName}, {Building}/{Corpus}, {Flat}";
        //    }
        //}
    }
}