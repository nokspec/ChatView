using Microsoft.AspNetCore.Mvc;

namespace ChatView.Controllers
{
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
