namespace BookProject.IService
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string path);
    }
}
