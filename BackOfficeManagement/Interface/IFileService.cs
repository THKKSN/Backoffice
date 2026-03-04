
namespace BackOfficeManagement.Interface
{
    public interface IFileService
    {
        Task<byte[]> CompressToMaxSizeAsync(IFormFile file, long maxBytes = 2 * 1024 * 1024);
    }
}
