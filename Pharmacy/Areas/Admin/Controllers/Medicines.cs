using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Pharmacy.Areas.Admin.Models;

namespace Pharmacy.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MedicinesController : Controller
    {
        private readonly string _connectionString;

        public MedicinesController(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IActionResult Index()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string query = @"SELECT m.id, m.name, m.price, m.prescription_required, 
                            m.stock_quantity, m.is_active, m.rating,
                            c.name AS category_name,
                            b.name AS brand_name,
                            mf.name AS manufacturer_name
                     FROM medicines m
                     LEFT JOIN categories c ON m.category_id = c.id
                     LEFT JOIN brands b ON m.brand_id = b.id
                     LEFT JOIN manufacturers mf ON m.manufacturer_id = mf.id
                     ORDER BY m.id DESC;";

            using var command = new MySqlCommand(query, connection);
            using var reader = command.ExecuteReader();

            var medicines = new List<AdminMedicineViewModel>();
            while (reader.Read())
            {
                medicines.Add(new AdminMedicineViewModel
                {
                    Id = reader.GetInt32("id"),
                    Name = reader.GetString("name"),
                    Price = reader.GetDecimal("price"),
                    PrescriptionRequired = reader.GetBoolean("prescription_required"),
                    StockQuantity = reader.GetInt32("stock_quantity"),
                    IsActive = reader.GetBoolean("is_active"),
                    Rating = reader.GetDecimal("rating"),
                    CategoryName = reader.IsDBNull(reader.GetOrdinal("category_name")) ? null : reader.GetString("category_name"),
                    BrandName = reader.IsDBNull(reader.GetOrdinal("brand_name")) ? null : reader.GetString("brand_name"),
                    ManufacturerName = reader.IsDBNull(reader.GetOrdinal("manufacturer_name")) ? null : reader.GetString("manufacturer_name")
                });
            }

            return View(medicines);
        }
    }
}
