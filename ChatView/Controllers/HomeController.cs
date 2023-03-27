using ChatView.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ChatView.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private const string PageViewCount = "PageViewCount";

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            UpdatePageViewCookie();
            ViewBag.PageViews = Request.Cookies[PageViewCount];
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public void UpdatePageViewCookie()
        {
            var currentCookieValue = Request.Cookies[PageViewCount];

            if (currentCookieValue == null)
            {
                Response.Cookies.Append(PageViewCount, "1");
            }
            else
            {
                var newCookieValue = short.Parse(currentCookieValue) + 1;

                Response.Cookies.Append(PageViewCount, newCookieValue.ToString());
            }
        }
    }
}