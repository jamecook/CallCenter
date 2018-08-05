using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RequestWebService.Controllers
{
    [Produces("application/json")]
    [Route("api/Image")]
    public class ImageController : Controller
    {
        private readonly IHostingEnvironment _environment;

        public ImageController(IHostingEnvironment environment)
        {
            _environment = environment ;//?? throw new ArgumentNullException(nameof(environment));
        }

        // POST: api/Image
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Content("file not selected");
            var uploads = Path.Combine(_environment.WebRootPath, "uploads");

            return Content("file uploaded");
            if (file.Length > 0)
            {
                using (var fileStream = new FileStream(Path.Combine(uploads, file.FileName), FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }
        }
    }
}