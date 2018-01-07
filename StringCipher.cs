using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace FGMCDocumentCopy.Helpers
{
    class StringCipher
    {
        const string ENCRYPTION_KEY = "MAKV2SPBNI99212";
        // encrypt whole list 
        public List<string> EncryptInputValues(List<string> valuesToEncrypt)
        {
            List<string> encryptedValues = new List<string>();
            foreach (var valueToEncryptt in valuesToEncrypt)
            {
                string encryptedValue = Encrypt(valueToEncryptt);
                encryptedValues.Add(encryptedValue);
            }
            return encryptedValues;
        }

        // decrypt whole list
        public List<string> DecryptInputValues(List<string> valuesToDecrypt)
        {
            List<string> decryptedValues = new List<string>();
            foreach (var valueToEncryptt in decryptedValues)
            {
                string decryptedValue = Decrypt(valueToEncryptt);
                decryptedValues.Add(decryptedValue);
            }
            return decryptedValues;
        }

        public string Encrypt(string valueToEncrypt)
        {
            try
            {
                
                byte[] clearBytes = Encoding.Unicode.GetBytes(valueToEncrypt);
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(ENCRYPTION_KEY,
                        new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (
                            CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(clearBytes, 0, clearBytes.Length);
                            cs.Close();
                        }
                        valueToEncrypt = Convert.ToBase64String(ms.ToArray());
                    }
                }
            }

            catch (Exception ex)
            {
                throw ex;
            }
            return valueToEncrypt;
        }

       

        public string Decrypt(string valueToDecrypt)
        {
            try
            {
                valueToDecrypt = valueToDecrypt.Replace(" ", "+");
               
                byte[] cipherBytes = Convert.FromBase64String(valueToDecrypt);
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(ENCRYPTION_KEY,
                        new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (
                            CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.Close();
                        }
                        valueToDecrypt = Encoding.Unicode.GetString(ms.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return valueToDecrypt;
        }

        

        // customized by waris.. below two additional methods for encryptiona and decryption using UTF8Encoding
        public string Encryptdata(string inputFieldValue)
        {
            inputFieldValue = inputFieldValue.Replace(" ", "+");   // Important to avoid not a valid 64byte error
            string strmsg = string.Empty;
            byte[] encode = new byte[inputFieldValue.Length];
            encode = System.Text.Encoding.UTF8.GetBytes(inputFieldValue);
            strmsg = Convert.ToBase64String(encode);
            return strmsg;
        }

        public string Decryptdata(string encryptFieldValue)
        {
            encryptFieldValue = encryptFieldValue.Replace(" ", "+");
            // System.Text Is Important
            string decryptpwd = string.Empty;
            System.Text.UTF8Encoding encodepwd = new System.Text.UTF8Encoding();
            System.Text.Decoder utf8Decode = encodepwd.GetDecoder();
            byte[] todecode_byte = Convert.FromBase64String(encryptFieldValue);
            int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
            char[] decoded_char = new char[charCount];
            utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
            decryptpwd = new string(decoded_char);
            return decryptpwd;

        }

    }
}
