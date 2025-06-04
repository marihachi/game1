using System.Net;
using System.Net.Sockets;

namespace GameCommon;

public static class NetworkUtilities
{
    public static IPAddress ResolveAddress(string hostname)
    {
        var host = Dns.GetHostEntry(hostname);
        var address = host.AddressList.Where(x => x.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault();
        if (address == null)
        {
            throw new Exception($"Failed to resolve the hostname: {hostname}");
        }
        return address;
    }
}
