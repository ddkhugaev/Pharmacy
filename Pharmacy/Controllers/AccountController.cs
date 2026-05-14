using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Pharmacy.Models;
using System.Security.Claims;

namespace Pharmacy.Controllers
{
    public class AccountController : Controller
    {
        private readonly string _connectionString;

        public AccountController(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            // Проверяем, что email не занят
            string checkQuery = "SELECT COUNT(*) FROM users WHERE email = @email;";
            using var checkCommand = new MySqlCommand(checkQuery, connection);
            checkCommand.Parameters.AddWithValue("@email", model.Email);
            var count = Convert.ToInt64(checkCommand.ExecuteScalar());

            if (count > 0)
            {
                ModelState.AddModelError("Email", "Этот email уже зарегистрирован");
                return View(model);
            }

            // Хешируем пароль
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            // Сохраняем пользователя
            string insertQuery = @"INSERT INTO users (first_name, last_name, email, phone, password_hash)
                           VALUES (@firstName, @lastName, @email, @phone, @passwordHash);";
            using var insertCommand = new MySqlCommand(insertQuery, connection);
            insertCommand.Parameters.AddWithValue("@firstName", model.FirstName);
            insertCommand.Parameters.AddWithValue("@lastName", model.LastName);
            insertCommand.Parameters.AddWithValue("@email", model.Email);
            insertCommand.Parameters.AddWithValue("@phone", (object?)model.Phone ?? DBNull.Value);
            insertCommand.Parameters.AddWithValue("@passwordHash", passwordHash);
            insertCommand.ExecuteNonQuery();

            // Получаем id нового пользователя
            long userId = insertCommand.LastInsertedId;

            // Создаём claims и логиним пользователя
            var claims = new List<Claim>
            {
                new Claim("id", userId.ToString()),
                new Claim(ClaimTypes.Email, model.Email),
                new Claim(ClaimTypes.Name, model.FirstName),
                new Claim(ClaimTypes.Role, "Customer")
            };

            var identity = new ClaimsIdentity(claims, "Cookies");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("Cookies", principal);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            // Ищем пользователя по email
            string query = @"SELECT id, first_name, email, password_hash, role, is_active 
                     FROM users WHERE email = @email;";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@email", model.Email);

            using var reader = command.ExecuteReader();

            if (!reader.Read())
            {
                ModelState.AddModelError(string.Empty, "Неверный email или пароль");
                return View(model);
            }

            var passwordHash = reader.GetString("password_hash");
            var isActive = reader.GetBoolean("is_active");

            // Проверяем пароль
            if (!BCrypt.Net.BCrypt.Verify(model.Password, passwordHash))
            {
                ModelState.AddModelError(string.Empty, "Неверный email или пароль");
                return View(model);
            }

            // Проверяем что аккаунт активен
            if (!isActive)
            {
                ModelState.AddModelError(string.Empty, "Аккаунт заблокирован");
                return View(model);
            }

            var userId = reader.GetInt32("id");
            var firstName = reader.GetString("first_name");
            var role = reader.GetString("role");

            // Создаём claims
            var claims = new List<Claim>
            {
                new Claim("id", userId.ToString()),
                new Claim(ClaimTypes.Email, model.Email),
                new Claim(ClaimTypes.Name, firstName),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, "Cookies");
            var principal = new ClaimsPrincipal(identity);

            // Если RememberMe - постоянная cookie, иначе сессионная
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(7)
                    : null
            };

            await HttpContext.SignInAsync("Cookies", principal, authProperties);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
