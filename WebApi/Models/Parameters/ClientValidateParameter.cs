using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Parameters
{
    public class ClientValidateParameter
    {
        [Required]
        public string Phone { get; set; }
    }
}