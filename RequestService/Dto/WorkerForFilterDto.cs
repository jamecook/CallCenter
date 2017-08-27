namespace RequestServiceImpl.Dto
{
    public class WorkerForFilterDto
    {
        public int Id { get; set; }
        public string SurName { get; set; }
        public string FirstName { get; set; }
        public string PatrName { get; set; }
        public string FullName => SurName + " " + FirstName + " " + PatrName;
        public bool Selected { get; set; }
    }
}