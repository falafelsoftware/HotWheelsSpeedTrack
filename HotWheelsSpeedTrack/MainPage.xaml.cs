using System;
using System.Diagnostics;
using Windows.Devices.Gpio;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace HotWheelsSpeedTrack
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private int _startSensorPin = 18;
        private int _finishSensorPin = 23;
        private GpioPin _startSensor;
        private GpioPin _finishSensor;
        private double _trackLength = 74; //in inches
        private Stopwatch _timer;
        private string _beginMessage = "When you are ready to start, \"Click Ready, Set Go!\"";

        public MainPage()
        {
            this.InitializeComponent();
          
            _timer = new Stopwatch();

            //initialize sensors with internal microcontroller pull up
            GpioController controller = GpioController.GetDefault();
            _startSensor = controller.OpenPin(_startSensorPin);
            _startSensor.SetDriveMode(GpioPinDriveMode.InputPullUp); //use the internal pull up resistor
            _startSensor.ValueChanged += _startSensor_ValueChanged;
            _finishSensor = controller.OpenPin(_finishSensorPin);
            _finishSensor.SetDriveMode(GpioPinDriveMode.InputPullUp); //use internal pull up resistor
            _finishSensor.ValueChanged += _finishSensor_ValueChanged;

            txtStatus.Text = _beginMessage;
        }


        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private async void _startSensor_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (!_timer.IsRunning)
            {
                
                _timer.Start();
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,() => {
                    txtStatus.Text = "Timer Started....";
                    txtResults.Text = "";
                });
               
            }
        
        }

        private async void _finishSensor_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (_timer.IsRunning)
            {
                _timer.Stop();

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                    txtStatus.Text = "Timer Stopped....";
                    
                });
               
                CalculateSpeed();
                
            }
        }
                
        private void Reset()
        {
            _timer.Reset();
            txtStatus.Text = "";
            txtResults.Text = "Ready to begin";
        }

        private async void CalculateSpeed()
        {
            double elapsedTime = _timer.Elapsed.TotalMilliseconds;
            if(elapsedTime > 0)
            {
                double milesConversion = _trackLength / 63360; //convert inches to miles
                double timeConversion = elapsedTime / 3600000; //convert milliseconds to hours
                double result = milesConversion / timeConversion; //calculate MPH
                double displayResult = Math.Round(result, 4);
                _timer.Reset();

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                    txtStatus.Text = "The results are in!";
                    txtResults.Text = String.Format("Your Speed Was: \n{0} M.P.H", displayResult);
                });

            }
        }
    }
}
