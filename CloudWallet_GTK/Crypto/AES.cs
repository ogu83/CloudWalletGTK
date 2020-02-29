using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CloudWallet_GTK.Crypto
{
    public sealed class AES
    {
        private static byte[] _salt = Encoding.UTF8.GetBytes("ksdj123987654");

        //public static byte[] Encrypt(byte[] inputData, string sharedSecret)
        //{
        //    if (string.IsNullOrEmpty(sharedSecret))
        //        throw new ArgumentNullException("sharedSecret");

        //    byte[] outputData = null;                       // Encrypted string to return 
        //    RijndaelManaged aesAlg = null;              // RijndaelManaged object used to encrypt the data. 

        //    try
        //    {
        //        // generate the key from the shared secret and the salt 
        //        Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sharedSecret, _salt);

        //        // Create a RijndaelManaged object 
        //        // with the specified key and IV. 
        //        aesAlg = new RijndaelManaged();
        //        aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
        //        aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);

        //        // Create a decrytor to perform the stream transform. 
        //        ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        //        // Create the streams used for encryption. 
        //        using (MemoryStream msEncrypt = new MemoryStream())
        //        {
        //            using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        //            {
        //                //using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
        //                //{
        //                //    //Write all data to the stream. 
        //                //    swEncrypt.Write(inputData);
        //                //}
        //                foreach (byte b in inputData)
        //                    csEncrypt.WriteByte(b);
        //            }
        //            outputData = msEncrypt.ToArray();
        //        }
        //    }
        //    finally
        //    {
        //        // Clear the RijndaelManaged object. 
        //        if (aesAlg != null)
        //            aesAlg.Clear();
        //    }

        //    // Return the encrypted bytes from the memory stream. 
        //    return outputData;
        //}

        //public static byte[] Decrypt(byte[] cipherText, string sharedSecret)
        //{
        //    if (string.IsNullOrEmpty(sharedSecret))
        //        throw new ArgumentNullException("sharedSecret");

        //    // Declare the RijndaelManaged object 
        //    // used to decrypt the data. 
        //    RijndaelManaged aesAlg = null;

        //    // Declare the string used to hold 
        //    // the decrypted text. 
        //    List<byte> outputData = new List<byte>();

        //    try
        //    {
        //        // generate the key from the shared secret and the salt 
        //        Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sharedSecret, _salt);

        //        // Create a RijndaelManaged object 
        //        // with the specified key and IV. 
        //        aesAlg = new RijndaelManaged();
        //        aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
        //        aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);

        //        // Create a decrytor to perform the stream transform. 
        //        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
        //        // Create the streams used for decryption.                 
        //        using (MemoryStream msDecrypt = new MemoryStream(cipherText))
        //        {
        //            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
        //            {
        //                //using (StreamReader srDecrypt = new StreamReader(csDecrypt))
        //                //    // Read the decrypted bytes from the decrypting stream 
        //                //    // and place them in a string. 
        //                //    Convert.FromBase64String(srDecrypt.ReadToEnd());                        
        //                while (true)
        //                {
        //                    int b = csDecrypt.ReadByte();
        //                    if (b != -1)
        //                        outputData.Add((byte)b);
        //                    else
        //                        break;
        //                }
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        // Clear the RijndaelManaged object. 
        //        if (aesAlg != null)
        //            aesAlg.Clear();
        //    }

        //    return outputData.ToArray();
        //}

        public static byte[] Encrypt(byte[] inputData, string sharedSecret)
        {
            if (string.IsNullOrEmpty(sharedSecret))
                throw new ArgumentNullException("sharedSecret");

            byte[] outputData = null;                       // Encrypted string to return 
            //RijndaelManaged aesAlg = null;              // RijndaelManaged object used to encrypt the data. 

            try
            {
                // generate the key from the shared secret and the salt 
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sharedSecret, _salt);

                // Create a RijndaelManaged object 
                // with the specified key and IV. 
                //aesAlg = new RijndaelManaged();
                AesManaged aesAlg = new AesManaged();
                //aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                //aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);
                aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);

                // Create a decrytor to perform the stream transform. 
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption. 
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        //using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        //{
                        //    //Write all data to the stream. 
                        //    swEncrypt.Write(inputData);
                        //}

                        //foreach (byte b in inputData)
                        //    csEncrypt.WriteByte(b);

                        using (MemoryStream inputDataStream = new MemoryStream(inputData))
                            inputDataStream.CopyTo(csEncrypt);
                    }
                    outputData = msEncrypt.ToArray();
                }
            }
            finally
            {
                // Clear the RijndaelManaged object. 
                //if (aesAlg != null)
                //    aesAlg.Clear();
            }

            // Return the encrypted bytes from the memory stream. 
            return outputData;
        }

        public static byte[] Decrypt(byte[] cipherText, string sharedSecret)
        {
            if (string.IsNullOrEmpty(sharedSecret))
                throw new ArgumentNullException("sharedSecret");

            // Declare the RijndaelManaged object 
            // used to decrypt the data. 
            //RijndaelManaged aesAlg = null;
            //AesManaged aesAlg = new AesManaged();

            // Declare the string used to hold 
            // the decrypted text. 
            List<byte> outputData = new List<byte>();

            try
            {
                // generate the key from the shared secret and the salt 
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sharedSecret, _salt);

                // Create a RijndaelManaged object 
                // with the specified key and IV. 
                //aesAlg = new RijndaelManaged();
                AesManaged aesAlg = new AesManaged();
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);

                // Create a decrytor to perform the stream transform. 
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                // Create the streams used for decryption.                 
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        //using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        //    // Read the decrypted bytes from the decrypting stream 
                        //    // and place them in a string. 
                        //    Convert.FromBase64String(srDecrypt.ReadToEnd());          

                        //while (true)
                        //{
                        //    int b = csDecrypt.ReadByte();
                        //    if (b != -1)
                        //        outputData.Add((byte)b);
                        //    else
                        //        break;
                        //}

                        using (MemoryStream outputMemoryStream = new MemoryStream())
                        {
                            csDecrypt.CopyTo(outputMemoryStream);
                            return outputMemoryStream.ToArray();
                        }
                    }
                }
            }
            finally
            {
                // Clear the RijndaelManaged object. 
                //if (aesAlg != null)
                //    aesAlg.Clear();
            }
        }
    }
}
