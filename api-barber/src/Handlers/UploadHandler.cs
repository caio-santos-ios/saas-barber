using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
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
                string fileName = Guid.NewGuid().ToString();

                using var stream = attachment.OpenReadStream();

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(attachment.FileName, stream),
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