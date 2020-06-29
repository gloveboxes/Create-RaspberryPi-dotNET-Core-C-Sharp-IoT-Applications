using System.Threading.Tasks;
using web_app.Interfaces;
using Microsoft.Extensions.Logging;
using Iot.Device.CpuTemperature;
using System;

namespace web_app.Services
{
    public class Device : IDevice
    {
        CpuTemperature _temperature;

        public Device()
        {
            _temperature = new CpuTemperature();
        }

        public double GetCpuTemperature()
        {
            return Math.Round(_temperature.Temperature.Celsius, 2);
        }
    }
}
