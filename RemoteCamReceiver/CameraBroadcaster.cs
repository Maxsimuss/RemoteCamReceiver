using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteCamReceiver;

public class CameraBroadcaster
{
    public void StartBroadcast()
    {
        int port = 43922;
        IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, port);
        
        var data = "RemoteCam"u8.ToArray();

        Task.Run(() =>
        {
            while (true)
            {
                UdpClient udpSend = new UdpClient(port);
                udpSend.EnableBroadcast = true;
                udpSend.Send(data, new IPEndPoint(IPAddress.Broadcast, port));
                udpSend.Close();
                foreach (var item in GetEndpoints())
                {
                    udpSend = new UdpClient(new IPEndPoint(item, port));
                    udpSend.EnableBroadcast = true;
                    udpSend.Send(data, new IPEndPoint(IPAddress.Broadcast, port));
                    udpSend.Close();
                }

                Thread.Sleep(1000);
            }
        });
    }
    
    private List<IPAddress> GetEndpoints()
    {
        List<IPAddress> AddressList = new List<IPAddress>();
        NetworkInterface[] Interfaces = NetworkInterface.GetAllNetworkInterfaces();
        foreach (NetworkInterface I in Interfaces)
        {
            if (I.OperationalStatus == OperationalStatus.Up)
            {
                foreach (var Unicast in I.GetIPProperties().UnicastAddresses)
                {
                    if (Unicast.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        AddressList.Add(Unicast.Address);
                    }
                }
            }
        }
        return AddressList;
    }
}