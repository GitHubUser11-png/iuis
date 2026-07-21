using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

using IUIS.Application.Abstractions.Security;

namespace IUIS.Infrastructure.Presentation
{
    public sealed class NetworkContextProvider : INetworkContextProvider
    {
        public string[] GetActiveIpv4Addresses()
        {
            try
            {
                return NetworkInterface.GetAllNetworkInterfaces()
                    .Where(item => item.OperationalStatus == OperationalStatus.Up)
                    .SelectMany(item => item.GetIPProperties().UnicastAddresses)
                    .Where(item => item.Address.AddressFamily == AddressFamily.InterNetwork
                        && !IPAddress.IsLoopback(item.Address))
                    .Select(item => item.Address.ToString())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
            catch
            {
                return new string[0];
            }
        }
    }
}
