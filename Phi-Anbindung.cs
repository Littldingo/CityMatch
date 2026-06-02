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

        private const string SystemPrompt = @"Du bist ein Extraktions-Modul für ein Reisebüro. Aus dem Freitext eines Kunden extrahierst du Schlagwörter für die Datenbanksuche nach passenden Reisezielen. Du interpretierst nicht und plauderst nicht – du extrahierst.

Wähle ausschließlich Tags, deren Bedeutung wörtlich oder als klares Synonym im Kundenwunsch vorkommt. Lass jede Kategorie leer, in der der Kunde nichts erwähnt. Füge niemals einen Tag hinzu, nur weil er thematisch passen könnte.

Erlaubte Tags pro Kategorie:
- klima: warm, gemäßigt, kalt, tropisch
- landschaft: berge, natur, see, stadt, strand, wüste
- charakter: exotisch, familienfreundlich, günstig, luxus, ländlich, romantisch, luxuriös 
- kultur: architektur, geschichte, kaffeehaus, kultur, kunst, museen
- aktivitaet: nachtleben, shopping, wandern, bummeln, einkaufsmöglichkeiten, party, feiern, läden, geschäfte
- kueche: asiatisch, fleischlastig, mediterran
- groesse: dorf, kleinstadt, mittelstadt, grossstadt, metropole

Bei allgemeinem ""Kultur"" oder ""kulturell"" wähle NUR das Tag ""kultur"". Die Tags ""architektur"", ""geschichte"", ""kaffeehaus"", ""kunst"", ""museen"" nur bei wörtlicher Nennung.


Nutze dabei jedes Tag nur 1 mal

Synonyme zuordnen: Meer/Mittelmeer/Küste → strand, heiß → warm, Gebirge → berge, Großstadt → grossstadt, weltstadt → metropole, Städtchen/beschaulich → kleinstadt.

Größenregel: Bei vager Angabe nur benachbarte Stufen wählen, die zur Aussage passen – niemals das ganze Spektrum.

Beispiel:

Dorf <-- ""eher klein"" --> kleinstadt


Antworte ausschließlich mit einem gültigen JSON-Objekt mit genau diesen Keys, ohne Markdown, ohne Erklärung:
{""klima"":[],""landschaft"":[],""charakter"":[],""kultur"":[],""aktivitaet"":[],""kueche"":[],""groesse"":[]}

Beispiele:

Kundenwunsch: ""warme Stadt mit viel Kultur""
Antwort: {""klima"":[""warm""],""landschaft"":[""stadt""],""charakter"":[],""kultur"":[""kultur""],""aktivitaet"":[],""kueche"":[],""groesse"":[]}

Kundenwunsch: ""wandern in den Bergen, ländlich und ruhig""
Antwort: {""klima"":[],""landschaft"":[""berge""],""charakter"":[""ländlich""],""kultur"":[],""aktivitaet"":[""wandern""],""kueche"":[],""groesse"":[]}

Kundenwunsch: ""eine kleine, gemütliche Stadt am Meer""
Antwort: {""klima"":[],""landschaft"":[""strand"",""stadt""],""charakter"":[],""kultur"":[],""aktivitaet"":[],""kueche"":[],""groesse"":[""kleinstadt""]}

Kundenwunsch: ""eine große Stadt""
Antwort: {""klima"":[],""landschaft"":[""stadt""],""charakter"":[],""kultur"":[],""aktivitaet"":[],""kueche"":[],""groesse"":[""grossstadt""]}

Kundenwunsch: ""Metropole mit Nachtleben und Shopping""
Antwort: {""klima"":[],""landschaft"":[""stadt""],""charakter"":[],""kultur"":[],""aktivitaet"":[""nachtleben"",""shopping""],""kueche"":[],""groesse"":[""metropole""]}

Kundenwunsch: ""ein schöner, entspannter Urlaub zu zweit""
Antwort: {""klima"":[],""landschaft"":[],""charakter"":[],""kultur"":[],""aktivitaet"":[],""kueche"":[],""groesse"":[]}

Kundenwunsch: ""Moin""
Antwort: {""klima"":[],""landschaft"":[],""charakter"":[],""kultur"":[],""aktivitaet"":[],""kueche"":[],""groesse"":[]}

Kundenwunsch: ""Apfelsine""
Antwort: {""klima"":[],""landschaft"":[],""charakter"":[],""kultur"":[],""aktivitaet"":[],""kueche"":[],""groesse"":[]}

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
                                @enum = new[] { "nachtleben", "shopping", "wandern", "party", "feiern", "läden", "geschäfte" }
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

            HttpResponseMessage antwort;

            string antwortText;
            try
            {
                antwort = await httpClient.PostAsync(OllamaUrl, content);
                antwort.EnsureSuccessStatusCode();
                antwortText = await antwort.Content.ReadAsStringAsync();
            }
            catch(HttpRequestException)
            {
                return null;
            }

            using (JsonDocument doc = JsonDocument.Parse(antwortText))
            {
                string llmAntwort = doc.RootElement.GetProperty("response").GetString();
                return llmAntwort;
            }
        }
    }
}