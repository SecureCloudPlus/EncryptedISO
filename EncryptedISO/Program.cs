using System.IO;

namespace EncryptedISO
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string[] filesToBeEncrypted = new string[] { @"D:\ParadoxSE.iso", @"D:\test\TheGlobeReciept - Copy.pdf", @"D:\test\Bulldog Commission Guide 500084v2_2.pdf" };
            string[] filesNOTToBeEncrypted = new string[] { @"D:\test\Extract.exe" };
            string saveISOPath = @"D:\test\data.iso";
            string tempPath = Path.GetTempPath(); ;
            string password = "password";
            int cnt = BuildISO.Build(filesToBeEncrypted, filesNOTToBeEncrypted, saveISOPath, tempPath, password);
        }
    }
}