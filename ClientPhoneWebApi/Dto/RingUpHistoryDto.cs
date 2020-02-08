using System;

namespace ClientPhoneWebApi.Dto
{
    public class RingUpHistoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FromPhone { get; set; }
        public DateTime CallTime { get; set; }
        public int StateId { get; set; }
        public string State => StateId == 0 ? "Новый"
            : StateId == 1 ? "В работе"
            : StateId == 2 ? "Выполнен"
            : StateId == 3 ? "Прерван" : "Неизвестно";
        public int PhoneCount { get; set; }
        public int DoneCalls { get; set; }
        public int NotDoneCalls { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}