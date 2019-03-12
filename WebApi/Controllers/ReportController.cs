using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("[controller]")]
    [Produces("application/json")]
    [Consumes("application/json", "multipart/form-data")]
    public class ReportController : Controller
    {
        public IConfiguration Configuration { get; }
        private readonly ILogger<RequestController> _logger;

        public ReportController(IConfiguration configuration, ILogger<RequestController> logger)
        {
            Configuration = configuration;
            _logger = logger;
        }

        [HttpGet("awailable")]
        public IEnumerable<ReportDto> GetAwailableReports()
        {
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            return RequestService.ReportsGetAwailable(workerId);
        }

        [HttpGet("base")]
        public byte[] BaseReport([FromQuery]string requestId, [FromQuery] bool? filterByCreateDate,
            [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] streets,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] houses,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] addresses,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] parentServices,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] services,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] statuses,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] workers,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] executors,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] ratings,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] companies,
            [FromQuery] bool? badWork,
            [FromQuery] bool? garanty,
            [FromQuery] bool? onlyRetry,
            [FromQuery] bool? chargeable,
            [FromQuery] string clientPhone,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] warranties,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] immediates,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] regions
            )

        {
            //var ttt = User.Claims.ToArray();
            //var login = User.Claims.FirstOrDefault(c => c.Type == "Login")?.Value;
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            int? rId = null;
            if (!string.IsNullOrEmpty(requestId))
            {
                if (int.TryParse(requestId, out int parseId))
                {
                    rId = parseId;
                }
                else
                {
                    rId = -1;
                }
            }
            var awailableReports = RequestService.ReportsGetAwailable(workerId);
            if (awailableReports.All(r => r.Url != "/report/base"))
                return null;

            var requests = RequestService.WebRequestListArrayParam(workerId, rId,
                filterByCreateDate ?? true,
                fromDate ?? DateTime.Today,
                toDate ?? DateTime.Today.AddDays(1),
                fromDate ?? DateTime.Today,
                toDate ?? DateTime.Today.AddDays(1),
                streets, houses, addresses, parentServices, services, statuses, workers, executors, ratings, companies, warranties, immediates,regions,
                badWork ?? false,
                garanty ?? false, onlyRetry ?? false, chargeable ?? false, clientPhone);

            XElement root = new XElement("Records");
            foreach (var request in requests)
            {
                root.AddFirst(
                    new XElement("Record",
                        new[]
                        {
                                        new XElement("Заявка", request.Id),
                                        new XElement("Мк_район", request.StreetName),
                                        new XElement("Улица", request.StreetName),
                                        new XElement("Дом", request.Building),
                                        new XElement("Корпус", request.Corpus),
                                        new XElement("Квартира", request.Flat),
                                        new XElement("Телефоны", request.ContactPhones),
                                        new XElement("ФИО", request.MainFio),
                                        new XElement("Причина", request.Service),
                                        new XElement("Описание", request.Description),
                                        new XElement("Выполнение", request.ExecuteTime?.Date.ToString("dd.MM.yyyy") ?? ""),
                                        new XElement("Время", request.ExecutePeriod),
                                        new XElement("Комментарий_исполнителя", request.LastNote),
                                        //new XElement("Заявка", request.Id),
                                        //new XElement("Статус", request.Status),
                                        //new XElement("ДатаСоздания", request.CreateTime.ToString("dd.MM.yyyy HH:mm")),
                                        //new XElement("Создатель", request.CreateUser.FullName),
                                        //new XElement("Улица", request.StreetName),
                                        //new XElement("Дом", request.Building),
                                        //new XElement("Корпус", request.Corpus),
                                        //new XElement("Квартира", request.Flat),
                                        //new XElement("Телефоны", request.ContactPhones),
                                        //new XElement("ФИО", request.MainFio),
                                        //new XElement("Услуга", request.ParentService),
                                        //new XElement("Причина", request.Service),
                                        //new XElement("Примечание", request.Description),
                                        //new XElement("Дата", request.ExecuteTime?.Date.ToString("dd.MM.yyyy") ?? ""),
                                        //new XElement("Время", request.ExecutePeriod),
                                        //new XElement("Мастер", request.Master?.FullName),
                                        //new XElement("Исполнитель", request.Executer?.FullName),
                                        //new XElement("ВыполнениеС", request.FromTime?.ToString("HH:mm:ss") ?? ""),
                                        //new XElement("ВыполнениеПо", request.ToTime?.ToString("HH:mm:ss") ?? ""),
                                        //new XElement("ПотраченоВремени", request.SpendTime),
                                        //new XElement("Гарантийная", request.GarantyTest),
                                        //new XElement("Аварийная", request.ImmediateText),
                                        //new XElement("Оценка", request.Rating),
                                        //new XElement("Комментарий_К_Оценке", request.RatingDescription),
                                        //new XElement("Повторная", request.IsRetry?"Да":""),
                                        //new XElement("Комментарий_исполнителя", request.LastNote),
                        }));
            }
            var saver = new MemoryStream();
            root.Save(saver);
            var buffer = saver.ToArray();
            saver.Close();
            return buffer;
        }
        [HttpGet("newExcel")]
        public byte[] NewExcelReport([FromQuery]string requestId, [FromQuery] bool? filterByCreateDate,
            [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] streets,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] houses,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] addresses,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] parentServices,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] services,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] statuses,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] workers,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] executors,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] ratings,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] companies,
            [FromQuery] bool? badWork,
            [FromQuery] bool? garanty,
            [FromQuery] bool? onlyRetry,
            [FromQuery] bool? chargeable,
            [FromQuery] string clientPhone,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] warranties,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] immediates,
            [ModelBinder(typeof(CommaDelimitedArrayModelBinder))]int[] regions
            )

        {
            //var ttt = User.Claims.ToArray();
            //var login = User.Claims.FirstOrDefault(c => c.Type == "Login")?.Value;
            var workerIdStr = User.Claims.FirstOrDefault(c => c.Type == "WorkerId")?.Value;
            int.TryParse(workerIdStr, out int workerId);
            int? rId = null;
            if (!string.IsNullOrEmpty(requestId))
            {
                if (int.TryParse(requestId, out int parseId))
                {
                    rId = parseId;
                }
                else
                {
                    rId = -1;
                }
            }
            var awailableReports = RequestService.ReportsGetAwailable(workerId);
            if (awailableReports.All(r => r.Url != "/report/newExcel"))
                return null;

            var requests = RequestService.WebRequestListArrayParam(workerId, rId,
                filterByCreateDate ?? true,
                fromDate ?? DateTime.Today,
                toDate ?? DateTime.Today.AddDays(1),
                fromDate ?? DateTime.Today,
                toDate ?? DateTime.Today.AddDays(1),
                streets, houses, addresses, parentServices, services, statuses, workers, executors, ratings, companies, warranties, immediates,regions,
                badWork ?? false,
                garanty ?? false, onlyRetry ?? false, chargeable ?? false, clientPhone);
            byte[] buffer = RequestService.GenerateExcel(requests);
            return buffer;
        }
    }
}