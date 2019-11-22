using System;

namespace WebApi.Models
{
    public class DocTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class OrganizationalTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class DocOrgDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Inn { get; set; }
        public string Director { get; set; }
    }

    public class DocStatusDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class DocDto
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public string Topic { get; set; }
        public string DocNumber { get; set; }
        public DateTime DocDate { get; set; }
        public string InNumber { get; set; }
        public string OutNumber { get; set; }
        public DateTime? InDate { get; set; }
        public DateTime? OutDate { get; set; }
        public DateTime? DoneDate { get; set; }
        public UserDto CreateUser { get; set; }
        public DocOrgDto Org { get; set; }
        public DocStatusDto Status { get; set; }
        public DocTypeDto Type { get; set; }
        public OrganizationalTypeDto OrganizationalType { get; set; }
        public string Description { get; set; }
        public int AttachCount { get; set; }
    }

    public class CreateDocDto
    {
        public int? Id { get; set; }
        public int TypeId { get; set; }
        public string Topic { get; set; }
        public string DocNumber { get; set; }
        public DateTime DocDate { get; set; }
        public string InNumber { get; set; }
        public DateTime? InDate { get; set; }
        public OrgDocDto[] Orgs { get; set; }
        public int? AppointedWorkerId { get; set; }
        public int? OrganizationalTypeId { get; set; }
        public string Description { get; set; }
    }
    public class OrgDocDto
    {
        public int? Id { get; set; }
        public int OrgId { get; set; }
        public string InNumber { get; set; }
        public DateTime? InDate { get; set; }
    }

    public class AttachmentToDocDto
    {
        public int Id { get; set; }
        public int DocId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }
        public DateTime CreateDate { get; set; }
        public UserDto User { get; set; }
    }


}