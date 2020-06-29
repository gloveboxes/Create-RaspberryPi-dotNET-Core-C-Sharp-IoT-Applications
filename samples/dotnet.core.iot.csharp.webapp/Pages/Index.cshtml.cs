using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using web_app.Interfaces;

namespace web_app.Pages
{
    public class IndexModel : PageModel
    {
        public double Temperature { get; set; }
        private readonly IDevice _device;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IDevice device, ILogger<IndexModel> logger)
        {
            _logger = logger;
            _device = device;
        }

        public void OnGet()
        {
            Temperature= _device.GetCpuTemperature();
        }
    }
}
