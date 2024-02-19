using Grpc.Net.Client;
using Remotecam;
using Remotecam.Remotecontrol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RemoteCamReceiver
{
    internal class RemoteController
    {
        private readonly RemoteControl.RemoteControlClient _client;
        public RemoteController(EndPoint endpoint)
        {
            var remoteIpEndPoint = endpoint as IPEndPoint;
            var channel = GrpcChannel.ForAddress("http://" + remoteIpEndPoint.Address.ToString() + ":43924");
            _client = new RemoteControl.RemoteControlClient(channel);
        }

        public void SetZoom(float value)
        {
            _client.SetZoom(new SettingValue { Value = value });
        }

        public List<CameraInfo> GetCameras()
        {
            return _client.GetCameras(new Empty()).Cameras.ToList();
        }

        public void SwitchCamera(string id)
        {
            _client.SwitchCamera(new CameraId() { Id = id });
        }

        public void SetResolution(Size resolution)
        {
            _client.SetResolution(resolution);
        }
    }
}
