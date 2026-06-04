using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Pharmacy.Areas.Admin.Models;

namespace Pharmacy.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BrandsController : Controller
    {
        private readonly string _connectionString;

        public BrandsController(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IActionResult Index()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string query = "SELECT id, name FROM brands ORDER BY name;";
            using var command = new MySqlCommand(query, connection);
            using var reader = command.ExecuteReader();

            var brands = new List<BrandViewModel>();
            while (reader.Read())
                brands.Add(new BrandViewModel
                {
                    Id = reader.GetInt32("id"),
                    Name = reader.GetString("name")
                });

            return View(brands);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BrandViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string query = "INSERT INTO brands (name) VALUES (@name);";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", model.Name);
            command.ExecuteNonQuery();

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string query = "SELECT id, name FROM brands WHERE id = @id;";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            using var reader = command.ExecuteReader();

            if (!reader.Read())
                return NotFound();

            var model = new BrandViewModel
            {
                Id = reader.GetInt32("id"),
                Name = reader.GetString("name")
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, BrandViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string query = "UPDATE brands SET name = @name WHERE id = @id;";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@name", model.Name);
            command.ExecuteNonQuery();

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string query = "SELECT id, name FROM brands WHERE id = @id;";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            using var reader = command.ExecuteReader();

            if (!reader.Read())
                return NotFound();

            var model = new BrandViewModel
            {
                Id = reader.GetInt32("id"),
                Name = reader.GetString("name")
            };

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string checkQuery = "SELECT COUNT(*) FROM medicines WHERE brand_id = @id;";
            using var checkCommand = new MySqlCommand(checkQuery, connection);
            checkCommand.Parameters.AddWithValue("@id", id);
            var count = Convert.ToInt32(checkCommand.ExecuteScalar());

            if (count > 0)
            {
                ModelState.AddModelError(string.Empty, $"Невозможно удалить бренд — он используется в {count} лекарствах.");

                var brand = new BrandViewModel { Id = id };
                string nameQuery = "SELECT name FROM brands WHERE id = @id;";
                using var nameCommand = new MySqlCommand(nameQuery, connection);
                nameCommand.Parameters.AddWithValue("@id", id);
                brand.Name = (string)nameCommand.ExecuteScalar();

                return View(brand);
            }

            string query = "DELETE FROM brands WHERE id = @id;";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();

            return RedirectToAction("Index");
        }
    }
}