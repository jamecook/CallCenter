using System;

namespace RequestServiceImpl.Dto
{
    public class NotAnsweredDto
    {
        public string UniqueId { get; set; }

        public string CallerId { get; set; }

        public DateTime? CreateTime { get; set; }
    }
}