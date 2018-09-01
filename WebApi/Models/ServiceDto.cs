namespace WebApi.Models
{
    public class ServiceDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool CanSendSms { get; set; }
    }
}