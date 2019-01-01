using System;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
    public class WarrantyInfoDto
    {
        [Required]
        public int? Id { get; set; }
        [Required]
        public int RequestId { get; set; }
        [Required]
        public DateTime? StartDate { get; set; }
        [Required]
        public DateTime? BeginDate { get; set; }
        [Required]
        public DateTime? EndDate { get; set; }
        [Required]
        public DateTime? InsertDate { get; set; }
        [Required]
        public string ContactName { get; set; }
        [Required]
        public string ContactPhone { get; set; }
        [Required]
        public int? OrgId { get; set; }
        [Required]
        public WarrantyOrganizationDto Organization { get; set; }
    }
}