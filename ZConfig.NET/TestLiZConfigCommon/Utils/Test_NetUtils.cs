using System;
using Xunit;

namespace LibZConfig.Common.Utils
{
    public class Test_NetUtils
    {
        [Fact]
        public void GetIpAddress()
        {
            try
            {
                string ip = NetUtils.GetIpAddress();
                Assert.False(String.IsNullOrWhiteSpace(ip));
                LogUtils.Debug(String.Format("IP Address = [{0}]", ip));
            }
            catch (Exception ex)
            {
                LogUtils.Error(ex);
                throw ex;
            }
        }

        [Fact]
        public void GetIpV6Address()
        {
            try
            {
                string ip = NetUtils.GetIpAddress(System.Net.Sockets.AddressFamily.InterNetworkV6);
                Assert.False(String.IsNullOrWhiteSpace(ip));
                LogUtils.Debug(String.Format("IP V6 Address = [{0}]", ip));
            }
            catch (Exception ex)
            {
                LogUtils.Error(ex);
                throw ex;
            }
        }

        [Fact]
        public void GetHostName()
        {
            try
            {
                string host = NetUtils.GetHostName();
                Assert.False(String.IsNullOrWhiteSpace(host));
                LogUtils.Debug(String.Format("IP Address = [{0}]", host));
            }
            catch (Exception ex)
            {
                LogUtils.Error(ex);
                throw ex;
            }
        }
    }
}
