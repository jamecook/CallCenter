namespace RequestServiceImpl.Dto
{
    public class WebUserDto
    {
        public int Id { get; set; }
        public string SurName { get; set; }
        public string FirstName { get; set; }
        public string PatrName { get; set; }
        public string FullName => SurName + " " + FirstName + " " + PatrName;
    }
}