using System;
using Xunit;
using System.Collections.Generic;
using System.Text;

namespace LibZConfig.Common.Utils
{
    public class Test_CryptoUtils
    {
        [Fact]
        public void EncryptionBytes()
        {
            try
            {
                string text = "Then I use System.Security.Cryptography library and calculate it. But it gives me different result. Could you help me for that situation?";
                string password = Guid.NewGuid().ToString().Substring(0, 16);
                string iv = Guid.NewGuid().ToString().Substring(0, 16);

                byte[] encrypted = CryptoUtils.Encrypt(text, password, iv);
                Assert.NotNull(encrypted);
                Assert.True(encrypted.Length > 0);

                LogUtils.Debug(String.Format("Encrypted: [{0}]", encrypted));

                string decrypted = CryptoUtils.Decrypt(encrypted, password, iv);
                Assert.NotNull(decrypted);
                Assert.False(String.IsNullOrWhiteSpace(decrypted));
                Assert.Equal(text, decrypted);

                LogUtils.Debug(String.Format("Decrypted: [{0}]", decrypted));
            }
            catch (Exception ex)
            {
                LogUtils.Error(ex);
                throw ex;
            }
        }

        [Fact]
        public void EncryptionString()
        {
            try
            {
                string text = "TEST-PARAM-3";
                string password = "21947a50-6755-47";
                string iv = "test-configTEST-";

                string encrypted = CryptoUtils.EncryptToString(text, password, iv);
                Assert.NotNull(encrypted);
                Assert.True(encrypted.Length > 0);

                LogUtils.Debug(String.Format("Encrypted: [{0}]", encrypted));

                string decrypted = CryptoUtils.Decrypt(encrypted, password, iv);
                Assert.NotNull(decrypted);
                Assert.False(String.IsNullOrWhiteSpace(decrypted));
                Assert.Equal(text, decrypted);

                LogUtils.Debug(String.Format("Decrypted: [{0}]", decrypted));
            }
            catch (Exception ex)
            {
                LogUtils.Error(ex);
                throw ex;
            }
        }
    }
}
