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
        }

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
    }
}
