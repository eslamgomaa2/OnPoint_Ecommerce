using System.Security.Cryptography;
using System.Text;

namespace BuildingBlocks.Common.Helpers
{
    public static class GenerateNumericCode
    {
        public static string Generate(int length)
        {
            var bytes = RandomNumberGenerator.GetBytes(length);
            var sb = new StringBuilder(length);
            foreach (var b in bytes)
            {
                sb.Append((b % 10).ToString());
            }
            return sb.ToString();
        }
    }
}
