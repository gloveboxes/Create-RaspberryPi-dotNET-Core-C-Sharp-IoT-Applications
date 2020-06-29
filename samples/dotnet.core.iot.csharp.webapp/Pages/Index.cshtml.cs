using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Iot.Device.CpuTemperature;


namespace web_app.Pages
{
    public class IndexModel : PageModel
    {

        public double Temperature { get; set; }
        CpuTemperature _temperature;

        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
            _temperature = new CpuTemperature();
        }

        public void OnGet()
        {
            Temperature = Math.Round(_temperature.Temperature.Celsius, 2);
        }
    }
}
