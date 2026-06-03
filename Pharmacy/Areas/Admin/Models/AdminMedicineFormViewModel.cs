using Pharmacy.Models;
using System.ComponentModel.DataAnnotations;

namespace Pharmacy.Areas.Admin.Models
{
    public class AdminMedicineFormViewModel
    {
        [Required(ErrorMessage = "Введите название")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите цену")]
        [Range(0.01, 999999, ErrorMessage = "Цена должна быть больше 0")]
        public decimal Price { get; set; }

        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public int? ManufacturerId { get; set; }

        public string? Indications { get; set; }
        public string? Administration { get; set; }
        public string? Composition { get; set; }
        public string? PharmaGroup { get; set; }
        public string? PharmaProps { get; set; }
        public string? Contraindications { get; set; }
        public string? SideEffects { get; set; }
        public string? Overdose { get; set; }
        public string? DrugInteractions { get; set; }
        public string? SpecialInstructions { get; set; }
        public string? DosageForm { get; set; }
        public string? DispensingConditions { get; set; }
        public string? ImageUrl { get; set; }

        [Required]
        public bool PrescriptionRequired { get; set; }

        [Required(ErrorMessage = "Введите количество")]
        [Range(0, 999999, ErrorMessage = "Некорректное количество")]
        public int StockQuantity { get; set; }

        public bool IsActive { get; set; } = true;

        public List<CategoryViewModel> Categories { get; set; } = new();
        public List<BrandViewModel> Brands { get; set; } = new();
        public List<ManufacturerViewModel> Manufacturers { get; set; } = new();
    }
}
