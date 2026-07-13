namespace Onpoint.Store.Application.DTOs.Media
{
    public class FileUploadDto
    {
        public Stream FileContent { get; set; } = null!;
        public string FileName { get; set; } = null!;
    }
}
