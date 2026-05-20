using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CityMatch
{
    public class OllamaClient
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private const string OllamaUrl = "http://localhost:11434/api/generate";

        private const string SystemPrompt = @"Du bist ein Assistent für ein Reisebüro. Aus dem Freitext eines Kunden extrahierst du Schlagwörter für die Datenbanksuche nach passenden Reisezielen.

        Wähle ausschließlich Tags, deren Bedeutung wörtlich oder als klares Synonym im Kundenwunsch vorkommt. Lass alle Kategorien leer, in denen der Kunde nichts erwähnt hat.

        Erlaubte Tags pro Kategorie:
        - klima: warm, gemäßigt, kalt, tropisch
        - landschaft: berge, natur, see, stadt, strand, wüste
        - charakter: exotisch, familienfreundlich, günstig, luxus, ländlich, romantisch
        - kultur: architektur, geschichte, kaffeehaus, kultur, kunst, museen
        - aktivitaet: nachtleben, shopping, wandern
        - kueche: asiatisch, fleischlastig, mediterran

        Synonyme zuordnen: Meer → strand, heiß → warm, Gebirge → berge, Mittelmeer → strand.

        Beispiele für korrektes Verhalten:

        Kundenwunsch: ""warme Stadt mit viel Kultur""
        Antwort: {""klima"":[""warm""],""landschaft"":[""stadt""],""charakter"":[],""kultur"":[""kultur""],""aktivitaet"":[],""kueche"":[]}

        Kundenwunsch: ""wandern in den Bergen, ländlich und ruhig""
        Antwort: {""klima"":[],""landschaft"":[""berge""],""charakter"":[""ländlich""],""kultur"":[],""aktivitaet"":[""wandern""],""kueche"":[]}

        Kundenwunsch: ""Moin""
        Antwort: {""klima"":[],""landschaft"":[],""charakter"":[],""kultur"":[],""aktivitaet"":[],""kueche"":[]}

        Kundenwunsch: ""Apfelsine""
        Antwort: {""klima"":[],""landschaft"":[],""charakter"":[],""kultur"":[],""aktivitaet"":[],""kueche"":[]}

        Antworte ausschließlich auf Deutsch.

        Kundenwunsch: """;


        public async Task<string> FrageStellen(string kundenwunsch)
        {
            string vollerPrompt = SystemPrompt + kundenwunsch + "\"";

            var anfrage = new
            {
                model = "phi4-mini",
                prompt = vollerPrompt,
                stream = false,
                options = new { temperature = 0.0 },
                format = new
                {
                    type = "object",
                    properties = new
                    {
                        klima = new
                        {
                            type = "array",
                            items = new
                            {
                                type = "string",
                                @enum = new[] { "warm", "gemäßigt", "kalt", "tropisch" }
                            }
                        },
                        landschaft = new
                        {
                            type = "array",
                            items = new
                            {
                                type = "string",
                                @enum = new[] { "berge", "natur", "see", "stadt", "strand", "wüste" }
                            }
                        },
                        charakter = new
                        {
                            type = "array",
                            items = new
                            {
                                type = "string",
                                @enum = new[] { "exotisch", "familienfreundlich", "günstig", "luxus", "ländlich", "romantisch" }
                            }
                        },
                        kultur = new
                        {
                            type = "array",
                            items = new
                            {
                                type = "string",
                                @enum = new[] { "architektur", "geschichte", "kaffeehaus", "kultur", "kunst", "museen" }
                            }
                        },
                        aktivitaet = new
                        {
                            type = "array",
                            items = new
                            {
                                type = "string",
                                @enum = new[] { "nachtleben", "shopping", "wandern" }
                            }
                        },
                        kueche = new
                        {
                            type = "array",
                            items = new
                            {
                                type = "string",
                                @enum = new[] { "asiatisch", "fleischlastig", "mediterran" }
                            }
                        }
                    },
                    required = new[] { "klima", "landschaft", "charakter", "kultur", "aktivitaet", "kueche" }
                }
            };

            string jsonAnfrage = JsonSerializer.Serialize(anfrage);
            var content = new StringContent(jsonAnfrage, Encoding.UTF8, "application/json");

            HttpResponseMessage antwort = await httpClient.PostAsync(OllamaUrl, content);
            string antwortText = await antwort.Content.ReadAsStringAsync();

            using (JsonDocument doc = JsonDocument.Parse(antwortText))
            {
                string llmAntwort = doc.RootElement.GetProperty("response").GetString();
                return llmAntwort;
            }
        }
    }
}