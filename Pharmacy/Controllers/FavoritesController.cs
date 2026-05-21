using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Pharmacy.Models;

namespace Pharmacy.Controllers
{
    public class FavoritesController : Controller
    {
        private readonly string _connectionString;

        public FavoritesController(string connectionString)
        {
            _connectionString = connectionString;
        }

        [Authorize]
        public IActionResult Index()
        {
            var userId = User.FindFirst("id")?.Value;

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string query = @"SELECT m.id, m.name, m.price, m.rating, m.image_url,
                        m.prescription_required, m.stock_quantity,
                        c.name AS category_name
                 FROM favorite_items fi
                 JOIN medicines m ON fi.medicine_id = m.id
                 LEFT JOIN categories c ON m.category_id = c.id
                 WHERE fi.user_id = @userId AND m.is_active = TRUE;";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@userId", userId);

            var medicines = new List<FavoriteItemViewModel>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                medicines.Add(new FavoriteItemViewModel
                {
                    Id = reader.GetInt32("id"),
                    Name = reader.GetString("name"),
                    Price = reader.GetDecimal("price"),
                    Rating = reader.GetDecimal("rating"),
                    ImageUrl = reader.IsDBNull(reader.GetOrdinal("image_url")) ? null : reader.GetString("image_url"),
                    PrescriptionRequired = reader.GetBoolean("prescription_required"),
                    StockQuantity = reader.GetInt32("stock_quantity"),
                    CategoryName = reader.IsDBNull(reader.GetOrdinal("category_name")) ? null : reader.GetString("category_name")
                });
            }

            return View(medicines);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Toggle([FromBody] ToggleFavoriteRequest request)
        {
            var userId = User.FindFirst("id")?.Value;

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string checkQuery = "SELECT COUNT(*) FROM favorite_items WHERE user_id = @userId AND medicine_id = @medicineId;";
            using var checkCommand = new MySqlCommand(checkQuery, connection);
            checkCommand.Parameters.AddWithValue("@userId", userId);
            checkCommand.Parameters.AddWithValue("@medicineId", request.MedicineId);
            var exists = Convert.ToInt64(checkCommand.ExecuteScalar()) > 0;

            if (exists)
            {
                string deleteQuery = "DELETE FROM favorite_items WHERE user_id = @userId AND medicine_id = @medicineId;";
                using var deleteCommand = new MySqlCommand(deleteQuery, connection);
                deleteCommand.Parameters.AddWithValue("@userId", userId);
                deleteCommand.Parameters.AddWithValue("@medicineId", request.MedicineId);
                deleteCommand.ExecuteNonQuery();
                return Json(new { isFavorite = false });
            }
            else
            {
                string insertQuery = "INSERT INTO favorite_items (user_id, medicine_id) VALUES (@userId, @medicineId);";
                using var insertCommand = new MySqlCommand(insertQuery, connection);
                insertCommand.Parameters.AddWithValue("@userId", userId);
                insertCommand.Parameters.AddWithValue("@medicineId", request.MedicineId);
                insertCommand.ExecuteNonQuery();
                return Json(new { isFavorite = true });
            }
        }
    }

    public class ToggleFavoriteRequest
    {
        public int MedicineId { get; set; }
    }
}
