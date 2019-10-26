using System;

namespace WebApi.Models
{
    public class DocTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class DocAgentDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Inn { get; set; }
        public string Director { get; set; }
    }
    public class DocKindDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
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
        public DateTime InsertDate { get; set; }
        public string InNumber { get; set; }
        public string OutNumber { get; set; }
        public DateTime? InDate { get; set; }
        public DateTime? OutDate { get; set; }
        public DateTime? DoneDate { get; set; }
        public UserDto CreateUser { get; set; }
        public DocAgentDto Agent { get; set; }
        public DocStatusDto Status { get; set; }
        public DocKindDto Kind { get; set; }
        public DocTypeDto Type { get; set; }
        public string Description { get; set; }
        public int AttachCount { get; set; }
    }

    public class CreateOrUpdateDocDto
    {
        public int? Id { get; set; }
        public DateTime CreateDate { get; set; }
        public string InNumber { get; set; }
        public string OutNumber { get; set; }
        public DateTime? InDate { get; set; }
        public DateTime? OutDate { get; set; }
        public DateTime? DoneDate { get; set; }
        public int? AgentId { get; set; }
        public int StatusId { get; set; }
        public int KindId { get; set; }
        public int TypeId { get; set; }
        public string Description { get; set; }
    }


}