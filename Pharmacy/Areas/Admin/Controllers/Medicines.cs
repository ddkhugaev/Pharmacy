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

        public IActionResult Create()
        {
            var model = new AdminMedicineFormViewModel();
            LoadDropdowns(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AdminMedicineFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdowns(model);
                return View(model);
            }

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string query = @"INSERT INTO medicines 
                        (name, price, category_id, brand_id, manufacturer_id,
                         indications, administration, composition, pharma_group, pharma_props,
                         contraindications, side_effects, overdose, drug_interactions,
                         special_instructions, dosage_form, dispensing_conditions,
                         image_url, prescription_required, stock_quantity, is_active)
                     VALUES
                        (@name, @price, @categoryId, @brandId, @manufacturerId,
                         @indications, @administration, @composition, @pharmaGroup, @pharmaProps,
                         @contraindications, @sideEffects, @overdose, @drugInteractions,
                         @specialInstructions, @dosageForm, @dispensingConditions,
                         @imageUrl, @prescriptionRequired, @stockQuantity, @isActive);";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", model.Name);
            command.Parameters.AddWithValue("@price", model.Price);
            command.Parameters.AddWithValue("@categoryId", (object?)model.CategoryId ?? DBNull.Value);
            command.Parameters.AddWithValue("@brandId", (object?)model.BrandId ?? DBNull.Value);
            command.Parameters.AddWithValue("@manufacturerId", (object?)model.ManufacturerId ?? DBNull.Value);
            command.Parameters.AddWithValue("@indications", (object?)model.Indications ?? DBNull.Value);
            command.Parameters.AddWithValue("@administration", (object?)model.Administration ?? DBNull.Value);
            command.Parameters.AddWithValue("@composition", (object?)model.Composition ?? DBNull.Value);
            command.Parameters.AddWithValue("@pharmaGroup", (object?)model.PharmaGroup ?? DBNull.Value);
            command.Parameters.AddWithValue("@pharmaProps", (object?)model.PharmaProps ?? DBNull.Value);
            command.Parameters.AddWithValue("@contraindications", (object?)model.Contraindications ?? DBNull.Value);
            command.Parameters.AddWithValue("@sideEffects", (object?)model.SideEffects ?? DBNull.Value);
            command.Parameters.AddWithValue("@overdose", (object?)model.Overdose ?? DBNull.Value);
            command.Parameters.AddWithValue("@drugInteractions", (object?)model.DrugInteractions ?? DBNull.Value);
            command.Parameters.AddWithValue("@specialInstructions", (object?)model.SpecialInstructions ?? DBNull.Value);
            command.Parameters.AddWithValue("@dosageForm", (object?)model.DosageForm ?? DBNull.Value);
            command.Parameters.AddWithValue("@dispensingConditions", (object?)model.DispensingConditions ?? DBNull.Value);
            command.Parameters.AddWithValue("@imageUrl", (object?)model.ImageUrl ?? DBNull.Value);
            command.Parameters.AddWithValue("@prescriptionRequired", model.PrescriptionRequired);
            command.Parameters.AddWithValue("@stockQuantity", model.StockQuantity);
            command.Parameters.AddWithValue("@isActive", model.IsActive);
            command.ExecuteNonQuery();

            return RedirectToAction("Index");
        }

        private void LoadDropdowns(AdminMedicineFormViewModel model)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string categoriesQuery = "SELECT id, name FROM categories ORDER BY name;";
            using var categoriesCommand = new MySqlCommand(categoriesQuery, connection);
            using var categoriesReader = categoriesCommand.ExecuteReader();
            while (categoriesReader.Read())
                model.Categories.Add(new CategoryViewModel
                {
                    Id = categoriesReader.GetInt32("id"),
                    Name = categoriesReader.GetString("name")
                });
            categoriesReader.Close();

            string brandsQuery = "SELECT id, name FROM brands ORDER BY name;";
            using var brandsCommand = new MySqlCommand(brandsQuery, connection);
            using var brandsReader = brandsCommand.ExecuteReader();
            while (brandsReader.Read())
                model.Brands.Add(new BrandViewModel
                {
                    Id = brandsReader.GetInt32("id"),
                    Name = brandsReader.GetString("name")
                });
            brandsReader.Close();

            string manufacturersQuery = "SELECT id, name FROM manufacturers ORDER BY name;";
            using var manufacturersCommand = new MySqlCommand(manufacturersQuery, connection);
            using var manufacturersReader = manufacturersCommand.ExecuteReader();
            while (manufacturersReader.Read())
                model.Manufacturers.Add(new ManufacturerViewModel
                {
                    Id = manufacturersReader.GetInt32("id"),
                    Name = manufacturersReader.GetString("name")
                });
        }

        public IActionResult Edit(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string query = @"SELECT id, name, price, category_id, brand_id, manufacturer_id,
                            indications, administration, composition, pharma_group, pharma_props,
                            contraindications, side_effects, overdose, drug_interactions,
                            special_instructions, dosage_form, dispensing_conditions,
                            image_url, prescription_required, stock_quantity, is_active
                     FROM medicines WHERE id = @id;";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            using var reader = command.ExecuteReader();

            if (!reader.Read())
                return NotFound();

            var model = new AdminMedicineFormViewModel
            {
                Name = reader.GetString("name"),
                Price = reader.GetInt32("price"),
                CategoryId = reader.IsDBNull(reader.GetOrdinal("category_id")) ? null : reader.GetInt32("category_id"),
                BrandId = reader.IsDBNull(reader.GetOrdinal("brand_id")) ? null : reader.GetInt32("brand_id"),
                ManufacturerId = reader.IsDBNull(reader.GetOrdinal("manufacturer_id")) ? null : reader.GetInt32("manufacturer_id"),
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
                ImageUrl = reader.IsDBNull(reader.GetOrdinal("image_url")) ? null : reader.GetString("image_url"),
                PrescriptionRequired = reader.GetBoolean("prescription_required"),
                StockQuantity = reader.GetInt32("stock_quantity"),
                IsActive = reader.GetBoolean("is_active")
            };
            reader.Close();

            LoadDropdowns(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, AdminMedicineFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdowns(model);
                return View(model);
            }

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string query = @"UPDATE medicines SET
                        name = @name,
                        price = @price,
                        category_id = @categoryId,
                        brand_id = @brandId,
                        manufacturer_id = @manufacturerId,
                        indications = @indications,
                        administration = @administration,
                        composition = @composition,
                        pharma_group = @pharmaGroup,
                        pharma_props = @pharmaProps,
                        contraindications = @contraindications,
                        side_effects = @sideEffects,
                        overdose = @overdose,
                        drug_interactions = @drugInteractions,
                        special_instructions = @specialInstructions,
                        dosage_form = @dosageForm,
                        dispensing_conditions = @dispensingConditions,
                        image_url = @imageUrl,
                        prescription_required = @prescriptionRequired,
                        stock_quantity = @stockQuantity,
                        is_active = @isActive
                     WHERE id = @id;";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@name", model.Name);
            command.Parameters.AddWithValue("@price", model.Price);
            command.Parameters.AddWithValue("@categoryId", (object?)model.CategoryId ?? DBNull.Value);
            command.Parameters.AddWithValue("@brandId", (object?)model.BrandId ?? DBNull.Value);
            command.Parameters.AddWithValue("@manufacturerId", (object?)model.ManufacturerId ?? DBNull.Value);
            command.Parameters.AddWithValue("@indications", (object?)model.Indications ?? DBNull.Value);
            command.Parameters.AddWithValue("@administration", (object?)model.Administration ?? DBNull.Value);
            command.Parameters.AddWithValue("@composition", (object?)model.Composition ?? DBNull.Value);
            command.Parameters.AddWithValue("@pharmaGroup", (object?)model.PharmaGroup ?? DBNull.Value);
            command.Parameters.AddWithValue("@pharmaProps", (object?)model.PharmaProps ?? DBNull.Value);
            command.Parameters.AddWithValue("@contraindications", (object?)model.Contraindications ?? DBNull.Value);
            command.Parameters.AddWithValue("@sideEffects", (object?)model.SideEffects ?? DBNull.Value);
            command.Parameters.AddWithValue("@overdose", (object?)model.Overdose ?? DBNull.Value);
            command.Parameters.AddWithValue("@drugInteractions", (object?)model.DrugInteractions ?? DBNull.Value);
            command.Parameters.AddWithValue("@specialInstructions", (object?)model.SpecialInstructions ?? DBNull.Value);
            command.Parameters.AddWithValue("@dosageForm", (object?)model.DosageForm ?? DBNull.Value);
            command.Parameters.AddWithValue("@dispensingConditions", (object?)model.DispensingConditions ?? DBNull.Value);
            command.Parameters.AddWithValue("@imageUrl", (object?)model.ImageUrl ?? DBNull.Value);
            command.Parameters.AddWithValue("@prescriptionRequired", model.PrescriptionRequired);
            command.Parameters.AddWithValue("@stockQuantity", model.StockQuantity);
            command.Parameters.AddWithValue("@isActive", model.IsActive);
            command.ExecuteNonQuery();

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string query = "SELECT id, name FROM medicines WHERE id = @id;";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            using var reader = command.ExecuteReader();

            if (!reader.Read())
                return NotFound();

            var model = new AdminMedicineViewModel
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

            string query = "DELETE FROM medicines WHERE id = @id;";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();

            return RedirectToAction("Index");
        }

    }
}
