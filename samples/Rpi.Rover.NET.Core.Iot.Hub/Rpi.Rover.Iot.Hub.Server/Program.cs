using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace Rpi.Rover.Server
{
    class Program
    {
        enum MotorMap : byte { TwoPlus = 21, TwoMinus = 26, OnePlus = 19, OneMinus = 20 }
        enum MotorControl { Stop, Forward, LeftForward, RightForward, Backward, SharpLeft, SharpRight }

        private static GpioController controller = new GpioController();
        private static Motor left = new Motor(controller, (int)MotorMap.TwoMinus, (int)MotorMap.TwoPlus);
        private static Motor right = new Motor(controller, (int)MotorMap.OneMinus, (int)MotorMap.OnePlus);

        private static Action[][] direction = new Action[][]{
            new Action[] { left.Stop, right.Stop },         // stop
            new Action[] { left.Forward, right.Forward },   // forward
            new Action[] { left.Stop, right.Forward },      // left
            new Action[] { left.Forward, right.Stop },      // right
            new Action[] { left.Backward, right.Backward},  // backwards
            new Action[] { left.Forward, right.Backward },  // left circle
            new Action[] { left.Backward, right.Forward },  // right circle
        };

        private static string deviceConnectionString = Environment.GetEnvironmentVariable("IOT_HUB_CONNECTION_STRING");
        private static TransportType transportType = TransportType.Mqtt;

        private class Control
        {
            public string Direction { get; set; }
        }

        static async Task Main(string[] args)
        {
            using (var deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, transportType))
            {
                await deviceClient.SetMethodHandlerAsync("Direction", SetDirectionAsync, null).ConfigureAwait(false);
                Thread.Sleep(Timeout.Infinite);
            }
        }

        static private Task<MethodResponse> SetDirectionAsync(MethodRequest methodRequest, object userContext)
        {
            MotorControl control;

            Control command = JsonConvert.DeserializeObject<Control>(methodRequest.DataAsJson);

            if (command != null && Enum.TryParse(command.Direction, true, out control))
            {
                direction[(int)control][0]();
                direction[(int)control][1]();

                return Task.FromResult(new MethodResponse(new byte[0], 200));
            }

            return Task.FromResult(new MethodResponse(new byte[0], 500));
        }
    }
}
