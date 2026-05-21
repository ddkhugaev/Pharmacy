using System.ComponentModel.DataAnnotations;

namespace Pharmacy.Models
{
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Введите имя")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите фамилию")]
        public string LastName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Некорректный номер телефона")]
        public string? Phone { get; set; }
    }
}
