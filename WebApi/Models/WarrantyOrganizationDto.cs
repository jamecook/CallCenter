using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
    public class WarrantyOrganizationDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Inn { get; set; }
        [Required]
        public string DirectorFio { get; set; }
    }
}