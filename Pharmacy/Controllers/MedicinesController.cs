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

            var favoriteIds = new HashSet<int>();
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirst("id")?.Value;
                string favQuery = "SELECT medicine_id FROM favorite_items WHERE user_id = @userId;";
                using var favCommand = new MySqlCommand(favQuery, connection);
                favCommand.Parameters.AddWithValue("@userId", userId);
                using var favReader = favCommand.ExecuteReader();
                while (favReader.Read())
                    favoriteIds.Add(favReader.GetInt32("medicine_id"));
            }

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
                        : medicinesReader.GetString("brand_name"),
                    IsFavorite = favoriteIds.Contains(medicinesReader.GetInt32("id"))
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

        public IActionResult Details(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string query = @"SELECT m.id, m.name, m.price, m.rating, m.image_url,
                            m.prescription_required, m.stock_quantity, m.is_active,
                            m.indications, m.administration, m.composition,
                            m.pharma_group, m.pharma_props, m.contraindications,
                            m.side_effects, m.overdose, m.drug_interactions,
                            m.special_instructions, m.dosage_form, m.dispensing_conditions,
                            c.name AS category_name,
                            b.name AS brand_name,
                            mf.name AS manufacturer_name,
                            co.name AS country_name
                     FROM medicines m
                     LEFT JOIN categories c ON m.category_id = c.id
                     LEFT JOIN brands b ON m.brand_id = b.id
                     LEFT JOIN manufacturers mf ON m.manufacturer_id = mf.id
                     LEFT JOIN countries co ON mf.country_id = co.id
                     WHERE m.id = @id AND m.is_active = TRUE;";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();

            if (!reader.Read())
                return NotFound();

            var model = new MedicineDetailViewModel
            {
                Id = reader.GetInt32("id"),
                Name = reader.GetString("name"),
                Price = reader.GetDecimal("price"),
                Rating = reader.GetDecimal("rating"),
                ImageUrl = reader.IsDBNull(reader.GetOrdinal("image_url")) ? null : reader.GetString("image_url"),
                PrescriptionRequired = reader.GetBoolean("prescription_required"),
                StockQuantity = reader.GetInt32("stock_quantity"),
                IsActive = reader.GetBoolean("is_active"),
                CategoryName = reader.IsDBNull(reader.GetOrdinal("category_name")) ? null : reader.GetString("category_name"),
                BrandName = reader.IsDBNull(reader.GetOrdinal("brand_name")) ? null : reader.GetString("brand_name"),
                ManufacturerName = reader.IsDBNull(reader.GetOrdinal("manufacturer_name")) ? null : reader.GetString("manufacturer_name"),
                CountryName = reader.IsDBNull(reader.GetOrdinal("country_name")) ? null : reader.GetString("country_name"),
                Indications = reader.IsDBNull(reader.GetOrdinal("indications")) ? null : reader.GetString("indications"),
                Administration = reader.IsDBNull(reader.GetOrdinal("administration")) ? null : reader.GetString("administration"),
                Composition = reader.IsDBNull(reader.GetOrdinal("composition")) ? null : reader.GetString("composition"),
                PharmaGroup = reader.IsDBNull(reader.GetOrdinal("pharma_group")) ? null : reader.GetString("pharma_group"),
                PharmaProps = reader.IsDBNull(reader.GetOrdinal("pharma_props")) ? null : reader.GetString("pharma_props"),
                Contraindications = reader.IsDBNull(reader.GetOrdinal("contraindications")) ? null : reader.GetString("contraindications"),
                SideEffects = reader.IsDBNull(reader.GetOrdinal("side_effects")) ? null : reader.GetString("side_effects"),
                Overdose = reader.IsDBNull(reader.GetOrdinal("overdose")) ? null : reader.GetString("overdose"),
                DrugInteractions = reader.IsDBNull(reader.GetOrdinal("drug_interactions")) ? null : reader.GetString("drug_interactions"),
                SpecialInstructions = reader.IsDBNull(reader.GetOrdinal("special_instructions")) ? null : reader.GetString("special_instructions"),
                DosageForm = reader.IsDBNull(reader.GetOrdinal("dosage_form")) ? null : reader.GetString("dosage_form"),
                DispensingConditions = reader.IsDBNull(reader.GetOrdinal("dispensing_conditions")) ? null : reader.GetString("dispensing_conditions"),
            };
            reader.Close();

            // Загружаем действующие вещества
            string ingredientsQuery = @"SELECT ai.name, aim.quantity
                                 FROM active_ingredient_medicines aim
                                 JOIN active_ingredients ai ON aim.active_ingredient_id = ai.id
                                 WHERE aim.medicine_id = @id;";

            using var ingredientsCommand = new MySqlCommand(ingredientsQuery, connection);
            ingredientsCommand.Parameters.AddWithValue("@id", id);

            using var ingredientsReader = ingredientsCommand.ExecuteReader();
            while (ingredientsReader.Read())
            {
                model.ActiveIngredients.Add(new ActiveIngredientViewModel
                {
                    Name = ingredientsReader.GetString("name"),
                    Quantity = ingredientsReader.IsDBNull(ingredientsReader.GetOrdinal("quantity"))
                        ? null
                        : ingredientsReader.GetString("quantity")
                });
            }

            return View(model);
        }
    }
}
