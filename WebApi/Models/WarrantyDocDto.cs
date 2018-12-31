using System;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
    public class WarrantyDocDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int? RequestId { get; set; }
        [Required]
        public DateTime CreateDate { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Direction { get; set; }
        [Required]
        public WarrantyOrganizationDto Organization { get; set; }
        [Required]
        public WorkerDto CreateWorker { get; set; }
    }
}