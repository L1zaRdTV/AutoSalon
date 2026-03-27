using System;
using System.Text.RegularExpressions;

namespace AutoSalon
{
    public enum UserRole
    {
        Гость,
        Пользователь,
        Менеджер,
        Администратор
    }

    public class SessionUser
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public UserRole Role { get; set; }
    }

    public static class ValidationHelper
    {
        public static bool IsValidEmail(string email)
        {
            return !string.IsNullOrWhiteSpace(email) && Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        public static bool IsValidPhone(string phone)
        {
            return !string.IsNullOrWhiteSpace(phone) && Regex.IsMatch(phone, @"^\+?[0-9]{10,15}$");
        }

        public static bool IsStrongPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                return false;
            }

            return Regex.IsMatch(password, @"[A-Za-z]") && Regex.IsMatch(password, @"\d");
        }

        public static string CopyProductImage(string sourcePath)
        {
            var fileName = $"{Guid.NewGuid():N}_{System.IO.Path.GetFileName(sourcePath)}";
            var relativePath = System.IO.Path.Combine("ProductImages", fileName);
            var absolutePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
            System.IO.File.Copy(sourcePath, absolutePath, true);
            return relativePath;
        }
    }
}
