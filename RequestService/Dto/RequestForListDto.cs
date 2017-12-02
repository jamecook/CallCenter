using System;

namespace RequestServiceImpl.Dto
{
    public class RequestForListDto
    {
        public int Id { get; set; }
        public bool HasAttachment { get; set; }
        public bool HasRecord { get; set; }
        public bool IsBadWork { get; set; }
        public string RecordUniqueId { get; set; }
        public DateTime CreateTime { get; set; }
        public string StreetPrefix { get; set; }
        public string StreetName { get; set; }
        public string Building { get; set; }
        public string Corpus { get; set; }
        public string Flat { get; set; }
        public string AddressType { get; set; }
        public RequestUserDto Worker { get; set; }
        public RequestUserDto CreateUser { get; set; }
        public DateTime? ExecuteTime { get; set; }
        public string ExecutePeriod { get; set; }
        public string Description { get; set; }
        public string ParentService { get; set; }
        public string Service { get; set; }
        public string ContactPhones { get; set; }
        public string Rating { get; set; }
        public string RatingDescription { get; set; }
        public bool BadWork { get; set; }
        public int StatusId { get; set; }
        public string Status { get; set; }
        public string SpendTime { get; set; }
        public DateTime? FromTime { get; set; }
        public DateTime? ToTime { get; set; }
        public DateTime? AlertTime { get; set; }


        public string FullAddress
        {
            //get { return string.IsNullOrEmpty(Corpus)?$"{StreetPrefix} {StreetName}, {Building}, {AddressType} {Flat}"
            //        : $"{StreetPrefix} {StreetName}, {Building}/{Corpus}, {AddressType} {Flat}"; }
            get
            {
                return string.IsNullOrEmpty(Corpus) ? $"{StreetName}, {Building}, {Flat}"
                  : $"{StreetName}, {Building}/{Corpus}, {Flat}";
            }
        }

    }
}