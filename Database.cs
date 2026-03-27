using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace AutoSalon
{
    public static class DatabaseInitializer
    {
        public static void Initialize()
        {
            using (var connection = new SqlConnection(Database.ConnectionString))
            {
                connection.Open();
                var commandText = @"
IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users(
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FullName NVARCHAR(120) NOT NULL,
        Email NVARCHAR(120) NOT NULL UNIQUE,
        Phone NVARCHAR(30) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(256) NOT NULL,
        Role NVARCHAR(20) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME()
    );
END;

IF NOT EXISTS(SELECT 1 FROM dbo.Users WHERE Role = N'Администратор')
BEGIN
    INSERT INTO dbo.Users(FullName, Email, Phone, PasswordHash, Role)
    VALUES(N'Главный администратор', N'admin@autosalon.local', N'+70000000000', @Hash, N'Администратор');
END;";

                using (var command = new SqlCommand(commandText, connection))
                {
                    command.Parameters.AddWithValue("@Hash", SecurityHelper.HashPassword("Admin123!"));
                    command.ExecuteNonQuery();
                }
            }
        }
    }

    public static class Database
    {
        public static string ConnectionString =>
            ConfigurationManager.ConnectionStrings["AutoSalonDb"]?.ConnectionString
            ?? @"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;Initial Catalog=AutoSalonDB;";

        public static DataTable Query(string sql, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(sql, connection))
            using (var adapter = new SqlDataAdapter(command))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                var table = new DataTable();
                connection.Open();
                adapter.Fill(table);
                return table;
            }
        }

        public static int Execute(string sql, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        public static object Scalar(string sql, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                connection.Open();
                return command.ExecuteScalar();
            }
        }
    }

    public static class SecurityHelper
    {
        public static string HashPassword(string password)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}
