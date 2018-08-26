using System;

namespace WebApi.Models
{
    public class TokenDto
    {
        public string Access { get; set; }
        public Guid Refresh { get; set; }
    }
}