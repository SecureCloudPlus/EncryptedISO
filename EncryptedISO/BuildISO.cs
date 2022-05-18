using DiscUtils.Iso9660;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

internal class BuildISO
{
    /// <summary>
    /// Creates an ISO file and populates it with the files provided before saving to the given location
    /// Encrypts those files that require encryption with the password provided
    /// </summary>
    /// <param name="filesToBeEncrypted"></param>
    /// <param name="filesNOTToBeEncrypted"></param>
    /// <param name="saveISOPath"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public static int Build(string[] filesToBeEncrypted, string[] filesNOTToBeEncrypted, string saveISOPath, string password)
    {
        int cnt = 0;
        try
        {
            CDBuilder builder = new CDBuilder
            {
                UseJoliet = true,
                VolumeIdentifier = string.Empty
            };
            foreach (string file in filesToBeEncrypted)
            {
                FileInfo inFile = new FileInfo(file);
                string tempFile = @"d:\test\" + EncodeString(inFile.Name, password) + ".enc";
                using (FileStream inFs = inFile.OpenRead())
                {

                    FileInfo outFile = new FileInfo(tempFile);
                    using (FileStream outFs = outFile.OpenWrite())
                    {
                        if (EncryptStream(inFs, outFs, password))
                        {
                            outFs.Close();
                            builder.AddFile(Path.GetFileName(tempFile), tempFile);
                            builder.Build(saveISOPath);
                            cnt++;
                        }
                    }
                }

                /**
                                        HugeMemoryStream outMs = new HugeMemoryStream();
                                        if (EncryptStream(inFs, outMs, password))
                                        {
                                            builder.AddFile(EncodeString(inFile.Name, password) + ".enc", outMs);
                                            builder.Build(saveISOPath);
                                            cnt++;
                                        }

                **/

            }
            foreach (string file in filesNOTToBeEncrypted)
            {
                FileInfo inFile = new FileInfo(file);
                using (FileStream inFs = inFile.OpenRead())
                {
                    builder.AddFile(inFile.Name, inFs);
                    builder.Build(saveISOPath);
                    cnt++;
                }
            }
            return cnt;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return -1;
        }
    }

    /// <summary>
    /// Creates a long string of values A-Z using the password to act as a one time pad for the Vigenere Cipher
    /// Takes a plaintext string and returns an encoded string.
    /// </summary>
    /// <param name="plainText"></param>
    /// <param name="passPhrase"></param>
    /// <returns></returns>
    public static string EncodeString(string plainText, string passPhrase)
    {
        SHA512 sha512Hash = SHA512.Create();
        byte[] keySourceHash = sha512Hash.ComputeHash(ASCIIEncoding.ASCII.GetBytes(passPhrase));
        string cipherKey = IncreaseVigenereCipherKeyEntropy(BitConverter.ToString(keySourceHash).Replace("-", ""));
        return VigenereCipher(plainText, cipherKey, true);
    }

    /// <summary>
    /// Takes a HEX string and returns a string value based on HEX input string but replaces the numbers with characters from A-Z
    /// </summary>
    /// <param name="tmpKey"></param> 
    /// <returns></returns>
    private static string IncreaseVigenereCipherKeyEntropy(string tmpKey)
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
    /// Modulo calculator
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private static int Mod(int a, int b)
    {
        return (a % b + b) % b;
    }

    /// <summary>
    /// Encodes the input using the key and returns the encoded output
    /// </summary>
    /// <param name="input"></param>
    /// <param name="key"></param>
    /// <param name="encipher"></param>
    /// <returns></returns>
    private static string VigenereCipher(string input, string key, bool encipher)
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
    /// Encrypts the input data stream to the output data stream using AES 256CBC 
    /// </summary>
    /// <param name="inStream"></param>
    /// <param name="OutStream"></param>
    /// <param name="password"></param>
    /// <param name="salt"></param>
    /// <returns></returns>
    private static bool EncryptStream(Stream inStream, Stream OutStream, string password, string salt = null)
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
                ICryptoTransform encryptor = encryption.CreateEncryptor();
                inStream.Position = 0;
                CryptoStream encryptStream = new CryptoStream(OutStream, encryptor, CryptoStreamMode.Write);
                inStream.CopyTo(encryptStream);
                encryptStream.FlushFinalBlock();
            }
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
    }
}