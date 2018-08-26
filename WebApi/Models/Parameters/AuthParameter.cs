using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Parameters
{
    public class AuthParameter
    {
        [Required]
        public string Login { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
