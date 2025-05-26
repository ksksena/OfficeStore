using System.ComponentModel.DataAnnotations;

namespace OfficeStore.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Поле Email обязательно")]
        [EmailAddress(ErrorMessage = "Некорректный email")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Поле Пароль обязательно")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Display(Name = "Запомнить меня")]
        public bool RememberMe { get; set; }
    }
}
