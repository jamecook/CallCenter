using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Parameters
{
    public class TermParameter
    {
        [Required]
        public string Temperature { get; set; }
        [Required]
        public string Pressure { get; set; }
        [Required]
        public string Humidity { get; set; }
    }
}