namespace ClientPhoneWebApi.Dto
{
    public class NewNoteDto
    {
        public int UserId { get; set; }
        public int RequestId { get; set; }
        public string Note { get; set; }
    }
}