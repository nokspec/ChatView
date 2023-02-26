using ChatView.Models;
using ChatView.Models.Contact;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ChatView.Controllers
{
    public class ContactController : Controller
    {
        private readonly ILogger<HomeController> _logger;


        [HttpPost]
        public IActionResult Create(Contact contactForm)
        {
            if (ModelState.IsValid)
            {
                return View("Index");
            }
            return View("Index");
        }

        public ContactController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
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
