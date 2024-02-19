using Remotecam;
using System;
using Terminal.Gui;
using Remotecam.Remotecontrol;
using System.Runtime.CompilerServices;

namespace RemoteCamReceiver;

public class Program
{
    [STAThread]
    private static void Main()
    {
        Application.Init();
        var remoteCam = new RemoteCamera();
        var label = new Label("Disconnected");
        remoteCam.StateChanged += (string state) =>
        {
            Application.Invoke(() =>
            {
                label.Text = state;
            });
        };

        remoteCam.StartListening();
        remoteCam.StartVirtualCamera();

        var top = new Toplevel()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        var menu = new MenuBar(new MenuBarItem[] {
            new MenuBarItem ("_Application", new MenuItem [] {
                new MenuItem ("_Exit", "", () => {
                    Application.RequestStop();
                })
            }),
        });


        label.Y = Pos.Bottom(menu);

        List<object> zoomValues = new();
        for (double i = 1; i <= 10; i += .5)
        {
            zoomValues.Add(i);
        }
        var zoomSlider = new Slider(zoomValues)
        {
            Title = "Zoom",
            X = 0,
            Y = Pos.Bottom(label),
            Width = Dim.Fill(),
            Type = SliderType.Single,
            BorderStyle = LineStyle.Dashed,
            AllowEmpty = false,
            AutoSize = false,
            ShowLegends = true,
            InnerSpacing = 3,
            ShowSpacing = true,
        };
        zoomSlider.OptionsChanged += (s, e) =>
        {
            remoteCam.Controller?.SetZoom((float)(double)e.Options.First().Value.Data);
        };

        var cameraComboBox = new ComboBox
        {
            X = 0,
            Y = Pos.Bottom(zoomSlider) + 1,
            HideDropdownListOnClick = true,

            Width = Dim.Percent(40),
        };
        var resolutionComboBox = new ComboBox
        {
            X = 0,
            Y = Pos.Bottom(cameraComboBox) + 1,
            HideDropdownListOnClick = true,
            
            Height = 2,
            Width = Dim.Percent(40),
        };
        List<CameraInfo> cameras = new List<CameraInfo>();
        cameraComboBox.SelectedItemChanged += (s, e) =>
        {
            if (e.Item < 0) return;

            remoteCam.Controller?.SwitchCamera(cameras[e.Item].Id);
            resolutionComboBox.SetSource(cameras[e.Item].Resolutions);
        };
        resolutionComboBox.SelectedItemChanged += (s, e) =>
        {
            if (e.Item < 0) return;

            remoteCam.Controller?.SetResolution(cameras[cameraComboBox.SelectedItem].Resolutions[e.Item]);
        };

        remoteCam.CamerasUpdated += (newCameras) =>
        {
            Application.Invoke(() =>
            {
                cameras = newCameras;
                cameraComboBox.SetSource(newCameras.Select(cam => cam.Id).ToList());
            });
        };

        top.Add(menu, label, zoomSlider, cameraComboBox, resolutionComboBox);



        Application.Run(top);
    }
}