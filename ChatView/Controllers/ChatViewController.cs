using Microsoft.AspNetCore.Mvc;

namespace ChatView.Controllers
{
    public class ChatViewController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
