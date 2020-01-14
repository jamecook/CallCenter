using System;

namespace RequestServiceImpl.Dto
{
    public class RequestForListShortDto
    {
        public int Id { get; set; }
        public DateTime CreateTime { get; set; }
        public string StreetPrefix { get; set; }
        public string StreetName { get; set; }
        public string Building { get; set; }
        public string Corpus { get; set; }
        public string Flat { get; set; }
        public string AddressType { get; set; }
        public string Description { get; set; }
        public string ParentService { get; set; }
        public string Service { get; set; }
        public string ContactPhones { get; set; }
        public string ServiceCompany { get; set; }

        public string FullAddress
        {
            //get { return string.IsNullOrEmpty(Corpus)?$"{StreetPrefix} {StreetName}, {Building}, {AddressType} {Flat}"
            //        : $"{StreetPrefix} {StreetName}, {Building}/{Corpus}, {AddressType} {Flat}"; }
            get
            {
                return string.IsNullOrEmpty(Corpus) ? $"{StreetName}, {Building}, {Flat}"
                    : $"{StreetName}, {Building} к.{Corpus}, {Flat}";
            }
        }

    }
}