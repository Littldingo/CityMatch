using System.Data.SQLite;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;

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
            verbindung = $"Data Source={dbPfad}";
        }

        public List<(string Stadt, string Land, int score)> FindeStaedte(List<string> tags)
        {
            var ergebnisse = new List<(string, string, int)>();
            if (tags.Count == 0)
            return ergebnisse;
            string platzhalter = string.Join(",", tags.Select((_, i) => $"@tag{i}"));

            string sql = $@"
            SELECT s.Name, s.Land, COUNT(*) AS treffer
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

                for(int i= 0; i < tags.Count; i++)
                {
                    cmd.Parameters.AddWithValue($"@tag{i}", tags[i]);
                }
                
                using (var reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        string name = reader.GetString(0);
                        string land = reader.GetString(1);
                        int treffer = reader.GetInt32(2);
                        int score = (int)((double)treffer / tags.Count * 100);
                        ergebnisse.Add((name, land, score));
                    }
                }
            }
            return ergebnisse;
        }
    }
}