using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Parameters
{
    public class SetAddressDto
    {
        [Required]
        public int AddressId { get; set; }
        [Required]
        public int ServiceId { get; set; }
        [Required]
        public int? MasterId { get; set; }
        [Required]
        public int? ExecutorId { get; set; }
    }
}