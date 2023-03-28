using ChatView_API.DAL;
using ChatView_API.Models.ChatView;
using ChatView_API.Models.Db_Models;
using ChatView_API.Models.ReCaptcha;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace ChatView_API.Controllers
{
    [ApiController]
    [Route("/api/contact/submit")]
    public class ContactController : ControllerBase
    {
        private readonly ChatViewDbContext _context;
        public ContactController(ChatViewDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitContactForm([FromBody] Contact contactForm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ReCaptchaValidationResult recaptchaResult = IsValid(contactForm.CaptchaToken);

            if (recaptchaResult.Success) //validate the captcha token before proceeding.
            {
                var webhookUrl = "https://discord.com/api/webhooks/1090244934658969651/dp9n0BIwIElPWTaax8Jea343gig_VYDoYwDat6btIszro75BqC8G8mHWVEqixDiiPoGq";
                var payload = new
                {
                    embeds = new[]
                    {
                    new
                    {
                        title = "New Contact Form Submission",
                        description = $"**Topic:** {contactForm.Topic}\n**Email:** {contactForm.Email}\n**Message:** {contactForm.Message}"
                    }
                }
                };
                var httpClient = new HttpClient();
                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(webhookUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    DbContact _dbcontact = new DbContact
                    {
                        Topic = contactForm.Topic,
                        Email = contactForm.Email,
                        Message = contactForm.Message
                    };

                    _context.ContactForms.Add(_dbcontact);
                    await _context.SaveChangesAsync();
                    return Ok(); //200
                }
                return BadRequest();
            }
            return BadRequest();
        }

        /// <summary>
        /// Validate if the captcha token is valid.
        /// </summary>
        /// <param name="captchaResponse"></param>
        /// <returns></returns>
        public static ReCaptchaValidationResult IsValid(string captchaResponse)
        {
            if (string.IsNullOrWhiteSpace(captchaResponse))
            {
                return new ReCaptchaValidationResult()
                { Success = false };
            }

            HttpClient client = new()
            {
                BaseAddress = new Uri("https://www.google.com")
            };

            List<KeyValuePair<string, string>> values = new()
            {
                new KeyValuePair<string, string>("secret", "6LfnNYokAAAAAJxDKtAoXR5g0N5CjzM03b7jDD8e"),
                new KeyValuePair<string, string>("response", captchaResponse)
            };

            FormUrlEncodedContent content = new(values);

            HttpResponseMessage response = client.PostAsync("/recaptcha/api/siteverify", content).Result;

            string verificationResponse = response.Content.ReadAsStringAsync().Result;

            var verificationResult = JsonConvert.DeserializeObject<ReCaptchaValidationResult>(verificationResponse);

            return verificationResult;
        }
    }
}
