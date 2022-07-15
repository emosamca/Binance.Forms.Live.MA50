using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Binance.Connections
{
    public static class Helpers
    {
        private static readonly Encoding SignatureEncoding = Encoding.UTF8;

        public static string CreateSignature(this string message, string secret)
        {

            byte[] keyBytes = SignatureEncoding.GetBytes(secret);
            byte[] messageBytes = SignatureEncoding.GetBytes(message);
            HMACSHA256 hmacsha256 = new HMACSHA256(keyBytes);

            byte[] bytes = hmacsha256.ComputeHash(messageBytes);

            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

    }
}
