namespace RequestServiceImpl.Dto
{
    public class StatCallListDto
    {
        public string Direction { get; set; }
        public string Exten { get; set; }
        public string ServiceCompany { get; set; }
        public string PhoneNum { get; set; }
        public string CreateDate { get; set; }
        public string CreateTime { get; set; }
        public string BridgeDate { get; set; }
        public string BridgeTime { get; set; }
        public string EndDate { get; set; }
        public string EndTime { get; set; }
        public int? WaitSec { get; set; }
        public int CallTime { get; set; }
        public int? UserId { get; set; }
        public string Fio { get; set; }
    }
}