using System;

namespace RequestServiceImpl.Dto
{
    public class StatInfoDto
    {
        public DateTime StatDate { get; set; }
        public string Name { get; set; }
        public int? Count { get; set; }
    }
}