using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using RemoteCamProto;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RemoteCamReceiver;

public class FrameDecodingTcpServer : FrameDecoder
{
    public static readonly int HeaderSize = 8;
    private readonly TcpListener _tcpListener = new(IPEndPoint.Parse("0.0.0.0:43921"));

    public delegate void OnClientConnected(EndPoint endpoint);
    public event OnClientConnected ClientConnected;

    public void StartListening()
    {
        _tcpListener.Start();
        Task.Run(() =>
        {
            while (true) AcceptTcpClient();
        });
    }

    private void AcceptTcpClient()
    {
        var client = _tcpListener.AcceptTcpClient();

        ClientConnected?.Invoke(client.Client.RemoteEndPoint);

        Task.Run(() =>
        {
            var stream = client.GetStream();
            var headerBuffer = new byte[HeaderSize];

            while (true)
            {
                if (!ReadBytes(stream, headerBuffer)) return false;

                int id = BitConverter.ToInt32(headerBuffer, 0);
                int dataSize = BitConverter.ToInt32(headerBuffer, 4);
                if(dataSize < 0)
                {
                    return false;
                }

                var dataBuffer = new byte[dataSize];

                if (!ReadBytes(stream, dataBuffer)) return false;

                HandleFrame(id, dataBuffer);
            }
        });
    }

    private bool ReadBytes(NetworkStream stream, byte[] dataBuffer)
    {
        var bytesRead = 0;
        try
        {
            while (bytesRead < dataBuffer.Length)
            {
                var read = stream.Read(dataBuffer, bytesRead, dataBuffer.Length - bytesRead);

                if (read == 0)
                {
                    Console.WriteLine("Connection closed!");
                    return false;
                }

                bytesRead += read;
            }
        }
        catch
        {
            Console.WriteLine("Connection closed!");
            return false;
        }

        return true;
    }
}