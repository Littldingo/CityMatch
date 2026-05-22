using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CityMatch
{
    public class DatenbankZugriff
    {
        private readonly string verbindung;

        public DatenbankZugriff()
        {
            string dbPfad = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "staedte.db"
            );
            System.Diagnostics.Debug.WriteLine($"DB-PFAD: {dbPfad}");
            System.Diagnostics.Debug.WriteLine($"DATEI EXISTIERT: {File.Exists(dbPfad)}");
            verbindung = $"Data Source={dbPfad}";
        }

        public List<(string Stadt, string Land, int score, List<string> fehlendeTags)> FindeStaedte(List<string> tags)
        {
            var ergebnisse = new List<(string, string, int, List<string>)>();
            if (tags.Count == 0)
                return ergebnisse;

            string platzhalter = string.Join(",", tags.Select((_, i) => $"@tag{i}"));

            // Erste Query: Treffer pro Stadt zählen + welche Tags getroffen wurden sammeln
            string sql = $@"
            SELECT s.Name, s.Land, COUNT(*) AS treffer, GROUP_CONCAT(t.name) AS getroffene_tags
            FROM Stadt s
            JOIN stadt_tag st ON s.stadt_id = st.stadt_id
            JOIN tag t ON st.tag_id = t.tag_id
            WHERE t.name IN ({platzhalter})
            GROUP BY s.stadt_id, s.Name, s.Land
            ORDER BY treffer DESC
            LIMIT 5";

            using (var conn = new SQLiteConnection(verbindung))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = sql;

                for (int i = 0; i < tags.Count; i++)
                {
                    cmd.Parameters.AddWithValue($"@tag{i}", tags[i]);
                }

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string name = reader.GetString(0);
                        string land = reader.GetString(1);
                        int treffer = reader.GetInt32(2);
                        string getroffeneTagsRaw = reader.IsDBNull(3) ? "" : reader.GetString(3);

                        int score = (int)((double)treffer / tags.Count * 100);

                        // Welche Tags fehlen? = angeforderte minus getroffene
                        var getroffeneTags = getroffeneTagsRaw
                            .Split(',')
                            .Select(t => t.Trim())
                            .ToHashSet();
                        getroffeneTagsRaw = reader.IsDBNull(3) ? "" : reader.GetString(3);
                        System.Diagnostics.Debug.WriteLine($"[{name}] rohstring: '{getroffeneTagsRaw}'");
                        var fehlendeTags = tags
                            .Where(t => !getroffeneTags.Contains(t))
                            .ToList();

                        ergebnisse.Add((name, land, score, fehlendeTags));
                    }
                }
            }
            return ergebnisse;
        }
    }
}