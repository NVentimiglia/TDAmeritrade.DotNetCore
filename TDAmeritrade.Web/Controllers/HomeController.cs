using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using System.Threading.Tasks;
using TDAmeritrade.Web.Models;

namespace TDAmeritrade.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TDAmeritradeClient _client;

        public HomeController(ILogger<HomeController> logger, TDAmeritradeClient client)
        {
            _logger = logger;
            _client = client;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RequestAccessToken(string consumerKey)
        {
            var path = _client.GetSignInUrl(consumerKey);
            _logger.LogInformation(path);
            return Redirect(path);
        }

        public async Task<IActionResult> PostAccessToken(string consumerKey, string code)
        {
            await _client.SignIn(consumerKey, code);
            return View("Index");
        }


        public async Task<IActionResult> Quote(string symbol)
        {
            if (!_client.IsSignedIn)
            {
                await _client.SignIn();
            }
            var data = await _client.GetQuoteJson(symbol);
            return Content(data);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
