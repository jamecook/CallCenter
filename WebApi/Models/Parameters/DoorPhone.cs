using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Parameters
{
    public class DoorPhone
    {
        [Required]
        public string Phone { get; set; }
        [Required]
        public string DoorUid { get; set; }
    }
}