using System.ComponentModel.DataAnnotations;

namespace ChatView.Models.Contact
{
    public class Contact
    {
        [Required(ErrorMessage = "Please enter a topic")]
        public string Topic { get; set; }

        [Required(ErrorMessage = "Please enter your e-mail address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter your message")]
        public string Message { get; set; }

        public string CaptchaToken { get; set; }
    }
}
