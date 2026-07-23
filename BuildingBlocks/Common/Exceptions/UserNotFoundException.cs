namespace BuildingBlocks.Common.Exceptions
{
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(string message = "User was not found.") : base(message) { }
    }
}