using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Panel = Windows.Devices.Enumeration.Panel;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CameraLiveView.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private MediaCapture captureSource;

        private bool isStarted;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            // 背面とか前面とかは適当にこの辺で絞る
            // using Panel = Windows.Devices.Enumeration.Panel;
            var camera = devices.FirstOrDefault(d => d.EnclosureLocation?.Panel == Panel.Front);

            var settings = new MediaCaptureInitializationSettings {VideoDeviceId = camera.Id};
            captureSource = new MediaCapture();
            await captureSource.InitializeAsync(settings);

            LiveView.Source = captureSource;
        }

        private async void StartLiveView(object sender, RoutedEventArgs e)
        {
            if (isStarted) return;

            await captureSource.StartPreviewAsync();
            isStarted = true;
        }

        private async void StopLiveView(object sender, RoutedEventArgs e)
        {
            if (!isStarted) return;

            await captureSource.StopPreviewAsync();
            isStarted = false;
        }

        private async void CaptureImage(object sender, RoutedEventArgs e)
        {
            if (!isStarted) return;

            var fn = $"{DateTime.Now:yyyyMMdd-HHmmss}.jpg";
            var file = await KnownFolders.CameraRoll.CreateFileAsync(fn, CreationCollisionOption.GenerateUniqueName);
            await captureSource.CapturePhotoToStorageFileAsync(ImageEncodingProperties.CreateJpeg(), file);
        }

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            if (isStarted)
                await captureSource.StopPreviewAsync();
            captureSource.Dispose();
        }
    }
}
