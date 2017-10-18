namespace RequestServiceImpl.Dto
{
    public class ServiceCompanyDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Info { get; set; }
        public string Sender { get; set; }
        public bool SendToClient { get; set; }
        public bool SendToWorker { get; set; }
    }
}