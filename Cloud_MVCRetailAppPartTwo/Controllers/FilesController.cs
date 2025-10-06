using Cloud_MVCRetailAppPartTwo.Models;
using Cloud_MVCRetailAppPartTwo.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cloud_MVCRetailAppPartTwo.Controllers
{
    public class FilesController : Controller
    {
        private readonly FileService _fileService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public FilesController(FileService fileService, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _fileService = fileService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }
        public async Task<IActionResult> Index()
        {

            List<FileModel> files;
            try
            {

                files = await _fileService.ListFilesAsync();
            }
            catch (Exception ex)
            {

                ViewBag.Message = $"Failed to load files : {ex.Message}";
                files = new List<FileModel>();
            }
            return View(files);
        }


        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return await Index();

            try
            {
                string directoryName = "directory";
                string fileName = file.FileName;
                using var stream = file.OpenReadStream();
                await _fileService.UploadFileAsync(directoryName, fileName, stream);

                TempData["Message"] = $"File '{fileName}' uploaded successfully.";
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"File upload failed: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

    }
}
