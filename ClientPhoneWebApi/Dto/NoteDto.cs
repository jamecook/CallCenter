using System;

namespace ClientPhoneWebApi.Dto
{
    public class NoteDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Note { get; set; }
        public RequestUserDto User { get; set; }
    }
}