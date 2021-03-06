﻿using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using LibZConfig.Common.Config;

namespace LibZConfig.Common.Utils
{
    /// <summary>
    /// Utility class for Crypto related functions.
    /// </summary>
    public static class CryptoUtils
    {

        /// <summary>
        /// Get the hash (MD5) for the passed input string.
        /// </summary>
        /// <param name="key">String key</param>
        /// <returns>Hash String (Base64 encoded)</returns>
        public static string GetKeyHash(string key)
        {
            Preconditions.CheckArgument(key);

            using (MD5 md5 = MD5.Create())
            {
                byte[] input = Encoding.UTF8.GetBytes(key);
                byte[] output = md5.ComputeHash(input);

                return Convert.ToBase64String(output);
            }
        }

        /// <summary>
        /// Compare the hash of the passed key to the hash value.
        /// </summary>
        /// <param name="hash">Hash value</param>
        /// <param name="key">Key to compare</param>
        /// <returns>Matches?</returns>
        public static bool CompareHash(string hash, string key)
        {
            Preconditions.CheckArgument(hash);
            Preconditions.CheckArgument(key);

            string khash = GetKeyHash(key);
            return (hash == khash);
        }

        /// <summary>
        /// Encrypt the passed text using the specified secret key.
        /// </summary>
        /// <param name="text">Input Text</param>
        /// <param name="password">Secret Key</param>
        /// <returns>Encrypted Byte Array</returns>
        public static byte[] Encrypt(string text, string password, string iv)
        {
            Preconditions.CheckArgument(text);
            Preconditions.CheckArgument(password);
            Preconditions.CheckArgument(password.Length == 16);

            byte[] pbytes = Encoding.UTF8.GetBytes(password);
            using (Aes algo = Aes.Create())
            {
                algo.Mode = CipherMode.CBC;
                algo.KeySize = 128;
                algo.BlockSize = 128;
                algo.FeedbackSize = 128;
                algo.Padding = PaddingMode.PKCS7;

                algo.Key = pbytes;
                algo.IV = Encoding.UTF8.GetBytes(iv);

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = algo.CreateEncryptor(algo.Key, algo.IV);

                // Create the streams used for encryption.
                using (MemoryStream stream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            //Write all data to the stream.
                            streamWriter.Write(text);
                        }
                        return stream.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// Encrypt the passed text using the specified secret key.
        /// </summary>
        /// <param name="text">Input Text</param>
        /// <param name="password">Secret Key</param>
        /// <returns>Encrypted Base64 String</returns>
        public static string EncryptToString(string text, string password, string iv)
        {
            byte[] encrypted = Encrypt(text, password, iv);
            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// Decrypt the byte array using the specified secret key.
        /// </summary>
        /// <param name="data">Input array</param>
        /// <param name="password">Secret Key</param>
        /// <returns>Decrypted String</returns>
        public static string Decrypt(byte[] data, string password, string iv)
        {
            Preconditions.CheckArgument(data != null && data.Length > 0);
            Preconditions.CheckArgument(password);

            byte[] pbytes = Encoding.UTF8.GetBytes(password);
            // Create an Aes object
            // with the specified key and IV.
            using (Aes algo = Aes.Create())
            {
                algo.Mode = CipherMode.CBC;
                algo.KeySize = 128;
                algo.BlockSize = 128;
                algo.FeedbackSize = 128;
                algo.Padding = PaddingMode.PKCS7;

                algo.Key = pbytes;
                algo.IV = Encoding.UTF8.GetBytes(iv); ;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = algo.CreateDecryptor(algo.Key, algo.IV);

                // Create the streams used for decryption.
                using (MemoryStream stream = new MemoryStream(data))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(stream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader(cryptoStream))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            string output = streamReader.ReadToEnd();
                            if (!String.IsNullOrWhiteSpace(output))
                            {
                                return RemovePadding(output);
                            }
                            return null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Decrypt the string buffer the specified secret key.
        /// </summary>
        /// <param name="data">Input array</param>
        /// <param name="password">Secret Key</param>
        /// <returns>Decrypted String</returns>
        public static string Decrypt(string buffer, string password, string iv)
        {
            byte[] bytes = Convert.FromBase64String(buffer);
            return Decrypt(bytes, password, iv);
        }

        /// <summary>
        /// Method to remove the decryption padding if any.
        /// </summary>
        /// <param name="buffer">Input String buffer to clean.</param>
        /// <returns>Cleaned Buffer</returns>
        private static string RemovePadding(string buffer)
        {
            char[] charBuff = buffer.ToCharArray();
            int sindex = -1;
            for (int ii = 0; ii < buffer.Length; ii++)
            {
                if (charBuff[ii] == '\0')
                {
                    if (sindex < 0)
                    {
                        sindex = ii;
                    }
                }
                else
                {
                    sindex = -1;
                }
            }
            if (sindex >= 0)
            {
                return buffer.Substring(0, sindex);
            }
            return buffer;
        }

        private static RijndaelManaged GetCryptoAlgorithm()
        {
            RijndaelManaged algorithm = new RijndaelManaged();
            //set the mode, padding and block size
            algorithm.Padding = PaddingMode.PKCS7;
            algorithm.Mode = CipherMode.CBC;
            algorithm.KeySize = 128;
            algorithm.BlockSize = 128;
            return algorithm;
        }
    }

    /// <summary>
    /// Local Vault to store configuration passwords in an encrypted form.
    /// 
    /// TODO: Temporary solution for storing passwords in memory. Need to explore 
    /// a better option.
    /// </summary>
    public class ConfigVault
    {
        private Dictionary<string, string> vault = new Dictionary<string, string>();

        /// <summary>
        /// Add the passcode for the specified to the Vault.
        /// </summary>
        /// <param name="config">Configuration Instance.</param>
        /// <param name="passcode">Password</param>
        /// <returns>Self</returns>
        public ConfigVault AddPasscode(Configuration config, string passcode)
        {
            Preconditions.CheckArgument(passcode);
            Preconditions.CheckArgument(config);

            string key = GetEncodingKey(config);
            string iv = GetIvSpec(config);

            string encrypted = CryptoUtils.EncryptToString(passcode, key, iv);
            vault[config.Header.Id] = encrypted;

            return this;
        }

        /// <summary>
        /// Get the password for the specified configuration instance.
        /// </summary>
        /// <param name="config">Configuration instance.</param>
        /// <returns>Password</returns>
        public string GetPasscode(Configuration config)
        {
            Preconditions.CheckArgument(config);
            if (vault.ContainsKey(config.Header.Id))
            {
                string value = vault[config.Header.Id];
                string key = GetEncodingKey(config);
                string iv = GetIvSpec(config);

                return CryptoUtils.Decrypt(value, key, iv);
            }
            return null;
        }

        /// <summary>
        /// Decrypt data for the specified configuration.
        /// </summary>
        /// <param name="data">Data buffer to decrypt</param>
        /// <param name="config">Configuration instance.</param>
        /// <returns>Decrypted data</returns>
        public string Decrypt(string data, Configuration config)
        {
            Preconditions.CheckArgument(config);
            Preconditions.CheckArgument(data);

            string passcode = GetPasscode(config);
            if (String.IsNullOrWhiteSpace(passcode))
            {
                throw new Exception(
                        "Invalid Passcode: NULL/Empty passcode returned.");
            }
            string iv = GetIvSpec(config);

            return CryptoUtils.Decrypt(data, passcode, iv);
        }

        /// <summary>
        /// Generate a Secret Key for the specified configuration.
        /// </summary>
        /// <param name="config">Configuration Instance.</param>
        /// <returns>Secret Key</returns>
        private string GetEncodingKey(Configuration config)
        {
            string key = String.Format("{0}{1}{2}", config.Header.Name,
                                       config.Header.Id,
                                       config.Header.Timestamp);
            int index = (int)(config.Header.Timestamp % 16);

            if (index + 16 >= key.Length)
            {
                index = key.Length - 17;
            }
            return key.Substring(index, 16);
        }

        private string GetIvSpec(Configuration config)
        {
            string key = String.Format("{0}{1}{2}", config.Header.Name,
                                       config.Header.Application,
                                       config.Header.ApplicationGroup);
            
            return key.Substring(0, 16);
        }
    }
}
