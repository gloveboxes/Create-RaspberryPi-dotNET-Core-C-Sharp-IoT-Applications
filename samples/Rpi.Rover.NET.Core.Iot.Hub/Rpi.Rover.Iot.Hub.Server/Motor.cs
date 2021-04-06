using System.Device.Gpio;

namespace Rpi.Rover.Server
{
    public class Motor
    {
        GpioController _controller;

        private readonly int _in1;
        private readonly int _in2;

        public Motor(GpioController controller, byte in1, byte in2)
        {
            this._controller = controller;
            this._in1 = in1;
            this._in2 = in2;

            controller.OpenPin(in1, PinMode.Output);
            controller.OpenPin(in2, PinMode.Output);

            controller.Write(in1, PinValue.Low);
            controller.Write(in2, PinValue.Low);
        }

        public void Forward()
        {
            _controller.Write(_in1, PinValue.Low);
            _controller.Write(_in2, PinValue.High);
        }

        public void Backward()
        {
            _controller.Write(_in1, PinValue.High);
            _controller.Write(_in2, PinValue.Low);
        }

        public void Stop()
        {
            _controller.Write(_in1, PinValue.Low);
            _controller.Write(_in2, PinValue.Low);
        }
    }
}