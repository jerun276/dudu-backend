using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CupidLearn.Application.Abstractions;

namespace CupidLearn.Api.Controllers
{
    [Route("api/media")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        private readonly IFileStorageService _storage;

        public MediaController(IFileStorageService storage)
        {
            _storage = storage;
        }

        [HttpPost("upload")]
        [AllowAnonymous]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            using var stream = file.OpenReadStream();
            var url = await _storage.UploadAsync(stream, Path.GetFileName(file.FileName), file.ContentType);

            return Ok(new { Url = url });
        }
    }
}
