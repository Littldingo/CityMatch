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
        - groesse: dorf, kleinstadt, mittelstadt, grossstadt, metropole, grosse stadt, große stadt

        Synonyme zuordnen: Meer → strand, heiß → warm, Gebirge → berge, Mittelmeer → strand, Großstadt → grossstadt, Weltstadt → metropole, kleines Städtchen → kleinstadt, beschaulich → kleinstadt, nicht zu groß → kleinstadt, mittelstadt.

        Bei vagen Größenangaben mehrere passende Tags wählen. Beispiel: ""nicht zu groß"" → kleinstadt UND mittelstadt. ""richtig groß"" → grossstadt UND metropole.

        

        Beispiele für korrektes Verhalten:

        Bei ""nicht zu groß"", ""nicht zu klein"", ""mittel"" oder ähnlich vagen Angaben:
        Wähle MAXIMAL ZWEI benachbarte Größen-Tags, niemals mehr.
gib 

        Beispiele:
        ""nicht zu groß"" → kleinstadt, mittelstadt (NICHT grossstadt oder metropole)
        ""nicht zu klein"" → mittelstadt, grossstadt (NICHT dorf oder kleinstadt)
        ""mittelgroß"" → mittelstadt
        ""weder Dorf noch Metropole"" → kleinstadt, mittelstadt, grossstadt
        ""überschaubar"" → kleinstadt, mittelstadt

        Kundenwunsch: ""warme Stadt mit viel Kultur""
        Antwort: {""klima"":[""warm""],""landschaft"":[""stadt""],""charakter"":[],""kultur"":[""kultur""],""aktivitaet"":[],""kueche"":[],""groesse"":[]}

        Kundenwunsch: ""wandern in den Bergen, ländlich und ruhig""
        Antwort: {""klima"":[],""landschaft"":[""berge""],""charakter"":[""ländlich""],""kultur"":[],""aktivitaet"":[""wandern""],""kueche"":[],""groesse"":[]}

        Kundenwunsch: ""eine kleine, gemütliche Stadt am Meer""
        Antwort: {""klima"":[],""landschaft"":[""strand"",""stadt""],""charakter"":[],""kultur"":[],""aktivitaet"":[],""kueche"":[],""groesse"":[""kleinstadt""]}

        Kundenwunsch: ""Metropole mit Nachtleben und Shopping""
        Antwort: {""klima"":[],""landschaft"":[""stadt""],""charakter"":[],""kultur"":[],""aktivitaet"":[""nachtleben"",""shopping""],""kueche"":[],""groesse"":[""metropole""]}

        Kundenwunsch: ""Moin""
        Antwort: {""klima"":[],""landschaft"":[],""charakter"":[],""kultur"":[],""aktivitaet"":[],""kueche"":[],""groesse"":[]}

        Kundenwunsch: ""Apfelsine""
        Antwort: {""klima"":[],""landschaft"":[],""charakter"":[],""kultur"":[],""aktivitaet"":[],""kueche"":[],""groesse"":[]}

        Antworte ausschließlich auf Deutsch.

        Gib deine Antworten ausschließlich als gültiges Json Format zurück

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
                        },
                        groesse = new
                        {
                            type = "array",
                            items = new
                            {
                                type = "string",
                                @enum = new[] { "dorf", "kleinstadt", "mittelstadt", "grossstadt", "metropole" }
                            }
                        }
                    },
                    required = new[] { "klima", "landschaft", "charakter", "kultur", "aktivitaet", "kueche", "groesse" }
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