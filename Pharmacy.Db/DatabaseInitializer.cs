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

        public void Initialize()
        {
            CreateDatabase();
        }

        public void CreateDatabase()
        {
            // Убираю название бд на случай, если она еще не создана
            var builder = new MySqlConnectionStringBuilder(_connectionString);
            builder.Remove("Database");

            // Подключаюсь к серверу, а не к конкретной бд
            using var connection = new MySqlConnection(builder.ConnectionString);
            connection.Open();

            // Создаю бд, если ее нет
            string sqlQuery = $"CREATE DATABASE IF NOT EXISTS pharmacy_db";
            using MySqlCommand command = new MySqlCommand(sqlQuery, connection);
            command.ExecuteNonQuery();
        }
    }
}
