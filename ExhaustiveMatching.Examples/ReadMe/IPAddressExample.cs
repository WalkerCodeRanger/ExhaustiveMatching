using System;
using ExhaustiveMatching;

// ReSharper disable All

namespace Examples.ReadMe
{
    public static class IPAddressExample
    {
        #region snippet
        [Closed(typeof(IPv4Address), typeof(IPv6Address))]
        public abstract class IPAddress { /* … */ }

        public class IPv4Address : IPAddress { /* … */ }

        public class IPv6Address : IPAddress { /* … */ }
        #endregion

        public static IPv6Address Example(IPAddress ipAddress)
        {
            #region snippet
            // EM0003: Switch on Closed Type Not Exhaustive
            // ERROR Subtype not handled by switch: IPv6Address
            switch (ipAddress)
            {
                default:
                    throw ExhaustiveMatch.Failed(ipAddress);
                case IPv4Address ipv4Address:
                    return ipv4Address.MapToIPv6();
            }
            #endregion
        }

        // Use extension method to avoid putting a method in the body
        private static IPv6Address MapToIPv6(this IPv4Address address)
        {
            throw new NotImplementedException();
        }
    }
}
