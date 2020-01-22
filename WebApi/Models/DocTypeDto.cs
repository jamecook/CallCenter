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

    public class DocAttachOrgDto
    {
        public int OrgId { get; set; }
        public string OrgName { get; set; }
        public string OrgInn { get; set; }
        public string DirectorFio { get; set; }
        public string InNumber { get; set; }
        public DateTime? InDate { get; set; }
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
        public int DocYear { get; set; }
        public string InNumber { get; set; }
        public DateTime? InDate { get; set; }
        public DateTime? DoneDate { get; set; }
        public UserDto CreateUser { get; set; }
        public ShortAddressDto ClientAddress { get; set; }
        public UserDto AppointedWorker { get; set; }
        public DocOrgDto Org { get; set; }
        public DocStatusDto Status { get; set; }
        public DocTypeDto Type { get; set; }
        public DocAttachOrgDto[] AttachOrg { get; set; }
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
        public int? OrgId { get; set; }
        public OrgDocDto[] Orgs { get; set; }
        public int? AppointedWorkerId { get; set; }
        public int? OrganizationalTypeId { get; set; }
        public int? AddressId { get; set; }
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