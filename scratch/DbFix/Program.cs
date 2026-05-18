using System;
using Npgsql;

class Program
{
    static void Main(string[] args)
    {
        string connString = "Host=localhost;Port=5432;Database=art_galeri_db;Username=postgres;Password=2003";
        using var conn = new NpgsqlConnection(connString);
        conn.Open();

        // lara cihan (UserID=9) için kupon
        using (var cmd = new NpgsqlCommand("UPDATE \"Kampanyalar\" SET \"TargetUserID\" = 9, \"Ad\" = 'lara hanıma özel fırsat', \"KuponKodu\" = 'LARA61' WHERE \"KampanyaID\" = 7", conn))
        {
            cmd.ExecuteNonQuery();
            Console.WriteLine("Kupon LARA61 UserID=9 (Lara Cihan) için güncellendi.");
        }

        // Eren Can (UserID=15) için yeni kupon oluşturalım
        using (var cmd = new NpgsqlCommand(@"
            INSERT INTO ""Kampanyalar"" (""Ad"", ""Aciklama"", ""IndirimOrani"", ""KuponKodu"", ""BaslangicTarihi"", ""BitisTarihi"", ""Aktif"", ""TargetUserID"")
            VALUES ('eren beye özel', 'eren cana özel kampanya', 30, 'EREN30', NOW() - INTERVAL '1 day', NOW() + INTERVAL '5 days', TRUE, 15)
            ON CONFLICT DO NOTHING;", conn))
        {
            cmd.ExecuteNonQuery();
            Console.WriteLine("Kupon EREN30 UserID=15 (Eren Can) için oluşturuldu.");
        }

        Console.WriteLine("\n=== CAMPAIGNS ===");
        using (var cmd = new NpgsqlCommand("SELECT \"KampanyaID\", \"Ad\", \"KuponKodu\", \"Aktif\", \"TargetUserID\", \"HedefRolID\" FROM \"Kampanyalar\"", conn))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var targetUser = reader.IsDBNull(4) ? "NULL" : reader.GetInt32(4).ToString();
                var targetRole = reader.IsDBNull(5) ? "NULL" : reader.GetInt32(5).ToString();
                Console.WriteLine($"ID: {reader.GetInt32(0)}, Name: {reader.GetString(1)}, Code: {reader.GetString(2)}, Active: {reader.GetBoolean(3)}, TargetUser: {targetUser}, TargetRole: {targetRole}");
            }
        }
    }
}
