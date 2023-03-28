using System.ComponentModel.DataAnnotations;

namespace ChatView_API.Models.ChatView
{
    public class Contact
    {
        [Required, MaxLength(200)]
        public string Topic { get; set; }
        [Required]
        public string Email { get; set; }
        [Required, MaxLength(600)]
        public string Message { get; set; }

        public string CaptchaToken { get; set; }
    }
}
