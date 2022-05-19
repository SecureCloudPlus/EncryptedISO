using System;
using System.IO;

namespace EncryptedISO
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] filesToBeEncrypted = new string[] { @"D:\test\TheGlobeReciept.pdf", @"D:\ParadoxSE.iso" };
            string[] filesNOTToBeEncrypted = new string[] { @"D:\test\Extract.exe" };
            string saveISOPath = @"D:\test\data.iso";
            string tempPath = Path.GetTempPath(); ;
            string password = "password";
            int cnt = BuildISO.Build(filesToBeEncrypted, filesNOTToBeEncrypted, saveISOPath, tempPath, password);
        }
    }
}
