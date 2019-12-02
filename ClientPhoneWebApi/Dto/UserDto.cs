namespace ClientPhoneWebApi.Dto
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string SurName { get; set; }
        public string FirstName { get; set; }
        public string PatrName { get; set; }
        public RoleDto[] Roles { get; set; }
        public string FullName => $"{SurName} {FirstName} {PatrName}".TrimEnd();
    }

}