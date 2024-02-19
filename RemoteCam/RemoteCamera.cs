using RemoteCamGrpc;
using RemoteCamProto;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace RemoteCamReceiver;

internal unsafe class RemoteCamera
{
    private int _currentWidth, _currentHeight;

    public delegate void OnCamerasUpdated(List<CameraInfo> cameras);
    public event OnCamerasUpdated? CamerasUpdated;

    private readonly FrameDecodingTcpServer _server = new FrameDecodingTcpServer();
    public RemoteController? Controller;

    private IntPtr _virtualCamera = IntPtr.Zero;
    private CameraBroadcaster _broadcaster = new();
    private List<CameraInfo> cameras = new List<CameraInfo>();
    public List<CameraInfo> Cameras
    {
        get => cameras;
        set
        {
            cameras = value;
            CamerasUpdated?.Invoke(cameras);
        }
    }

    [DllImport("Resources/softcam.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr scCreateCamera(int width, int height, float framerate);

    [DllImport("Resources/softcam.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void scDeleteCamera(IntPtr camera);

    [DllImport("Resources/softcam.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void scSendFrame(IntPtr camera, byte* imageBits);

    public delegate void OnStateChanged(string state);
    public event OnStateChanged StateChanged;

    public void StartVirtualCamera()
    {
        Process.Start("Resources/softcam_installer.exe", "register Resources/softcam.dll");
        Process.Start("Resources/softcam_installer32.exe", "register Resources/softcam32.dll");
        Task.Run(() =>
        {
            while (true) DrawFrame();
        });
    }

    private void InitCamera()
    {
        if (_virtualCamera != IntPtr.Zero) scDeleteCamera(_virtualCamera);

        _virtualCamera = scCreateCamera(_currentWidth, _currentHeight, 30);
        if (_virtualCamera == IntPtr.Zero)
        {
            Console.WriteLine("Error creating camera!");
            return;
        }

        StateChanged?.Invoke(string.Format("Running at {0}x{1}", _currentWidth, _currentHeight));
        Console.WriteLine("Running at {0}x{1}", _currentWidth, _currentHeight);
    }

    private unsafe void DrawFrame()
    {
        var frame = _server.GetNextFrame();
        if (frame == null)
        {
            Thread.Sleep(10);
            return;
        }

        if (_currentWidth != frame.Width || _currentHeight != frame.Height || _virtualCamera == IntPtr.Zero)
        {
            _currentWidth = frame.Width;
            _currentHeight = frame.Height;

            InitCamera();
        }

        fixed (byte* data = frame.Data)
        {
            scSendFrame(_virtualCamera, data);
        }
    }

    public void StartListening()
    {
        _broadcaster.StartBroadcast();
        _server.StartListening();
        _server.ClientConnected += (endpoint) =>
        {
            var controller = new RemoteController(endpoint);
            try
            {
                Cameras = controller.GetCameras();
            }
            catch { }

            Controller = controller;
        };
    }
}