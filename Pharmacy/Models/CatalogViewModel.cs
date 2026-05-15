namespace Pharmacy.Models
{
    public class CatalogViewModel
    {
        // Список лекарств на текущей странице
        public List<MedicineViewModel> Medicines { get; set; } = new();

        // Фильтры
        public string? SearchQuery { get; set; }
        public List<int> SelectedCategories { get; set; } = new();
        public decimal? PriceFrom { get; set; }
        public decimal? PriceTo { get; set; }
        public bool? PrescriptionRequired { get; set; }

        // Сортировка
        public string SortBy { get; set; } = "popular";

        // Пагинация
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 12;

        // Данные для фильтров
        public List<CategoryViewModel> Categories { get; set; } = new();
    }

    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
