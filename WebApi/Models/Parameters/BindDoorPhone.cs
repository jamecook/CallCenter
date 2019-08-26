using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Parameters
{
    public class BindDoorPhone
    {
        [Required]
        public int HouseId { get; set; }
        [Required]
        public string DoorUid { get; set; }
        [Required]
        public string DoorNumber { get; set; }
        [Required]
        public string FromFlat { get; set; }
        [Required]
        public string ToFlat { get; set; }
    }
}