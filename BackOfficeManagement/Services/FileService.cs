using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using BackOfficeManagement.Interface;

public class FileService : IFileService
{
    public async Task<byte[]> CompressToMaxSizeAsync(IFormFile file, long maxBytes = 2 * 1024 * 1024)
    {
        if (file.ContentType == "image/webp" ||
        Path.GetExtension(file.FileName).ToLower() == ".webp")
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            return ms.ToArray();
        }

        if (file.Length <= maxBytes)
        {
            using var stream = file.OpenReadStream();
            using var image = await Image.LoadAsync(stream);

            using var ms = new MemoryStream();
            var encoder = new WebpEncoder
            {
                Quality = 90,
                FileFormat = WebpFileFormatType.Lossy
            };

            await image.SaveAsWebpAsync(ms, encoder);
            return ms.ToArray();
        }

        // ถ้าใหญ่กว่า 2MB → เข้าสู่โหมดลด quality
        using (var stream = file.OpenReadStream())
        {
            using var image = await Image.LoadAsync(stream);

            int quality = 90;
            byte[] output;

            do
            {
                using var ms = new MemoryStream();

                var encoder = new WebpEncoder
                {
                    Quality = quality,
                    FileFormat = WebpFileFormatType.Lossy
                };

                await image.SaveAsWebpAsync(ms, encoder);

                output = ms.ToArray();

                if (output.Length <= maxBytes)
                    break;

                quality -= 5;
                if (quality < 10)
                    break;

            } while (true);

            return output;
        }
    }
}

