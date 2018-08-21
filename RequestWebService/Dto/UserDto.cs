namespace RequestWebService.Dto
{
    public class UserDto
    {
        public int? Id { get; set; }
        public string BitrixId { get; set; }
        public string SurName { get; set; }
        public string FirstName { get; set; }
        public string PatrName { get; set; }
        public string DefaultServiceCompany { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool IsMaster { get; set; }
    }
}