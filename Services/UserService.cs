using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ET.Models;

namespace ET.Services
{
    public class UserService
    {
        private static readonly string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        private static readonly string FolderPath = Path.Combine(DesktopPath, "ET");
        private static readonly string FilePath = Path.Combine(FolderPath, "users.json");

        private List<User> users;

        public UserService()
        {
            users = LoadData();
        }

        private List<User> LoadData()
        {
            if (!File.Exists(FilePath))
            {
                return new List<User>();
            }

            try
            {
                var json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data: {ex.Message}");
                return new List<User>();
            }
        }

        private async Task SaveDataAsync()
        {
            EnsureFolderExists();
            try
            {
                var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(FilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data: {ex.Message}");
            }
        }

        private void EnsureFolderExists()
        {
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
        }

        public List<User> GetAllUsers()
        {
            return users;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await Task.FromResult(users);
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            return await Task.FromResult(users.FirstOrDefault(u => u.Id == userId));
        }

        public async Task RegisterUserAsync(User user)
        {
            if (users.Any(u => u.Email == user.Email))
            {
                throw new InvalidOperationException("A user with this email already exists.");
            }

            user.Id = users.Count > 0 ? users.Max(u => u.Id) + 1 : 1;
            user.Password = HashPassword(user.Password, out string salt);
            user.Salt = salt;
            users.Add(user);
            await SaveDataAsync();
        }

        public async Task<User> LoginUserAsync(string email, string password)
        {
            var user = users.FirstOrDefault(u => u.Email == email);
            if (user == null) return null;

            var hashedPassword = HashPassword(password, user.Salt);
            if (user.Password == hashedPassword)
            {
                return user;
            }

            return null;
        }

        private string HashPassword(string password, out string salt)
        {
            using var sha256 = SHA256.Create();
            salt = GenerateSalt();
            var saltedPassword = password + salt;
            var bytes = Encoding.UTF8.GetBytes(saltedPassword);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private string HashPassword(string password, string salt)
        {
            using var sha256 = SHA256.Create();
            var saltedPassword = password + salt;
            var bytes = Encoding.UTF8.GetBytes(saltedPassword);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private string GenerateSalt()
        {
            var saltBytes = new byte[16];
            using var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(saltBytes);
            return Convert.ToBase64String(saltBytes);
        }
    }
}