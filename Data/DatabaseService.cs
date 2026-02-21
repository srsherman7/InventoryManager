using Microsoft.Data.Sqlite;
using InventoryManager.Models;

namespace InventoryManager.Data
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string dbPath)
        {
            _connectionString = $"Data Source={dbPath}";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Inventory (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ProductName TEXT NOT NULL,
                    ProductType TEXT NOT NULL,
                    QuantityOnHand INTEGER NOT NULL DEFAULT 0,
                    PricePerItem REAL NOT NULL DEFAULT 0.0
                );";
            cmd.ExecuteNonQuery();
        }

        public List<InventoryItem> GetAll(string searchTerm = "")
        {
            var items = new List<InventoryItem>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.CommandText = "SELECT * FROM Inventory ORDER BY ProductName;";
            }
            else
            {
                cmd.CommandText = @"SELECT * FROM Inventory 
                                    WHERE ProductName LIKE $search OR ProductType LIKE $search 
                                    ORDER BY ProductName;";
                cmd.Parameters.AddWithValue("$search", $"%{searchTerm}%");
            }

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                items.Add(new InventoryItem
                {
                    Id = reader.GetInt32(0),
                    ProductName = reader.GetString(1),
                    ProductType = reader.GetString(2),
                    QuantityOnHand = reader.GetInt32(3),
                    PricePerItem = (decimal)reader.GetDouble(4)
                });
            }
            return items;
        }

        public InventoryItem? GetById(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Inventory WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new InventoryItem
                {
                    Id = reader.GetInt32(0),
                    ProductName = reader.GetString(1),
                    ProductType = reader.GetString(2),
                    QuantityOnHand = reader.GetInt32(3),
                    PricePerItem = (decimal)reader.GetDouble(4)
                };
            }
            return null;
        }

        public void Add(InventoryItem item)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Inventory (ProductName, ProductType, QuantityOnHand, PricePerItem)
                                VALUES ($name, $type, $qty, $price);";
            cmd.Parameters.AddWithValue("$name", item.ProductName);
            cmd.Parameters.AddWithValue("$type", item.ProductType);
            cmd.Parameters.AddWithValue("$qty", item.QuantityOnHand);
            cmd.Parameters.AddWithValue("$price", (double)item.PricePerItem);
            cmd.ExecuteNonQuery();
        }

        public void Update(InventoryItem item)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE Inventory 
                                SET ProductName = $name, ProductType = $type, 
                                    QuantityOnHand = $qty, PricePerItem = $price
                                WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$name", item.ProductName);
            cmd.Parameters.AddWithValue("$type", item.ProductType);
            cmd.Parameters.AddWithValue("$qty", item.QuantityOnHand);
            cmd.Parameters.AddWithValue("$price", (double)item.PricePerItem);
            cmd.Parameters.AddWithValue("$id", item.Id);
            cmd.ExecuteNonQuery();
        }

        public void UpdateQuantity(int id, int newQuantity)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Inventory SET QuantityOnHand = $qty WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$qty", newQuantity);
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Inventory WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }

        public (int TotalItems, decimal TotalWorth) GetTotals(string searchTerm = "")
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                cmd.CommandText = "SELECT COUNT(*), SUM(QuantityOnHand * PricePerItem) FROM Inventory;";
            }
            else
            {
                cmd.CommandText = @"SELECT COUNT(*), SUM(QuantityOnHand * PricePerItem) FROM Inventory
                                    WHERE ProductName LIKE $search OR ProductType LIKE $search;";
                cmd.Parameters.AddWithValue("$search", $"%{searchTerm}%");
            }

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                int count = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                decimal worth = reader.IsDBNull(1) ? 0m : (decimal)reader.GetDouble(1);
                return (count, worth);
            }
            return (0, 0m);
        }
    }
}
