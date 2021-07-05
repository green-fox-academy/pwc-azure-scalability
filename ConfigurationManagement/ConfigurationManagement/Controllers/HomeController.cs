using ConfigurationManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ConfigurationManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        //private readonly IConfigurationRefresher _configurationRefresher;
        private readonly IFeatureManager _featureManager;

        public HomeController(
            ILogger<HomeController> logger,
            IConfiguration configuration,
            //IConfigurationRefresher configurationRefresher,
            IFeatureManager featureManager)
        {
            _logger = logger;
            _configuration = configuration;
            //_configurationRefresher = configurationRefresher;
            _featureManager = featureManager;
        }

        public async Task<IActionResult> Index()
        {
            //await _configurationRefresher.RefreshAsync();
            var isExtraMessage = await _featureManager.IsEnabledAsync("IsExtraMessage");
            var isSuperExtraMessage = await _featureManager.IsEnabledAsync("IsSuperExtraMessage");
            var secretValue = _configuration["Secret"];

            return View(
                new HomeViewModel
                {
                    WelcomeMessage = _configuration["PWC:Message"],
                    IsExtraMessage = isExtraMessage,
                    IsSuperExtraMessage = isSuperExtraMessage,
                });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
