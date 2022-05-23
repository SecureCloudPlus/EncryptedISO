using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

 class ExtractISO
{
    private int progress = -1;
    public int Progress
    {
        get
        {
            return this.progress;
        }
    }


    //static ToolStripProgressBar progress;

    /// <summary>
    /// Decrypts (using the password) files with the .enc extension from the given source Folder and saves them to the target Folder 
    /// </summary>
    /// <param name="sourceFolder"></param>
    /// <param name="targetFolder"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public int ExtractDirectory(string sourceFolder, string targetFolder, string password)
    {
        progress = 1;
        try
        {
            DirectoryInfo dinfo = new DirectoryInfo(sourceFolder);
            foreach (FileInfo inFile in dinfo.GetFiles(@"*.enc"))
            {
                using (FileStream inFs = inFile.OpenRead())
                {
                    string encFileName = Path.ChangeExtension(inFile.Name, null);
                    string fileName = DecipherString(encFileName, password);
                    bool success = false;
                    using (FileStream outFs = File.Create(targetFolder + "\\" + fileName))
                    {
                        if (DecryptStream(inFs, outFs, password))
                        {
                            inFs.CopyTo(outFs);
                            progress++;
                            success = true;
                        }
                    }
                    if (!success)
                        File.Delete(targetFolder + "\\" + fileName);
                }
            }
            return progress;
        }
        catch (Exception e)
        {
            progress = -1;
            return progress;
        }
    }

    /// <summary>
    /// Creates a long string of values A-Z using the password to act as a one time pad for the Vigenere Cipher
    /// Takes an encoded string and returns a decoded string.
    /// </summary>
    /// <param name="plainText"></param>
    /// <param name="passPhrase"></param>
    /// <returns></returns>
    public string DecipherString(string cipherText, string passPhrase)
    {
        SHA512 sha512Hash = SHA512.Create();
        byte[] keySourceHash = sha512Hash.ComputeHash(ASCIIEncoding.ASCII.GetBytes(passPhrase));
        string cipherKey = IncreaseVigenereCipherKeyEntropy(BitConverter.ToString(keySourceHash).Replace("-", ""));
        return VigenereCipher(cipherText, cipherKey, false);
    }

    /// <summary>
    /// Takes a HEX string and returns a string value based on HEX input string but replaces the numbers with characters from A-Z
    /// </summary>
    /// <param name="tmpKey"></param>
    /// <returns></returns>
    private string IncreaseVigenereCipherKeyEntropy(string tmpKey)
    {
        try
        {
            StringBuilder outKey = new StringBuilder();
            string alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            int ctr = 0;
            foreach (char c in tmpKey)
            {
                string v = c.ToString();
                if (!(c >= 'A' && c <= 'Z'))
                {
                    int i = (int)Char.GetNumericValue(c) + ctr;
                    i = i % 26;
                    v = alpha[i].ToString();
                    ctr++;
                    ctr = ctr % 26;
                }
                outKey.Append(v.ToString());
            }
            return outKey.ToString();
        }
        catch (Exception e)
        {
        }
        return null;
    }

    /// <summary>
    /// Decrypts the input data stream to the output data stream using AES256 CBC 
    /// </summary>
    /// <param name="inStream"></param>
    /// <param name="OutStream"></param>
    /// <param name="password"></param>
    /// <param name="salt"></param>
    /// <returns></returns>
    public bool DecryptStream(Stream inStream, Stream OutStream, string password, string salt = null)
    {
        try
        {
            byte[] saltBytes;
            if (salt == null)
                saltBytes = new SHA256Managed().ComputeHash(Encoding.Unicode.GetBytes(password));
            else
                saltBytes = Encoding.Unicode.GetBytes(salt);
            using (Aes encryption = Aes.Create())
            {
                encryption.BlockSize = 128;
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, saltBytes);
                encryption.Key = key.GetBytes(encryption.KeySize / 8);
                encryption.IV = key.GetBytes(encryption.BlockSize / 8);
                encryption.Padding = PaddingMode.PKCS7;
                encryption.Mode = CipherMode.CBC;
                ICryptoTransform encryptor = encryption.CreateDecryptor();
                inStream.Position = 0;
                CryptoStream encryptStream = new CryptoStream(inStream, encryptor, CryptoStreamMode.Read);
                encryptStream.CopyTo(OutStream);
                OutStream.Position = 0;
            }
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    /// <summary>
    /// Decodes the input using the key and returns the decoded output
    /// </summary>
    /// <param name="input"></param>
    /// <param name="key"></param>
    /// <param name="encipher"></param>
    /// <returns></returns>
    private string VigenereCipher(string input, string key, bool encipher)
    {
        for (int i = 0; i < key.Length; ++i)
            if (!char.IsLetter(key[i]))
                return null;
        string output = string.Empty;
        int nonAlphaCharCount = 0;
        for (int i = 0; i < input.Length; ++i)
        {
            if (char.IsLetter(input[i]))
            {
                bool cIsUpper = char.IsUpper(input[i]);
                char offset = cIsUpper ? 'A' : 'a';
                int keyIndex = (i - nonAlphaCharCount) % key.Length;
                int k = (cIsUpper ? char.ToUpper(key[keyIndex]) : char.ToLower(key[keyIndex])) - offset;
                k = encipher ? k : -k;
                char ch = (char)(Mod(input[i] + k - offset, 26) + offset);
                output += ch;
            }
            else
            {
                output += input[i];
                ++nonAlphaCharCount;
            }
        }
        return output;
    }

    /// <summary>
    /// Modulo calculator
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private int Mod(int a, int b)
    {
        return (a % b + b) % b;
    }
}