namespace Pharmacy.Models
{
    public class MedicineViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? CategoryName { get; set; }
        public string? BrandName { get; set; }
        public decimal Rating { get; set; }
        public string? ImageUrl { get; set; }
        public bool PrescriptionRequired { get; set; }
        public int StockQuantity { get; set; }
    }
}
