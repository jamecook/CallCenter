using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
    public class WarrantyFileInfoDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int RequestId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string FileName { get; set; }
    }
}