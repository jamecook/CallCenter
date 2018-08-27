namespace WebApi.Models
{
    public class WebUserDto
    {
        public int UserId { get; set; }
        public string SurName { get; set; }
        public string FirstName { get; set; }
        public string PatrName { get; set; }
        public int WorkerId { get; set; }
        public int ServiceCompanyId { get; set; }
        public int SpecialityId { get; set; }
        public bool CanCreateRequestInWeb { get; set; }
        public string FullName => SurName + " " + FirstName + " " + PatrName;
    }
}