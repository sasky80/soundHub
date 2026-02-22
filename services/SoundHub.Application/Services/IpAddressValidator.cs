using System.Net;

namespace SoundHub.Application.Services;

public static class IpAddressValidator
{
    public static bool IsAllowedLanAddress(string ipAddress)
    {
        if (!IPAddress.TryParse(ipAddress, out var parsedIp))
        {
            return false;
        }

        if (parsedIp.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
        {
            return false;
        }

        if (IPAddress.IsLoopback(parsedIp))
        {
            return false;
        }

        var bytes = parsedIp.GetAddressBytes();

        return bytes[0] == 10 ||
               (bytes[0] == 172 && bytes[1] is >= 16 and <= 31) ||
               (bytes[0] == 192 && bytes[1] == 168);
    }
}