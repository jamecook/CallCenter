using System;
using System.ComponentModel;

namespace CRMPhone.Dto
{
    public class RequestForListDto
    {
        public int Id { get; set; }
        public DateTime CreateTime { get; set; }
        public string StreetPrefix { get; set; }
        public string StreetName { get; set; }
        public string Building { get; set; }
        public string Corpus { get; set; }
        public string Flat { get; set; }
        public string AddressType { get; set; }

        public string FullAddress
        {
            get { return string.IsNullOrEmpty(Corpus)?$"{StreetPrefix} {StreetName}, {Building}, {AddressType} {Flat}"
                    : $"{StreetPrefix} {StreetName}, {Building}/{Corpus}, {AddressType} {Flat}"; }
        }

    }
}