using System.ComponentModel.DataAnnotations;

namespace Pharmacy.Areas.Admin.Models
{
    public class BrandViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название")]
        public string Name { get; set; } = string.Empty;
    }
}