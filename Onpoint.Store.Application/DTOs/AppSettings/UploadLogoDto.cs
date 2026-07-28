namespace Onpoint.Store.Application.DTOs.AppSettings
{
    public class UploadLogoDto
    {
        public string FileName { get; set; } = string.Empty;
        public Stream FileContent { get; set; } = Stream.Null;
    }
}
