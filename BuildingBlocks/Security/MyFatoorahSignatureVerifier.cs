using System.Security.Cryptography;
using System.Text;

namespace BuildingBlocks.Security
{
    public static class MyFatoorahSignatureVerifier
    {
        public static bool Verify(string payload, string signatureHeader, string secret)
        {
            if (string.IsNullOrWhiteSpace(payload) ||
                string.IsNullOrWhiteSpace(signatureHeader) ||
                string.IsNullOrWhiteSpace(secret))
            {
                return false;
            }

            try
            {
                var keyBytes = Encoding.UTF8.GetBytes(secret);
                var payloadBytes = Encoding.UTF8.GetBytes(payload);

                // 2. إنشاء كائن الـ HMACSHA256 باستخدام الـ Secret
                using var hmac = new HMACSHA256(keyBytes);

                // 3. حساب الـ Hash للـ Payload
                var hashBytes = hmac.ComputeHash(payloadBytes);

                // 4. تحويل الـ Hash إلى String (Hexadecimal) وتحويله لحروف صغيرة
                var computedSignature = Convert.ToHexString(hashBytes).ToLowerInvariant();

                // 5. الـ Header القادم من ماي فاتورة قد يكون بحروف صغيرة أو كبيرة، لذا نوحده
                var incomingSignature = signatureHeader.Replace("-", "").ToLowerInvariant();

                // 6. مقارنة الـ Signature بطريقة آمنة (لمنع هجمات التوقيت - Timing Attacks)
                var incomingBytes = Convert.FromHexString(incomingSignature);
                return CryptographicOperations.FixedTimeEquals(hashBytes, incomingBytes);
            }
            catch
            {
                // أي خطأ في فك التشفير أو المقارنة يعني أن التوقيع غير صحيح
                return false;
            }
        }
    }
}