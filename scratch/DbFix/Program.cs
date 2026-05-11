using System;
using Npgsql;

class Program
{
    static void Main(string[] args)
    {
        string connString = "Host=localhost;Port=5432;Database=art_galeri_db;Username=postgres;Password=2003";
        using var conn = new NpgsqlConnection(connString);
        conn.Open();

        // Tüm tabloları temizle (FK cascade)
        var tables = new[] {
            "\"Yorumlar\"", "\"Favoriler\"", "\"Siparisler\"", "\"Rezervasyonlar\"",
            "\"DestekTalepleri\"", "\"Artworks\"",
            "\"EgitmenProfiller\"", "\"SanatciProfiller\"", "\"MusteriProfiller\"",
            "\"Users\""
        };

        foreach (var table in tables)
        {
            try
            {
                using var cmd = new NpgsqlCommand($"TRUNCATE TABLE {table} CASCADE;", conn);
                cmd.ExecuteNonQuery();
                Console.WriteLine($"{table} temizlendi.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{table} temizlenemedi: {ex.Message}");
            }
        }

        Console.WriteLine("Tüm tablolar temizlendi.");
    }
}
