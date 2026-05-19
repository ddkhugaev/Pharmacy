namespace Pharmacy.Models
{
    public class MedicineDetailViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Rating { get; set; }
        public string? ImageUrl { get; set; }
        public bool PrescriptionRequired { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }

        // Связанные данные
        public string? CategoryName { get; set; }
        public string? BrandName { get; set; }
        public string? ManufacturerName { get; set; }
        public string? CountryName { get; set; }

        // Действующие вещества
        public List<ActiveIngredientViewModel> ActiveIngredients { get; set; } = new();

        // Описание - аккордеон
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
    }

    public class ActiveIngredientViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Quantity { get; set; }
    }
}
