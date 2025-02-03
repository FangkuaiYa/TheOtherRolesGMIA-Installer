using System.IO;

namespace GMIAInstaller.Extensions
{
    public static class FileExtensions
    {
        public static string Sha256Hash(string filePath)
        {
            using var file = File.OpenRead(filePath);
            return file.Sha256Hash();
        }
        
        public static bool FileEqualsSha256Hash(string filePath, string sha256Hash)
        {
            return Sha256Hash(filePath) == sha256Hash;
        }
    }
}