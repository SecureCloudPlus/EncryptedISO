using System;

namespace EncryptedISO
{
    class Program
    {
        static void Main(string[] args)
        {
            string s = BuildISO.EncodeString("the quick brown fox jumped over the lazy dog","password");


            string[] filesToBeEncrypted = new string[] { @"D:\test\TheGlobeReciept.pdf", @"D:\ParadoxSE.iso" };
            string[] filesNOTToBeEncrypted = new string[] { @"D:\test\Extract.exe" };
            string saveISOPath = @"D:\test\data.iso";
            string password = "password";
            int cnt = BuildISO.Build(filesToBeEncrypted, filesNOTToBeEncrypted, saveISOPath, password);
        }
    }
}
