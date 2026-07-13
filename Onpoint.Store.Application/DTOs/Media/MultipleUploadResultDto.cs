namespace Onpoint.Store.Application.DTOs.Media
{
    public class MultipleUploadResultDto
    {
        public List<string> UploadedUrls { get; set; } = new();

        public List<string> FailedFiles { get; set; } = new();
    }
}
