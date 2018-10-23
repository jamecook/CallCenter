using System;

namespace RequestServiceImpl.Dto
{
    public class StatIvrCallListDto
    {
        public string CallerIdNum { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime? AnswerTime { get; set; }
        public string Result { get; set; }
        public int? WaitSec { get; set; }
        public int CallTime { get; set; }
    }
}