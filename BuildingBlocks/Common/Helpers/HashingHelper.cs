using System.Security.Cryptography;
using System.Text;

namespace BuildingBlocks.Common.Helpers
{
    public static class HashingHelper
    {
        public static string Hash(string input)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }
    }
}
