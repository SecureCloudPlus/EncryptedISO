using System.IO;

internal class ExtractISO
{
    /// <summary>
    /// Decrypts (using the password) files with the .enc extension from the given source Folder and saves them to the target Folder 
    /// </summary>
    /// <param name="sourceFolder"></param>
    /// <param name="targetFolder"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public static int ExtractDirectory(string sourceFolder, string targetFolder,  string password)
    {
        int cnt = 0;
        try
        {
            DirectoryInfo dinfo = new DirectoryInfo(sourceFolder);
            foreach (FileInfo finfo in dinfo.GetFiles(@"*.enc"))
            {
                using (MemoryStream outMs = new MemoryStream())
                {
                    using (Stream FileStr = finfo.OpenRead())
                    {
                        string encFileName = Path.ChangeExtension(finfo.Name, null);
                        string fileName = Decryption.DecipherString(encFileName, password);
                        bool success = false;
                        using (FileStream Fs = File.Create(targetFolder + "\\" + fileName))
                        {
                            if (Decryption.DecryptStream(FileStr, outMs, password))
                            {
                                outMs.CopyTo(Fs);
                                cnt++;
                                success = true;
                            }
                        }
                        if (!success)
                            File.Delete(targetFolder + "\\" + fileName);
                    }
                }
            }
            return cnt;
        }
        catch
        {
            return -1;
        }
    }
}