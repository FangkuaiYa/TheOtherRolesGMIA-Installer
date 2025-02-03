using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace GMIAInstaller.Extensions
{
    public static class StreamExtensions
    {
        public static string Sha256Hash(this Stream stream)
        {
            using var sha256 = SHA256.Create();
            var sha256Hash = sha256.ComputeHash(stream);
            return BitConverter.ToString(sha256Hash).Replace("-", "");
        }

    }
}