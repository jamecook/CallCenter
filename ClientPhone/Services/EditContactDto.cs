﻿using RequestServiceImpl.Dto;

namespace ClientPhone.Services
{
    public class EditContactDto
    {
        public int UserId { get; set; }
        public int RequestId { get; set; }
        public ContactDto[] Contacts { get; set; }
    }
}