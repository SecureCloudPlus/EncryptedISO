using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

class ExtractISO
{
    private int progress = 0;
    public int Progress
    {
        get
        {
            return this.progress;
        }
    }

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
            int cnt = 0;
            foreach (FileInfo inFile in dinfo.GetFiles(@"*.enc"))
            {
                using (FileStream inFs = inFile.OpenRead())
                {
                    string encFileName = Path.ChangeExtension(inFile.Name, null);
                    string fileName = DecipherString(encFileName, password);
                    //bool success = false;
                    using (FileStream outFs = File.Create(targetFolder + "\\" + fileName))
                    {
                        if (!DecryptStream(inFs, outFs, password))
                        {
                            File.Delete(targetFolder + "\\" + fileName);
                            return 0;
                        }
                        cnt++;
                    }
                }
            }
            return cnt;
        }
        catch (Exception e)
        {
            return -1;
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
        using (var sha512Cng = new SHA512Cng())
        {
            byte[] keySourceHash = sha512Cng.ComputeHash(ASCIIEncoding.ASCII.GetBytes(passPhrase));
            string cipherKey = IncreaseVigenereCipherKeyEntropy(BitConverter.ToString(keySourceHash).Replace("-", ""));
            return VigenereCipher(cipherText, cipherKey, false);
        }
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
        catch (Exception exception)
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
            {
                using (var sha256Cng = new SHA256Cng())
                {
                    saltBytes = sha256Cng.ComputeHash(Encoding.Unicode.GetBytes(password));
                }
            }
            else
            {
                saltBytes = Encoding.Unicode.GetBytes(salt);
            }

            using(var aesCng = new AesCng())
            {
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, saltBytes);

                aesCng.BlockSize = 128;
                aesCng.Key = key.GetBytes(aesCng.KeySize / 8);
                aesCng.IV = key.GetBytes(aesCng.BlockSize / 8);
                aesCng.Padding = PaddingMode.PKCS7;
                aesCng.Mode = CipherMode.CBC;

                inStream.Position = 0;

                ICryptoTransform decryptor = aesCng.CreateDecryptor();

                using (CryptoStream decryptStream = new CryptoStream(inStream, decryptor, CryptoStreamMode.Read))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    do
                    {
                        bytesRead = decryptStream.Read(buffer, 0, buffer.Length);
                        OutStream.Write(buffer, 0, bytesRead);
                        progress++;
                    }
                    while (bytesRead != 0);

                }

                OutStream.Position = 0;
            }

            return true;
        }
        catch (Exception exception)
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