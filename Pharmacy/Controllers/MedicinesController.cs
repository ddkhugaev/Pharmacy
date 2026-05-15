using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Pharmacy.Models;

namespace Pharmacy.Controllers
{
    public class MedicinesController : Controller
    {
        private readonly string _connectionString;

        public MedicinesController(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IActionResult Index(
            string? searchQuery,
            List<int>? selectedCategories,
            decimal? priceFrom,
            decimal? priceTo,
            bool? prescriptionRequired,
            string sortBy = "popular",
            int page = 1)
        {
            int pageSize = 12;

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            // Загружаем категории для фильтра
            var categories = new List<CategoryViewModel>();
            string categoriesQuery = "SELECT id, name FROM categories;";
            using var categoriesCommand = new MySqlCommand(categoriesQuery, connection);
            using var categoriesReader = categoriesCommand.ExecuteReader();
            while (categoriesReader.Read())
            {
                categories.Add(new CategoryViewModel
                {
                    Id = categoriesReader.GetInt32("id"),
                    Name = categoriesReader.GetString("name")
                });
            }
            categoriesReader.Close();

            // Формируем базовый запрос с фильтрами
            var where = new List<string> { "m.is_active = TRUE" };
            var parameters = new Dictionary<string, object>();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                where.Add("m.name LIKE @search");
                parameters["@search"] = $"%{searchQuery}%";
            }

            if (selectedCategories != null && selectedCategories.Any())
            {
                var ids = string.Join(",", selectedCategories);
                where.Add($"m.category_id IN ({ids})");
            }

            if (priceFrom.HasValue)
            {
                where.Add("m.price >= @priceFrom");
                parameters["@priceFrom"] = priceFrom.Value;
            }

            if (priceTo.HasValue)
            {
                where.Add("m.price <= @priceTo");
                parameters["@priceTo"] = priceTo.Value;
            }

            if (prescriptionRequired.HasValue)
            {
                where.Add("m.prescription_required = @rx");
                parameters["@rx"] = prescriptionRequired.Value;
            }

            string whereClause = "WHERE " + string.Join(" AND ", where);

            // Сортировка
            string orderBy = sortBy switch
            {
                "price_asc" => "m.price ASC",
                "price_desc" => "m.price DESC",
                "rating" => "m.rating DESC",
                "name" => "m.name ASC",
                _ => "m.stock_quantity DESC"
            };

            // Считаем общее количество
            string countQuery = $@"SELECT COUNT(*) FROM medicines m {whereClause};";
            using var countCommand = new MySqlCommand(countQuery, connection);
            foreach (var p in parameters)
                countCommand.Parameters.AddWithValue(p.Key, p.Value);
            int totalCount = Convert.ToInt32(countCommand.ExecuteScalar());
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            int offset = (page - 1) * pageSize;

            // Загружаем лекарства
            string medicinesQuery = $@"SELECT m.id, m.name, m.price, m.rating, m.image_url,
                                              m.prescription_required, m.stock_quantity,
                                              c.name AS category_name,
                                              b.name AS brand_name
                                       FROM medicines m
                                       LEFT JOIN categories c ON m.category_id = c.id
                                       LEFT JOIN brands b ON m.brand_id = b.id
                                       {whereClause}
                                       ORDER BY {orderBy}
                                       LIMIT @pageSize OFFSET @offset;";

            using var medicinesCommand = new MySqlCommand(medicinesQuery, connection);
            foreach (var p in parameters)
                medicinesCommand.Parameters.AddWithValue(p.Key, p.Value);
            medicinesCommand.Parameters.AddWithValue("@pageSize", pageSize);
            medicinesCommand.Parameters.AddWithValue("@offset", offset);

            var medicines = new List<MedicineViewModel>();
            using var medicinesReader = medicinesCommand.ExecuteReader();
            while (medicinesReader.Read())
            {
                medicines.Add(new MedicineViewModel
                {
                    Id = medicinesReader.GetInt32("id"),
                    Name = medicinesReader.GetString("name"),
                    Price = medicinesReader.GetDecimal("price"),
                    Rating = medicinesReader.GetDecimal("rating"),
                    ImageUrl = medicinesReader.IsDBNull(medicinesReader.GetOrdinal("image_url"))
                        ? null
                        : medicinesReader.GetString("image_url"),
                    PrescriptionRequired = medicinesReader.GetBoolean("prescription_required"),
                    StockQuantity = medicinesReader.GetInt32("stock_quantity"),
                    CategoryName = medicinesReader.IsDBNull(medicinesReader.GetOrdinal("category_name"))
                        ? null
                        : medicinesReader.GetString("category_name"),
                    BrandName = medicinesReader.IsDBNull(medicinesReader.GetOrdinal("brand_name"))
                        ? null
                        : medicinesReader.GetString("brand_name")
                });
            }

            var model = new CatalogViewModel
            {
                Medicines = medicines,
                Categories = categories,
                SearchQuery = searchQuery,
                SelectedCategories = selectedCategories ?? new List<int>(),
                PriceFrom = priceFrom,
                PriceTo = priceTo,
                PrescriptionRequired = prescriptionRequired,
                SortBy = sortBy,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                PageSize = pageSize
            };

            return View(model);
        }
    }
}
