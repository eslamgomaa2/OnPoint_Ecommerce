namespace BuildingBlocks.Common.Exceptions
{
    public class InvalidImageException : Exception
    {
        public InvalidImageException(string message = "Invalid image provided.") : base(message) { }
    }
}