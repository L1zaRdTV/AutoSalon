using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
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

IF OBJECT_ID('dbo.Products', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products(
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Title NVARCHAR(120) NOT NULL,
        Description NVARCHAR(400) NULL,
        Price DECIMAL(12,2) NOT NULL,
        OldPrice DECIMAL(12,2) NULL,
        DiscountPercent INT NULL,
        ImagePath NVARCHAR(260) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME()
    );
END;

IF OBJECT_ID('dbo.Orders', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders(
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        TotalAmount DECIMAL(12,2) NOT NULL,
        Status NVARCHAR(40) NOT NULL DEFAULT N'Новый',
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        CONSTRAINT FK_Orders_Users FOREIGN KEY(UserId) REFERENCES dbo.Users(Id)
    );
END;

IF OBJECT_ID('dbo.OrderItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItems(
        Id INT IDENTITY(1,1) PRIMARY KEY,
        OrderId INT NOT NULL,
        ProductId INT NOT NULL,
        Quantity INT NOT NULL,
        UnitPrice DECIMAL(12,2) NOT NULL,
        CONSTRAINT FK_OrderItems_Orders FOREIGN KEY(OrderId) REFERENCES dbo.Orders(Id),
        CONSTRAINT FK_OrderItems_Products FOREIGN KEY(ProductId) REFERENCES dbo.Products(Id)
    );
END;

IF OBJECT_ID('dbo.CartItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CartItems(
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        ProductId INT NOT NULL,
        Quantity INT NOT NULL,
        CONSTRAINT FK_CartItems_Users FOREIGN KEY(UserId) REFERENCES dbo.Users(Id),
        CONSTRAINT FK_CartItems_Products FOREIGN KEY(ProductId) REFERENCES dbo.Products(Id)
    );
END;

IF NOT EXISTS(SELECT 1 FROM dbo.Users WHERE Role = N'Администратор')
BEGIN
    INSERT INTO dbo.Users(FullName, Email, Phone, PasswordHash, Role)
    VALUES(N'Главный администратор', N'admin@autosalon.local', N'+70000000000', @Hash, N'Администратор');
END;

IF NOT EXISTS(SELECT 1 FROM dbo.Products)
BEGIN
    INSERT INTO dbo.Products(Title, Description, Price, OldPrice, DiscountPercent)
    VALUES
    (N'Toyota Camry', N'Седан бизнес-класса', 35000, 39000, 10),
    (N'BMW X5', N'Кроссовер премиум', 78000, NULL, NULL),
    (N'Диагностика двигателя', N'Полная компьютерная диагностика', 450, 600, 25);
END;";
                using (var command = new SqlCommand(commandText, connection))
                {
                    command.Parameters.AddWithValue("@Hash", SecurityHelper.HashPassword("Admin123!"));
                    command.ExecuteNonQuery();
                }
            }

            var imagesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProductImages");
            if (!Directory.Exists(imagesPath))
            {
                Directory.CreateDirectory(imagesPath);
            }
        }
    }

    public static class Database
    {
        public static string ConnectionString =>
            ConfigurationManager.ConnectionStrings["AutoSalonDb"]?.ConnectionString
            ?? @"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;Initial Catalog=AutoSalonDb;";

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
