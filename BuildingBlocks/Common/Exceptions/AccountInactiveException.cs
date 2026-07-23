namespace BuildingBlocks.Common.Exceptions
{
    public class AccountInactiveException : Exception
    {
        public AccountInactiveException(string message = "Account is inactive.") : base(message) { }
    }
}