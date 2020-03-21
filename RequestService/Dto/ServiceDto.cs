namespace RequestServiceImpl.Dto
{
    public class ServiceDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool CanSendSms { get; set; }
        public bool Immediate { get; set; }
        public bool AvailableForClient { get; set; }

        public override string ToString()
        {
            return $"{Id}:{Name}";
        }

    }
}