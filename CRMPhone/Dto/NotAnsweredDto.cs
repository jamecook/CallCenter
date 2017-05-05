using System;
using System.ComponentModel;
using System.Windows.Input;
using CRMPhone.Annotations;

namespace CRMPhone.Dto
{
    public class NotAnsweredDto
    {
        public string UniqueId { get; set; }

        public string CallerId { get; set; }

        public DateTime? CreateTime { get; set; }
    }
}