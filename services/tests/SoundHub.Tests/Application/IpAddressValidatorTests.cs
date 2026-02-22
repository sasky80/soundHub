using SoundHub.Application.Services;

namespace SoundHub.Tests.Application;

public class IpAddressValidatorTests
{
    [Theory]
    [InlineData("10.0.0.5")]
    [InlineData("172.16.0.1")]
    [InlineData("172.31.255.254")]
    [InlineData("192.168.1.10")]
    public void IsAllowedLanAddress_PrivateIpv4_ReturnsTrue(string ip)
    {
        var result = IpAddressValidator.IsAllowedLanAddress(ip);

        Assert.True(result);
    }

    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("169.254.169.254")]
    [InlineData("169.254.1.1")]
    [InlineData("8.8.8.8")]
    [InlineData("::1")]
    [InlineData("fe80::1")]
    [InlineData("not-an-ip")]
    public void IsAllowedLanAddress_NonLanOrInvalid_ReturnsFalse(string ip)
    {
        var result = IpAddressValidator.IsAllowedLanAddress(ip);

        Assert.False(result);
    }
}