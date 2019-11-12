using System;

namespace WebApi.Models
{
    public class AttachmentDto
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public bool CanBeDeleted { get; set; }
        public DateTime CreateDate { get; set; }
        public UserDto User { get; set; }
    }
}