using System;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Parameters
{
    public class SetExecuteDateParams
    {
        [Required]
        public DateTime ExecuteDate { get; set; }
        [Required]
        public string Note { get; set; }
    }
}