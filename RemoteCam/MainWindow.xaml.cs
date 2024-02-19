using RemoteCamReceiver;
using RemoteCamProto;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RemoteCam
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RemoteCamera remoteCamera = new RemoteCamera();

        public MainWindow()
        {
            InitializeComponent();

            remoteCamera.StartListening();
            remoteCamera.StartVirtualCamera();

            remoteCamera.StateChanged += (string state) =>
            {
                Dispatcher.Invoke(() =>
                {
                    Title = state;
                });
            };

            remoteCamera.CamerasUpdated += (cameras) =>
            {
                Dispatcher.Invoke(() =>
                {
                    //cameras = newCameras;
                    Cameras.ItemsSource = cameras.Select(cam => cam.Id);
                    Cameras.SelectedIndex = 0;

                    Resolutions.ItemsSource = cameras[0].Resolutions.Select(res => res.Width + "x" + res.Height);
                    Resolutions.SelectedIndex = 0;
                });
            };
        }

        private void Zoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            remoteCamera.Controller?.SetZoom((float)e.NewValue);
        }

        private void Resolutions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CameraInfo camera = remoteCamera.Cameras[Cameras.SelectedIndex];
            if (Resolutions.SelectedIndex < 0) return;

            remoteCamera.Controller?.SetResolution(camera.Resolutions[Resolutions.SelectedIndex]);
        }

        private void Cameras_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CameraInfo camera = remoteCamera.Cameras[Cameras.SelectedIndex];
            Resolutions.ItemsSource = camera.Resolutions.Select(res => res.Width + "x" + res.Height);
            remoteCamera.Controller?.SwitchCamera(camera.Id);
        }
    }
}