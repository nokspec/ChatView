using ChatView.Models;
using ChatView.Models.Contact;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System.Text;

namespace ChatView.Controllers
{
    public class ContactController : Controller
    {
        private readonly HttpClient _httpClient;

        public ContactController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Contact contactForm)
        {
            //at this point the object is valid because we have added requirements in the Contact class.
            contactForm.CaptchaToken = HttpContext.Request.Form["g-Recaptcha-Response"]; //get the captcha token
            var apiUrl = "http://localhost:5134/api/contact/submit";
            var payload = new Contact
            {
                Topic = contactForm.Topic,
                Email = contactForm.Email,
                Message = contactForm.Message,
                CaptchaToken = contactForm.CaptchaToken
            };

            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode) return View("Success");
            return View("Index");
        }
    }
}
