using BookProject.IService;
using DotNetEnv;

namespace BookProject.Service
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FileService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string path)
        {
            if (file == null || file.Length == 0)

                throw new ArgumentException("File is empty or null", nameof(file));

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(_env.WebRootPath, path, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var request = _httpContextAccessor.HttpContext.Request;
            return $"{request.Scheme}://{request.Host}/{path}/{fileName}";
        }
    }
}

   
