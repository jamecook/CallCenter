using System;

namespace WebApi.Models
{
    public class MasterStatDto
    {
        public DateTime Date { get; set; }
        public int Incoming { get; set; }
        public int Done { get; set; }
        public int DoneInThisDay { get; set; }
        public int DoneInOtherDay { get; set; }
    }
}