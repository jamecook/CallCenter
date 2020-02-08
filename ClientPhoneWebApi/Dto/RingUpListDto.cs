namespace ClientPhoneWebApi.Dto
{
    public class RingUpListDto
    {
        public int UserId { get; set; }
        public int ConfigId { get; set; }
        public RingUpImportDto[] Records { get; set; }
    }
}