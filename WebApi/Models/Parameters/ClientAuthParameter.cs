using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Parameters
{
    public class ClientAuthParameter
    {
        [Required]
        public string Phone { get; set; }
        [Required]
        public string Code { get; set; }
        [Required]
        public string DeviceId { get; set; }
    }
}