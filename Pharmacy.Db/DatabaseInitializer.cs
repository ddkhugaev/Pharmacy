using MySql.Data.MySqlClient;

namespace Pharmacy.Db
{
    public class DatabaseInitializer
    {
        private readonly string _connectionString;

        public DatabaseInitializer(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Инициализация бд
        public void Initialize()
        {
            CreateDatabase();
            CreateTables();
            CreateTriggers();
        }

        // Создание бд
        private void CreateDatabase()
        {
            // Убираю название бд на случай, если она еще не создана
            var builder = new MySqlConnectionStringBuilder(_connectionString);
            builder.Remove("Database");

            // Подключаюсь к серверу, а не к конкретной бд
            using var connection = new MySqlConnection(builder.ConnectionString);
            connection.Open();

            // Создаю бд, если ее нет
            string sqlQuery = $"CREATE DATABASE IF NOT EXISTS pharmacy_db;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        // Создание таблиц
        private void CreateTables()
        {
            CreateTableCategories();
            CreateTableBrands();
            CreateTableCountries();
            CreateTableActiveIngredients();
            CreateTableUsers();
            CreateTableManufacturers();
            CreateTableMedicines();
            CreateTableMedicineBatches();
            CreateTableActiveIngredientMedicines();
            CreateTableReviews();
            CreateTableCartItems();
            CreateTableFavoriteItems();
            CreateTableOrders();
            CreateTableOrderItems();
            CreateTableAuditLog();
        }

        // Создание триггеров
        private void CreateTriggers()
        {
            CreateTriggersCategories();
            CreateTriggersBrands();
            CreateTriggersCountries();
            CreateTriggersActiveIngredients();
            CreateTriggersUsers();
            CreateTriggersManufacturers();
            CreateTriggersMedicines();
            CreateTriggersMedicineBatches();
            CreateTriggersActiveIngredientMedicines();
            CreateTriggersReviews();
            CreateTriggersCartItems();
            CreateTriggersFavoriteItems();
            CreateTriggersOrders();
            CreateTriggersOrderItems();
        }

        //Таблицы

        // Таблица категорий лекарств
        private void CreateTableCategories()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TABLE IF NOT EXISTS categories (
                                    id INT AUTO_INCREMENT PRIMARY KEY,
                                    name VARCHAR(255) NOT NULL UNIQUE
                                );";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        // Таблица брендов
        private void CreateTableBrands()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TABLE IF NOT EXISTS brands (
                                    id INT AUTO_INCREMENT PRIMARY KEY,
                                    name VARCHAR(255) NOT NULL UNIQUE
                                );";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        // Таблица стран производителей
        private void CreateTableCountries()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TABLE IF NOT EXISTS countries (
                                    id INT AUTO_INCREMENT PRIMARY KEY,
                                    name VARCHAR(255) NOT NULL UNIQUE
                                );";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        // Таблица действующих веществ
        private void CreateTableActiveIngredients()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TABLE IF NOT EXISTS active_ingredients (
                                    id INT AUTO_INCREMENT PRIMARY KEY,
                                    name VARCHAR(255) NOT NULL UNIQUE
                                );";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        // Таблица пользователей
        private void CreateTableUsers()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TABLE IF NOT EXISTS users (
                                    id INT AUTO_INCREMENT PRIMARY KEY,
                                    first_name VARCHAR(255) NOT NULL,
                                    last_name VARCHAR(255) NOT NULL,
                                    email VARCHAR(255) NOT NULL UNIQUE,
                                    phone VARCHAR(20) UNIQUE,
                                    password_hash VARCHAR(255) NOT NULL,
                                    role ENUM('Customer', 'Admin') NOT NULL DEFAULT 'Customer',
                                    is_active BOOLEAN DEFAULT TRUE,
                                    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
                                );";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        // Таблица производителей
        private void CreateTableManufacturers()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TABLE IF NOT EXISTS manufacturers (
                                    id INT AUTO_INCREMENT PRIMARY KEY,
                                    name VARCHAR(255) NOT NULL UNIQUE,
                                    country_id INT NOT NULL,
                                    FOREIGN KEY (country_id) REFERENCES countries(id)
                                );";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        // Таблица лекарств
        private void CreateTableMedicines()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TABLE IF NOT EXISTS medicines (
                                    id INT AUTO_INCREMENT PRIMARY KEY,
                                    name VARCHAR(255) NOT NULL,
                                    price DECIMAL(10, 2) NOT NULL,
                                    category_id INT,
                                    brand_id INT,
                                    manufacturer_id INT,
                                    indications TEXT,
                                    administration TEXT,
                                    composition TEXT,
                                    pharma_group TEXT,
                                    pharma_props TEXT,
                                    contraindications TEXT,
                                    side_effects TEXT,
                                    overdose TEXT,
                                    drug_interactions TEXT,
                                    special_instructions TEXT,
                                    dosage_form TEXT,
                                    dispensing_conditions TEXT,
                                    literature TEXT,
                                    rating DECIMAL(3, 2) DEFAULT 0.0 CHECK (rating >= 0 AND rating <= 5),
                                    image_url VARCHAR(255),
                                    prescription_required BOOLEAN DEFAULT FALSE,
                                    stock_quantity INT NOT NULL DEFAULT 0 CHECK (stock_quantity >= 0),
                                    is_active BOOLEAN DEFAULT TRUE,
                                    FOREIGN KEY (category_id) REFERENCES categories(id),
                                    FOREIGN KEY (brand_id) REFERENCES brands(id),
                                    FOREIGN KEY (manufacturer_id) REFERENCES manufacturers(id)
                                );";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        // Таблица партий лекарств
        private void CreateTableMedicineBatches()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TABLE IF NOT EXISTS medicine_batches (
                                    id INT AUTO_INCREMENT PRIMARY KEY,
                                    medicine_id INT NOT NULL,
                                    arrival_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                    expiry_date DATE NOT NULL,
                                    batch_number VARCHAR(50),
                                    initial_quantity INT NOT NULL,
                                    current_quantity INT NOT NULL CHECK (current_quantity >= 0),
                                    FOREIGN KEY (medicine_id) REFERENCES medicines(id)
                                );";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        // Таблица для связи действующих веществ и лекарств
        private void CreateTableActiveIngredientMedicines()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TABLE IF NOT EXISTS active_ingredient_medicines (
                                    medicine_id INT NOT NULL,
                                    active_ingredient_id INT NOT NULL,
                                    quantity VARCHAR(100),
                                    PRIMARY KEY (medicine_id, active_ingredient_id),
                                    FOREIGN KEY (medicine_id) REFERENCES medicines(id),
                                    FOREIGN KEY (active_ingredient_id) REFERENCES active_ingredients(id)
                                );";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        // Таблица отзывов
        private void CreateTableReviews()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TABLE IF NOT EXISTS reviews (
                                    id INT AUTO_INCREMENT PRIMARY KEY,
                                    user_id INT NOT NULL,
                                    medicine_id INT NOT NULL,
                                    rating INT NOT NULL CHECK(rating >= 1 AND rating <= 5),
                                    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                                    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                                    comment_text TEXT,
                                    FOREIGN KEY (user_id) REFERENCES users(id),
                                    FOREIGN KEY (medicine_id) REFERENCES medicines(id),
                                    UNIQUE KEY id_user_medicine (user_id, medicine_id)
                                );";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        // Таблица элементов корзины
        private void CreateTableCartItems()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TABLE IF NOT EXISTS cart_items (
                                    id INT AUTO_INCREMENT PRIMARY KEY,
                                    user_id INT NOT NULL,
                                    medicine_id INT NOT NULL,
                                    quantity INT NOT NULL DEFAULT 1 CHECK (quantity >= 1),
                                    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
                                    FOREIGN KEY (medicine_id) REFERENCES medicines(id),
                                    UNIQUE KEY id_user_medicine (user_id, medicine_id)
                                );";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        // Таблица элементов избранного
        private void CreateTableFavoriteItems()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TABLE IF NOT EXISTS favorite_items (
                                    id INT AUTO_INCREMENT PRIMARY KEY,
                                    user_id INT NOT NULL,
                                    medicine_id INT NOT NULL,
                                    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
                                    FOREIGN KEY (medicine_id) REFERENCES medicines(id),
                                    UNIQUE KEY id_user_medicine (user_id, medicine_id)
                                );";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        // Таблица заказов
        private void CreateTableOrders()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TABLE IF NOT EXISTS orders (
                                    id INT AUTO_INCREMENT PRIMARY KEY,
                                    status ENUM('Создан', 'Оплачен', 'Собран', 'В пути', 'Доставлен', 'Получен') NOT NULL DEFAULT 'Создан',
                                    user_id INT NOT NULL,
                                    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                    completed_at DATETIME,
                                    total_price DECIMAL(10, 2) NOT NULL,
                                    delivery_address VARCHAR(500),
                                    FOREIGN KEY (user_id) REFERENCES users(id)
                                );";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        // Таблица элементов заказа
        private void CreateTableOrderItems()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TABLE IF NOT EXISTS order_items (
                                    id INT AUTO_INCREMENT PRIMARY KEY,
                                    order_id INT NOT NULL,
                                    medicine_id INT NOT NULL,
                                    quantity INT NOT NULL CHECK(quantity > 0),
                                    price_at_time DECIMAL(10, 2) NOT NULL,
                                    FOREIGN KEY (order_id) REFERENCES orders(id),
                                    FOREIGN KEY (medicine_id) REFERENCES medicines(id)
                                );";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        // Таблица логов
        private void CreateTableAuditLog()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TABLE IF NOT EXISTS audit_log (
                                    id INT AUTO_INCREMENT PRIMARY KEY,
                                    table_name VARCHAR(255) NOT NULL,
                                    operation ENUM('INSERT', 'UPDATE', 'DELETE') NOT NULL,
                                    record_id INT NOT NULL,
                                    changed_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                                    old_data JSON,
                                    new_data JSON
                                );";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        // Триггеры

        // Триггеры таблицы категорий лекарств
        private void CreateTriggersCategories()
        {
            CreateTriggerCategoriesInsert();
            CreateTriggerCategoriesUpdate();
            CreateTriggerCategoriesDelete();
        }

        private void CreateTriggerCategoriesInsert()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_categories_insert
                                AFTER INSERT ON categories
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, new_data)
                                    VALUES ('categories', 'INSERT', NEW.id, JSON_OBJECT(
                                        'id', NEW.id,
                                        'name', NEW.name
                                    ));
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        private void CreateTriggerCategoriesUpdate()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_categories_update
                                AFTER UPDATE ON categories
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, old_data, new_data)
                                    VALUES ('categories', 'UPDATE', OLD.id,
                                        JSON_OBJECT(
                                            'name', OLD.name
                                        ),
                                        JSON_OBJECT(
                                            'name', NEW.name
                                        )
                                    );
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        private void CreateTriggerCategoriesDelete()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_categories_delete
                                AFTER DELETE ON categories
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, old_data)
                                    VALUES ('categories', 'DELETE', OLD.id, JSON_OBJECT(
                                        'id', OLD.id,
                                        'name', OLD.name
                                    ));
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        // Триггеры таблицы брендов
        private void CreateTriggersBrands()
        {
            CreateTriggerBrandsInsert();
            CreateTriggerBrandsUpdate();
            CreateTriggerBrandsDelete();
        }

