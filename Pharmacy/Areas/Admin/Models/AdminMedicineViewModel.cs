namespace Pharmacy.Areas.Admin.Models
{
    public class AdminMedicineViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? CategoryName { get; set; }
        public string? BrandName { get; set; }
        public string? ManufacturerName { get; set; }
        public bool PrescriptionRequired { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public decimal Rating { get; set; }
    }
}