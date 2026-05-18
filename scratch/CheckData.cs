using System;
using Npgsql;

class Program
{
    static void Main(string[] args)
    {
        string connString = "Host=localhost;Port=5432;Database=art_galeri_db;Username=postgres;Password=2003";
        using var conn = new NpgsqlConnection(connString);
        conn.Open();

        Console.WriteLine("\n=== USERS ===");
        using (var cmd = new NpgsqlCommand("SELECT \"UserID\", \"Ad\", \"Soyad\", \"Email\", \"RolID\" FROM \"Users\"", conn))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                Console.WriteLine($"ID: {reader.GetInt32(0)}, Name: {reader.GetString(1)} {reader.GetString(2)}, Email: {reader.GetString(3)}, Role: {reader.GetInt32(4)}");
            }
        }

        Console.WriteLine("\n=== ORDERS ===");
        using (var cmd = new NpgsqlCommand("SELECT \"SiparisID\", \"UserID\", \"ArtworkID\", \"Tutar\", \"Durum\" FROM \"Siparisler\"", conn))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var artworkId = reader.IsDBNull(2) ? "NULL" : reader.GetInt32(2).ToString();
                Console.WriteLine($"ID: {reader.GetInt32(0)}, UserID: {reader.GetInt32(1)}, ArtworkID: {artworkId}, Tutar: {reader.GetDecimal(3)}, Status: {reader.GetString(4)}");
            }
        }

        Console.WriteLine("\n=== COMMENTS ===");
        using (var cmd = new NpgsqlCommand("SELECT \"YorumID\", \"UserID\", \"ArtworkID\", \"Icerik\", \"Dogrulanmis\" FROM \"Yorumlar\"", conn))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var artworkId = reader.IsDBNull(2) ? "NULL" : reader.GetInt32(2).ToString();
                Console.WriteLine($"ID: {reader.GetInt32(0)}, UserID: {reader.GetInt32(1)}, ArtworkID: {artworkId}, Content: {reader.GetString(3)}, Verified: {reader.GetBoolean(4)}");
            }
        }
    }
}