        private void CreateTriggerBrandsInsert()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_brands_insert
                                AFTER INSERT ON brands
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, new_data)
                                    VALUES ('brands', 'INSERT', NEW.id, JSON_OBJECT(
                                        'id', NEW.id,
                                        'name', NEW.name
                                    ));
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        private void CreateTriggerBrandsUpdate()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_brands_update
                                AFTER UPDATE ON brands
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, old_data, new_data)
                                    VALUES ('brands', 'UPDATE', OLD.id,
                                        JSON_OBJECT(
                                            'name', OLD.name
                                        ),
                                        JSON_OBJECT(
                                            'name', NEW.name
                                        )
                                    );
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        private void CreateTriggerBrandsDelete()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_brands_delete
                                AFTER DELETE ON brands
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, old_data)
                                    VALUES ('brands', 'DELETE', OLD.id, JSON_OBJECT(
                                        'id', OLD.id,
                                        'name', OLD.name
                                    ));
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        // Триггеры таблицы стран производителей
        private void CreateTriggersCountries()
        {
            CreateTriggerCountriesInsert();
            CreateTriggerCountriesUpdate();
            CreateTriggerCountriesDelete();
        }

        private void CreateTriggerCountriesInsert()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_countries_insert
                                AFTER INSERT ON countries
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, new_data)
                                    VALUES ('countries', 'INSERT', NEW.id, JSON_OBJECT(
                                        'id', NEW.id,
                                        'name', NEW.name
                                    ));
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        private void CreateTriggerCountriesUpdate()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_countries_update
                                AFTER UPDATE ON countries
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, old_data, new_data)
                                    VALUES ('countries', 'UPDATE', OLD.id,
                                        JSON_OBJECT(
                                            'name', OLD.name
                                        ),
                                        JSON_OBJECT(
                                            'name', NEW.name
                                        )
                                    );
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        private void CreateTriggerCountriesDelete()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_countries_delete
                                AFTER DELETE ON countries
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, old_data)
                                    VALUES ('countries', 'DELETE', OLD.id, JSON_OBJECT(
                                        'id', OLD.id,
                                        'name', OLD.name
                                    ));
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        // Триггеры таблицы действующих веществ
        private void CreateTriggersActiveIngredients()
        {
            CreateTriggerActiveIngredientsInsert();
            CreateTriggerActiveIngredientsUpdate();
            CreateTriggerActiveIngredientsDelete();
        }

        private void CreateTriggerActiveIngredientsInsert()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_active_ingredients_insert
                                AFTER INSERT ON active_ingredients
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, new_data)
                                    VALUES ('active_ingredients', 'INSERT', NEW.id, JSON_OBJECT(
                                        'id', NEW.id,
                                        'name', NEW.name
                                    ));
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        private void CreateTriggerActiveIngredientsUpdate()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_active_ingredients_update
                                AFTER UPDATE ON active_ingredients
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, old_data, new_data)
                                    VALUES ('active_ingredients', 'UPDATE', OLD.id,
                                        JSON_OBJECT(
                                            'name', OLD.name
                                        ),
                                        JSON_OBJECT(
                                            'name', NEW.name
                                        )
                                    );
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        private void CreateTriggerActiveIngredientsDelete()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_active_ingredients_delete
                                AFTER DELETE ON active_ingredients
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, old_data)
                                    VALUES ('active_ingredients', 'DELETE', OLD.id, JSON_OBJECT(
                                        'id', OLD.id,
                                        'name', OLD.name
                                    ));
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        // Триггеры таблицы пользователей
        private void CreateTriggersUsers()
        {
            CreateTriggerUsersInsert();
            CreateTriggerUsersUpdate();
            CreateTriggerUsersDelete();
        }

        private void CreateTriggerUsersInsert()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_users_insert
                                AFTER INSERT ON users
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, new_data)
                                    VALUES ('users', 'INSERT', NEW.id, JSON_OBJECT(
                                        'id', NEW.id,
                                        'first_name', NEW.first_name,
                                        'last_name', NEW.last_name,
                                        'email', NEW.email,
                                        'phone', NEW.phone,
                                        'role', NEW.role,
                                        'is_active', NEW.is_active,
                                        'created_at', NEW.created_at
                                    ));
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        private void CreateTriggerUsersUpdate()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_users_update
                                AFTER UPDATE ON users
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, old_data, new_data)
                                    VALUES ('users', 'UPDATE', OLD.id,
                                        JSON_OBJECT(
                                            'first_name', OLD.first_name,
                                            'last_name', OLD.last_name,
                                            'email', OLD.email,
                                            'phone', OLD.phone,
                                            'role', OLD.role,
                                            'is_active', OLD.is_active
                                        ),
                                        JSON_OBJECT(
                                            'first_name', NEW.first_name,
                                            'last_name', NEW.last_name,
                                            'email', NEW.email,
                                            'phone', NEW.phone,
                                            'role', NEW.role,
                                            'is_active', NEW.is_active
                                        )
                                    );
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        private void CreateTriggerUsersDelete()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_users_delete
                                AFTER DELETE ON users
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, old_data)
                                    VALUES ('users', 'DELETE', OLD.id, JSON_OBJECT(
                                        'id', OLD.id,
                                        'first_name', OLD.first_name,
                                        'last_name', OLD.last_name,
                                        'email', OLD.email,
                                        'phone', OLD.phone,
                                        'role', OLD.role,
                                        'is_active', OLD.is_active,
                                        'created_at', OLD.created_at
                                    ));
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        // Триггеры таблицы производителей
        private void CreateTriggersManufacturers()
        {
            CreateTriggerManufacturersInsert();
            CreateTriggerManufacturersUpdate();
            CreateTriggerManufacturersDelete();
        }

        private void CreateTriggerManufacturersInsert()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_manufacturers_insert
                                AFTER INSERT ON manufacturers
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, new_data)
                                    VALUES ('manufacturers', 'INSERT', NEW.id, JSON_OBJECT(
                                        'id', NEW.id,
                                        'name', NEW.name,
                                        'country_id', NEW.country_id
                                    ));
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        private void CreateTriggerManufacturersUpdate()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_manufacturers_update
                                AFTER UPDATE ON manufacturers
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, old_data, new_data)
                                    VALUES ('manufacturers', 'UPDATE', OLD.id,
                                        JSON_OBJECT(
                                            'name', OLD.name,
                                            'country_id', OLD.country_id
                                        ),
                                        JSON_OBJECT(
                                            'name', NEW.name,
                                            'country_id', NEW.country_id
                                        )
                                    );
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        private void CreateTriggerManufacturersDelete()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_manufacturers_delete
                                AFTER DELETE ON manufacturers
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, old_data)
                                    VALUES ('manufacturers', 'DELETE', OLD.id, JSON_OBJECT(
                                        'id', OLD.id,
                                        'name', OLD.name,
                                        'country_id', OLD.country_id
                                    ));
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        // Триггеры таблицы лекарств
        private void CreateTriggersMedicines()
        {
            CreateTriggerMedicinesInsert();
            CreateTriggerMedicinesUpdate();
            CreateTriggerMedicinesDelete();
        }

        private void CreateTriggerMedicinesInsert()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_medicines_insert
                                AFTER INSERT ON medicines
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, new_data)
                                    VALUES ('medicines', 'INSERT', NEW.id, JSON_OBJECT(
                                        'id', NEW.id,
                                        'name', NEW.name,
                                        'price', NEW.price,
                                        'category_id', NEW.category_id,
                                        'brand_id', NEW.brand_id,
                                        'manufacturer_id', NEW.manufacturer_id,
                                        'rating', NEW.rating,
                                        'prescription_required', NEW.prescription_required,
                                        'stock_quantity', NEW.stock_quantity,
                                        'is_active', NEW.is_active
                                    ));
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        private void CreateTriggerMedicinesUpdate()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_medicines_update
                                AFTER UPDATE ON medicines
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, old_data, new_data)
                                    VALUES ('medicines', 'UPDATE', OLD.id,
                                        JSON_OBJECT(
                                            'name', OLD.name,
                                            'price', OLD.price,
                                            'category_id', OLD.category_id,
                                            'brand_id', OLD.brand_id,
                                            'manufacturer_id', OLD.manufacturer_id,
                                            'rating', OLD.rating,
                                            'prescription_required', OLD.prescription_required,
                                            'stock_quantity', OLD.stock_quantity,
                                            'is_active', OLD.is_active
                                        ),
                                        JSON_OBJECT(
                                            'name', NEW.name,
                                            'price', NEW.price,
                                            'category_id', NEW.category_id,
                                            'brand_id', NEW.brand_id,
                                            'manufacturer_id', NEW.manufacturer_id,
                                            'rating', NEW.rating,
                                            'prescription_required', NEW.prescription_required,
                                            'stock_quantity', NEW.stock_quantity,
                                            'is_active', NEW.is_active
                                        )
                                    );
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        private void CreateTriggerMedicinesDelete()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_medicines_delete
                                AFTER DELETE ON medicines
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, old_data)
                                    VALUES ('medicines', 'DELETE', OLD.id, JSON_OBJECT(
                                        'id', OLD.id,
                                        'name', OLD.name,
                                        'price', OLD.price,
                                        'category_id', OLD.category_id,
                                        'brand_id', OLD.brand_id,
                                        'manufacturer_id', OLD.manufacturer_id,
                                        'rating', OLD.rating,
                                        'prescription_required', OLD.prescription_required,
                                        'stock_quantity', OLD.stock_quantity,
                                        'is_active', OLD.is_active
                                    ));
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        // Триггеры таблицы партий лекарств
        private void CreateTriggersMedicineBatches()
        {
            CreateTriggerMedicineBatchesInsert();
            CreateTriggerMedicineBatchesUpdate();
            CreateTriggerMedicineBatchesDelete();
        }

        private void CreateTriggerMedicineBatchesInsert()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_medicine_batches_insert
                                AFTER INSERT ON medicine_batches
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, new_data)
                                    VALUES ('medicine_batches', 'INSERT', NEW.id, JSON_OBJECT(
                                        'id', NEW.id,
                                        'medicine_id', NEW.medicine_id,
                                        'arrival_date', NEW.arrival_date,
                                        'expiry_date', NEW.expiry_date,
                                        'batch_number', NEW.batch_number,
                                        'initial_quantity', NEW.initial_quantity,
                                        'current_quantity', NEW.current_quantity
                                    ));
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        private void CreateTriggerMedicineBatchesUpdate()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_medicine_batches_update
                                AFTER UPDATE ON medicine_batches
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, old_data, new_data)
                                    VALUES ('medicine_batches', 'UPDATE', OLD.id,
                                        JSON_OBJECT(
                                            'medicine_id', OLD.medicine_id,
                                            'arrival_date', OLD.arrival_date,
                                            'expiry_date', OLD.expiry_date,
                                            'batch_number', OLD.batch_number,
                                            'initial_quantity', OLD.initial_quantity,
                                            'current_quantity', OLD.current_quantity
                                        ),
                                        JSON_OBJECT(
                                            'medicine_id', NEW.medicine_id,
                                            'arrival_date', NEW.arrival_date,
                                            'expiry_date', NEW.expiry_date,
                                            'batch_number', NEW.batch_number,
                                            'initial_quantity', NEW.initial_quantity,
                                            'current_quantity', NEW.current_quantity
                                        )
                                    );
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        private void CreateTriggerMedicineBatchesDelete()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_medicine_batches_delete
                                AFTER DELETE ON medicine_batches
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, old_data)
                                    VALUES ('medicine_batches', 'DELETE', OLD.id, JSON_OBJECT(
                                        'id', OLD.id,
                                        'medicine_id', OLD.medicine_id,
                                        'arrival_date', OLD.arrival_date,
                                        'expiry_date', OLD.expiry_date,
                                        'batch_number', OLD.batch_number,
                                        'initial_quantity', OLD.initial_quantity,
                                        'current_quantity', OLD.current_quantity
                                    ));
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        // Триггеры таблицы связи действующих веществ и лекарств
        private void CreateTriggersActiveIngredientMedicines()
        {
            CreateTriggerActiveIngredientMedicinesInsert();
            CreateTriggerActiveIngredientMedicinesUpdate();
            CreateTriggerActiveIngredientMedicinesDelete();
        }

        private void CreateTriggerActiveIngredientMedicinesInsert()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_active_ingredient_medicines_insert
                                AFTER INSERT ON active_ingredient_medicines
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, new_data)
                                    VALUES ('active_ingredient_medicines', 'INSERT', NEW.medicine_id, JSON_OBJECT(
                                        'medicine_id', NEW.medicine_id,
                                        'active_ingredient_id', NEW.active_ingredient_id,
                                        'quantity', NEW.quantity
                                    ));
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        private void CreateTriggerActiveIngredientMedicinesUpdate()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_active_ingredient_medicines_update
                                AFTER UPDATE ON active_ingredient_medicines
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, old_data, new_data)
                                    VALUES ('active_ingredient_medicines', 'UPDATE', OLD.medicine_id,
                                        JSON_OBJECT(
                                            'quantity', OLD.quantity
                                        ),
                                        JSON_OBJECT(
                                            'quantity', NEW.quantity
                                        )
                                    );
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        private void CreateTriggerActiveIngredientMedicinesDelete()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_active_ingredient_medicines_delete
                                AFTER DELETE ON active_ingredient_medicines
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, old_data)
                                    VALUES ('active_ingredient_medicines', 'DELETE', OLD.medicine_id, JSON_OBJECT(
                                        'medicine_id', OLD.medicine_id,
                                        'active_ingredient_id', OLD.active_ingredient_id,
                                        'quantity', OLD.quantity
                                    ));
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        // Триггеры таблицы отзывов
        private void CreateTriggersReviews()
        {
            CreateTriggerReviewsInsert();
            CreateTriggerReviewsUpdate();
            CreateTriggerReviewsDelete();
        }

        private void CreateTriggerReviewsInsert()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_reviews_insert
                                AFTER INSERT ON reviews
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, new_data)
                                    VALUES ('reviews', 'INSERT', NEW.id, JSON_OBJECT(
                                        'id', NEW.id,
                                        'user_id', NEW.user_id,
                                        'medicine_id', NEW.medicine_id,
                                        'rating', NEW.rating,
                                        'comment_text', NEW.comment_text,
                                        'created_at', NEW.created_at
                                    ));
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        private void CreateTriggerReviewsUpdate()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_reviews_update
                                AFTER UPDATE ON reviews
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, old_data, new_data)
                                    VALUES ('reviews', 'UPDATE', OLD.id,
                                        JSON_OBJECT(
                                            'rating', OLD.rating,
                                            'comment_text', OLD.comment_text,
                                            'updated_at', OLD.updated_at
                                        ),
                                        JSON_OBJECT(
                                            'rating', NEW.rating,
                                            'comment_text', NEW.comment_text,
                                            'updated_at', NEW.updated_at
                                        )
                                    );
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        private void CreateTriggerReviewsDelete()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_reviews_delete
                                AFTER DELETE ON reviews
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, old_data)
                                    VALUES ('reviews', 'DELETE', OLD.id, JSON_OBJECT(
                                        'id', OLD.id,
                                        'user_id', OLD.user_id,
                                        'medicine_id', OLD.medicine_id,
                                        'rating', OLD.rating,
                                        'comment_text', OLD.comment_text,
                                        'created_at', OLD.created_at
                                    ));
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        // Триггеры таблицы элементов корзины
        private void CreateTriggersCartItems()
        {
            CreateTriggerCartItemsInsert();
            CreateTriggerCartItemsUpdate();
            CreateTriggerCartItemsDelete();
        }

        private void CreateTriggerCartItemsInsert()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_cart_items_insert
                                AFTER INSERT ON cart_items
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, new_data)
                                    VALUES ('cart_items', 'INSERT', NEW.id, JSON_OBJECT(
                                        'id', NEW.id,
                                        'user_id', NEW.user_id,
                                        'medicine_id', NEW.medicine_id,
                                        'quantity', NEW.quantity
                                    ));
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        private void CreateTriggerCartItemsUpdate()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_cart_items_update
                                AFTER UPDATE ON cart_items
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, old_data, new_data)
                                    VALUES ('cart_items', 'UPDATE', OLD.id,
                                        JSON_OBJECT(
                                            'quantity', OLD.quantity
                                        ),
                                        JSON_OBJECT(
                                            'quantity', NEW.quantity
                                        )
                                    );
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        private void CreateTriggerCartItemsDelete()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_cart_items_delete
                                AFTER DELETE ON cart_items
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, old_data)
                                    VALUES ('cart_items', 'DELETE', OLD.id, JSON_OBJECT(
                                        'id', OLD.id,
                                        'user_id', OLD.user_id,
                                        'medicine_id', OLD.medicine_id,
                                        'quantity', OLD.quantity
                                    ));
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        // Триггеры таблицы элементов избранного
        private void CreateTriggersFavoriteItems()
        {
            CreateTriggerFavoriteItemsInsert();
            CreateTriggerFavoriteItemsDelete();
        }

        private void CreateTriggerFavoriteItemsInsert()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_favorite_items_insert
                                AFTER INSERT ON favorite_items
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, new_data)
                                    VALUES ('favorite_items', 'INSERT', NEW.id, JSON_OBJECT(
                                        'id', NEW.id,
                                        'user_id', NEW.user_id,
                                        'medicine_id', NEW.medicine_id
                                    ));
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        private void CreateTriggerFavoriteItemsDelete()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_favorite_items_delete
                                AFTER DELETE ON favorite_items
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, old_data)
                                    VALUES ('favorite_items', 'DELETE', OLD.id, JSON_OBJECT(
                                        'id', OLD.id,
                                        'user_id', OLD.user_id,
                                        'medicine_id', OLD.medicine_id
                                    ));
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        // Триггеры таблицы заказов
        private void CreateTriggersOrders()
        {
            CreateTriggerOrdersInsert();
            CreateTriggerOrdersUpdate();
            CreateTriggerOrdersDelete();
        }

        private void CreateTriggerOrdersInsert()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_orders_insert
                                AFTER INSERT ON orders
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, new_data)
                                    VALUES ('orders', 'INSERT', NEW.id, JSON_OBJECT(
                                        'id', NEW.id,
                                        'status', NEW.status,
                                        'user_id', NEW.user_id,
                                        'created_at', NEW.created_at,
                                        'completed_at', NEW.completed_at,
                                        'total_price', NEW.total_price,
                                        'delivery_address', NEW.delivery_address
                                    ));
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        private void CreateTriggerOrdersUpdate()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_orders_update
                                AFTER UPDATE ON orders
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, old_data, new_data)
                                    VALUES ('orders', 'UPDATE', OLD.id,
                                        JSON_OBJECT(
                                            'status', OLD.status,
                                            'completed_at', OLD.completed_at,
                                            'total_price', OLD.total_price,
                                            'delivery_address', OLD.delivery_address
                                        ),
                                        JSON_OBJECT(
                                            'status', NEW.status,
                                            'completed_at', NEW.completed_at,
                                            'total_price', NEW.total_price,
                                            'delivery_address', NEW.delivery_address
                                        )
                                    );
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        private void CreateTriggerOrdersDelete()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_orders_delete
                                AFTER DELETE ON orders
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, old_data)
                                    VALUES ('orders', 'DELETE', OLD.id, JSON_OBJECT(
                                        'id', OLD.id,
                                        'status', OLD.status,
                                        'user_id', OLD.user_id,
                                        'created_at', OLD.created_at,
                                        'completed_at', OLD.completed_at,
                                        'total_price', OLD.total_price,
                                        'delivery_address', OLD.delivery_address
                                    ));
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        // Триггеры таблицы элементов заказа
        private void CreateTriggersOrderItems()
        {
            CreateTriggerOrderItemsInsert();
            CreateTriggerOrderItemsUpdate();
            CreateTriggerOrderItemsDelete();
        }

        private void CreateTriggerOrderItemsInsert()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_order_items_insert
                                AFTER INSERT ON order_items
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, new_data)
                                    VALUES ('order_items', 'INSERT', NEW.id, JSON_OBJECT(
                                        'id', NEW.id,
                                        'order_id', NEW.order_id,
                                        'medicine_id', NEW.medicine_id,
                                        'quantity', NEW.quantity,
                                        'price_at_time', NEW.price_at_time
                                    ));
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        private void CreateTriggerOrderItemsUpdate()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_order_items_update
                                AFTER UPDATE ON order_items
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, old_data, new_data)
                                    VALUES ('order_items', 'UPDATE', OLD.id,
                                        JSON_OBJECT(
                                            'order_id', OLD.order_id,
                                            'medicine_id', OLD.medicine_id,
                                            'quantity', OLD.quantity,
                                            'price_at_time', OLD.price_at_time
                                        ),
                                        JSON_OBJECT(
                                            'order_id', NEW.order_id,
                                            'medicine_id', NEW.medicine_id,
                                            'quantity', NEW.quantity,
                                            'price_at_time', NEW.price_at_time
                                        )
                                    );
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }

        private void CreateTriggerOrderItemsDelete()
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            string sqlQuery = @"CREATE TRIGGER IF NOT EXISTS trg_order_items_delete
                                AFTER DELETE ON order_items
                                FOR EACH ROW
                                BEGIN
                                    INSERT INTO audit_log (table_name, operation, record_id, old_data)
                                    VALUES ('order_items', 'DELETE', OLD.id, JSON_OBJECT(
                                        'id', OLD.id,
                                        'order_id', OLD.order_id,
                                        'medicine_id', OLD.medicine_id,
                                        'quantity', OLD.quantity,
                                        'price_at_time', OLD.price_at_time
                                    ));
                                END;";
            using var command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }
    }
}
