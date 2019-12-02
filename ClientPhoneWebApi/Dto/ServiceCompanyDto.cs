namespace ClientPhoneWebApi.Dto
{
    public class ServiceCompanyDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Info { get; set; }
        public string Sender { get; set; }
        public string ShortName { get; set; }
        public string Prefix { get; set; }
        public string Phone { get; set; }
        public string ShortNameWithPrefix => ShortName + " " + Phone;

        public bool SendToClient { get; set; }
        public bool SendToWorker { get; set; }
    }
}