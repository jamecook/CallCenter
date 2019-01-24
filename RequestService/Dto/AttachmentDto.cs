using System;

namespace RequestServiceImpl.Dto
{
    public class AttachmentDto
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public DateTime CreateDate { get; set; }
        public RequestUserDto User { get; set; }
    }
}