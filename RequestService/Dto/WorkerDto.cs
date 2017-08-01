namespace RequestServiceImpl.Dto
{
    public class WorkerDto
    {
        public int Id { get; set; }
        public int? ServiceCompanyId { get; set; }
        public string ServiceCompanyName { get; set; }
        public string SurName { get; set; }
        public string FirstName { get; set; }
        public string PatrName { get; set; }
        public int? SpecialityId { get; set; }
        public string SpecialityName { get; set; }
        public string FullName => SurName + " " + FirstName + " " + PatrName;
        public string Phone { get; set; }
    }
}