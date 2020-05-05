using System;
using ExhaustiveMatching;

[Closed(typeof(IPv4Address), typeof(IPv6Address))]
abstract class IPAddress
{
}

class IPv4Address : IPAddress
{
    public IPv6Address MapToIPv6()
    {
        throw new NotImplementedException();
    }
}

class IPv6Address : IPAddress
{

}

class IPAddressExample
{
    IPv6Address Example(IPAddress ipAddress)
    {
        switch (ipAddress)
        {
            default:
                throw ExhaustiveMatch.Failed(ipAddress);
            case IPv4Address ipv4Address:
                return ipv4Address.MapToIPv6();
            #region Snip
            case IPv6Address ipv6Address:
                return ipv6Address;
            #endregion
        }
    }
}