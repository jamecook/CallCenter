using System.Collections.Generic;

namespace RequestServiceImpl.Dto
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string SurName { get; set; }
        public string FirstName { get; set; }
        public string PatrName { get; set; }
        public List<RoleDto> Roles { get; set; }
    }
}