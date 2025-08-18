using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // /api/files
    public class FilesController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        public FilesController(IWebHostEnvironment env) => _env = env;

        [HttpPost("image")]
        [RequestSizeLimit(10_000_000)] // ~10 MB
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file is null || file.Length == 0)
                return BadRequest(new { error = "No file uploaded." });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            if (!allowed.Contains(ext))
                return BadRequest(new { error = "Only jpg, jpeg, png, webp, gif allowed." });

            // Ensure wwwroot/uploads exists
            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var uploadsRoot = Path.Combine(webRoot, "uploads");
            Directory.CreateDirectory(uploadsRoot);

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var savePath = Path.Combine(uploadsRoot, fileName);

            await using (var stream = System.IO.File.Create(savePath))
                await file.CopyToAsync(stream);

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var url = $"{baseUrl}/uploads/{fileName}";

            return Ok(new { url });
        }
    }
}
