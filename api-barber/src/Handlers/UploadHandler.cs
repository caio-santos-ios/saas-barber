using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using SixLabors.ImageSharp;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace api_barber.src.Handlers
{
    public class UploadHandler(Cloudinary cloudinary, ILogger<UploadHandler> logger)
    {
        public async Task<string> UploadAttachment(string parent, IFormFile attachment, string apiPath = "")
        {
            try
            {
                string extension = Path.GetExtension(attachment.FileName).ToLower();
                bool isHeic = extension == ".heic" || extension == ".heif";
                string fileName = Guid.NewGuid().ToString();

                using var memoryStream = new MemoryStream();

                if (isHeic)
                {
                    using var inputStream = attachment.OpenReadStream();
                    using var image = await Image.LoadAsync(inputStream);
                    await image.SaveAsJpegAsync(memoryStream);
                    memoryStream.Position = 0;
                    extension = ".jpg";
                }
                else
                {
                    await attachment.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;
                }

                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(fileName + extension, memoryStream),
                    Folder = $"saas-barbearia/{parent}",
                    PublicId = fileName
                };

                var result = await cloudinary.UploadAsync(uploadParams);

                if (result.Error != null)
                {
                    logger.LogError("Failed to upload attachment: {Message}", result.Error.Message);
                    return "";
                }

                return result.SecureUrl.ToString();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to upload attachment");
                return "";
            }
        }
    }
}