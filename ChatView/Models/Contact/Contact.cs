    using System.ComponentModel.DataAnnotations;

namespace ChatView.Models.Contact
{
    public class Contact
    {
        [Required(ErrorMessage = "Please enter a topic"), MaxLength(200, ErrorMessage = "Max length is 200 characters")]
        public string Topic { get; set; }

        [Required(ErrorMessage = "Please enter your e-mail address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter your message"), MaxLength(600, ErrorMessage = "Max length is 600 characters")]
        public string Message { get; set; }

        public string CaptchaToken { get; set; }
    }
}
