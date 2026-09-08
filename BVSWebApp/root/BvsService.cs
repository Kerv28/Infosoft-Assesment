using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace BVSWebApp
{
    public class Customer
    {
        public int CustomerID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
    }

    public class Video
    {
        public int VideoID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = "VCD"; // VCD or DVD
        public double RentalRate => Category == "VCD" ? 25.0 : 50.0;
        public int AllowedDays { get; set; } = 1; // 1 to 3 days
        public int TotalQuantity { get; set; }
    }

    public class InventoryReportItem
    {
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int In { get; set; }
        public int Out { get; set; }
    }

    public class CustomerRentalReportItem
    {
        public string CustomerName { get; set; } = string.Empty;
        public string VideoTitle { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string RentDate { get; set; } = string.Empty;
        public string DueDate { get; set; } = string.Empty;
    }

    public class BvsService
    {
        private readonly string _connectionString = "Data Source=bvs_store.db";

        public BvsService()
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Customers (
                    CustomerID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    ContactNumber TEXT
                );

                CREATE TABLE IF NOT EXISTS Videos (
                    VideoID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    Category TEXT CHECK(Category IN ('VCD', 'DVD')) NOT NULL,
                    RentalRate REAL NOT NULL,
                    AllowedDays INTEGER CHECK(AllowedDays BETWEEN 1 AND 3) NOT NULL,
                    TotalQuantity INTEGER NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Rentals (
                    RentalID INTEGER PRIMARY KEY AUTOINCREMENT,
                    CustomerID INTEGER REFERENCES Customers(CustomerID),
                    VideoID INTEGER REFERENCES Videos(VideoID),
                    RentDate TEXT NOT NULL,
                    DueDate TEXT NOT NULL,
                    ReturnDate TEXT NULL,
                    OverduePenalty REAL DEFAULT 0.0
                );";
            cmd.ExecuteNonQuery();
        }

        // --- CUSTOMER ACTIONS ---
        public List<Customer> GetCustomers()
        {
            var list = new List<Customer>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = new SqliteCommand("SELECT * FROM Customers", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Customer {
                    CustomerID = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    ContactNumber = reader.IsDBNull(2) ? "" : reader.GetString(2)
                });
            }
            return list;
        }

        public void AddCustomer(string name, string contact)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = new SqliteCommand("INSERT INTO Customers (Name, ContactNumber) VALUES (@n, @c)", conn);
            cmd.Parameters.AddWithValue("@n", name);
            cmd.Parameters.AddWithValue("@c", contact);
            cmd.ExecuteNonQuery();
        }

        // --- VIDEO ACTIONS ---
        public List<Video> GetVideos()
        {
            var list = new List<Video>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = new SqliteCommand("SELECT * FROM Videos ORDER BY Title ASC", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Video {
                    VideoID = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Category = reader.GetString(2),
                    AllowedDays = reader.GetInt32(4),
                    TotalQuantity = reader.GetInt32(5)
                });
            }
            return list;
        }

        public void AddVideo(string title, string category, int days, int quantity)
        {
            double rate = category == "VCD" ? 25.0 : 50.0;
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = new SqliteCommand("INSERT INTO Videos (Title, Category, RentalRate, AllowedDays, TotalQuantity) VALUES (@t, @c, @r, @d, @q)", conn);
            cmd.Parameters.AddWithValue("@t", title);
            cmd.Parameters.AddWithValue("@c", category);
            cmd.Parameters.AddWithValue("@r", rate);
            cmd.Parameters.AddWithValue("@d", days);
            cmd.Parameters.AddWithValue("@q", quantity);
            cmd.ExecuteNonQuery();
        }

        public void DeleteVideo(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = new SqliteCommand("DELETE FROM Videos WHERE VideoID = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        // --- RENTAL MODULE ---
        public void RentVideo(int customerId, int videoId, int days)
        {
            var rentDate = DateTime.Now;
            var dueDate = rentDate.AddDays(days);

            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = new SqliteCommand("INSERT INTO Rentals (CustomerID, VideoID, RentDate, DueDate) VALUES (@c, @v, @rd, @dd)", conn);
            cmd.Parameters.AddWithValue("@c", customerId);
            cmd.Parameters.AddWithValue("@v", videoId);
            cmd.Parameters.AddWithValue("@rd", rentDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@dd", dueDate.ToString("yyyy-MM-dd"));
            cmd.ExecuteNonQuery();
        }

        public double ReturnVideo(int rentalId)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var checkCmd = new SqliteCommand("SELECT DueDate FROM Rentals WHERE RentalID = @id", conn);
            checkCmd.Parameters.AddWithValue("@id", rentalId);
            var result = checkCmd.ExecuteScalar();

            if (result == null) return -1; // Not found

            DateTime dueDate = DateTime.Parse(result.ToString()!);
            DateTime returnDate = DateTime.Now;
            double penalty = 0.0;

            if (returnDate.Date > dueDate.Date)
            {
                int overdueDays = (returnDate.Date - dueDate.Date).Days;
                penalty = overdueDays * 5.0; // ₱5 per day overdue
            }

            var updateCmd = new SqliteCommand("UPDATE Rentals SET ReturnDate = @rd, OverduePenalty = @p WHERE RentalID = @id", conn);
            updateCmd.Parameters.AddWithValue("@rd", returnDate.ToString("yyyy-MM-dd"));
            updateCmd.Parameters.AddWithValue("@p", penalty);
            updateCmd.Parameters.AddWithValue("@id", rentalId);
            updateCmd.ExecuteNonQuery();

            return penalty;
        }

        // --- REPORTS ---
        public List<InventoryReportItem> GetInventoryReport()
        {
            var list = new List<InventoryReportItem>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            string query = @"
                SELECT 
                    v.Title, 
                    v.Category, 
                    (v.TotalQuantity - COUNT(r.RentalID)) AS [In], 
                    COUNT(r.RentalID) AS [Out]
                FROM Videos v
                LEFT JOIN Rentals r ON v.VideoID = r.VideoID AND r.ReturnDate IS NULL
                GROUP BY v.VideoID, v.Title, v.Category, v.TotalQuantity
                ORDER BY v.Title ASC;";
            
            var cmd = new SqliteCommand(query, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new InventoryReportItem {
                    Title = reader.GetString(0),
                    Category = reader.GetString(1),
                    In = reader.GetInt32(2),
                    Out = reader.GetInt32(3)
                });
            }
            return list;
        }

        public List<CustomerRentalReportItem> GetCustomerRentalsReport()
        {
            var list = new List<CustomerRentalReportItem>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            string query = @"
                SELECT 
                    c.Name AS CustomerName, 
                    v.Title AS VideoTitle, 
                    v.Category, 
                    r.RentDate, 
                    r.DueDate
                FROM Rentals r
                JOIN Customers c ON r.CustomerID = c.CustomerID
                JOIN Videos v ON r.VideoID = v.VideoID
                WHERE r.ReturnDate IS NULL
                ORDER BY c.Name ASC;";

            var cmd = new SqliteCommand(query, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new CustomerRentalReportItem {
                    CustomerName = reader.GetString(0),
                    VideoTitle = reader.GetString(1),
                    Category = reader.GetString(2),
                    RentDate = reader.GetString(3),
                    DueDate = reader.GetString(4)
                });
            }
            return list;
        }
    }
}