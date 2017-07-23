using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Core;
using Windows.Devices.Sensors;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas;

namespace App1 {
    public sealed partial class MainPage : Page {
        private Accelerometer _accelerometer;
        private Render render;

        private async void ReadingChanged(object sender, AccelerometerReadingChangedEventArgs e) {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                AccelerometerReading reading = e.Reading;
                //render.pointer.pitch = (float)reading.AccelerationY;
                render.roll = (float)reading.AccelerationX;
            });
        }

        public MainPage() {
            this.InitializeComponent();
        }

        private void canvas_Loaded(object sender, RoutedEventArgs e) {

        }

        private void canvas_CreateResources(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args) {
            render = new Render(sender);
            _accelerometer = Accelerometer.GetDefault();

            if (_accelerometer != null) {
                uint minReportInterval = _accelerometer.MinimumReportInterval;
                uint reportInterval = minReportInterval > 16 ? minReportInterval : 16;
                _accelerometer.ReportInterval = reportInterval;

                _accelerometer.ReadingChanged += new TypedEventHandler<Accelerometer, AccelerometerReadingChangedEventArgs>(ReadingChanged);
            }
        }

        private void canvas_DrawAnimated(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args) {
            CanvasDrawingSession d = args.DrawingSession;
            render.draw(sender, d);
        }
    }
}
