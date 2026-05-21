using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Pharmacy.Models;

namespace Pharmacy.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly string _connectionString;

        public ProfileController(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IActionResult Index()
        {
            var userId = User.FindFirst("id")?.Value;

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string query = "SELECT first_name, last_name, phone, email, role, created_at FROM users WHERE id = @id;";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", userId);

            using var reader = command.ExecuteReader();

            if (!reader.Read())
                return NotFound();

            var model = new ProfileViewModel
            {
                FirstName = reader.GetString("first_name"),
                LastName = reader.GetString("last_name"),
                Phone = reader.IsDBNull(reader.GetOrdinal("phone")) ? null : reader.GetString("phone"),
                Email = reader.GetString("email"),
                Role = reader.GetString("role"),
                CreatedAt = reader.GetDateTime("created_at")
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Edit()
        {
            var userId = User.FindFirst("id")?.Value;

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string query = "SELECT first_name, last_name, phone FROM users WHERE id = @id;";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", userId);

            using var reader = command.ExecuteReader();

            if (!reader.Read())
                return NotFound();

            var model = new EditProfileViewModel
            {
                FirstName = reader.GetString("first_name"),
                LastName = reader.GetString("last_name"),
                Phone = reader.IsDBNull(reader.GetOrdinal("phone")) ? null : reader.GetString("phone")
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirst("id")?.Value;

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string query = "UPDATE users SET first_name = @firstName, last_name = @lastName, phone = @phone WHERE id = @id;";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@firstName", model.FirstName);
            command.Parameters.AddWithValue("@lastName", model.LastName);
            command.Parameters.AddWithValue("@phone", (object?)model.Phone ?? DBNull.Value);
            command.Parameters.AddWithValue("@id", userId);
            command.ExecuteNonQuery();

            return RedirectToAction("Index");
        }
    }
}
