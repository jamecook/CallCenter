using System;

namespace RequestServiceImpl.Dto
{
    public class ExecuterToServiceCompanyDto
    {
        public int Id { get; set; }
        public WorkerDto Executer { get; set; }
        public int ServiceCompanyId { get; set; }
        public int TypeId { get; set; }
        public int Weigth { get; set; }
        public string Type { get; set; }
    }
}