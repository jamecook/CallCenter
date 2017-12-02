using System;

namespace RequestServiceImpl.Dto
{
    public class AlertTimeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AddMinutes { get; set; }
    }
}