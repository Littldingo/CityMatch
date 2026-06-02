using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CityMatch
{
    public static class TagFilter
    {
        // Behält nur Tags, die wörtlich oder als Synonym im Kundenwunsch stehen.
        public static List<string> FiltereListe(
            List<string> tags,
            string userInput,
            Dictionary<string, List<string>> synonyme)
        {
            var behaltene = new List<string>();
            foreach (var tag in tags)
            {
                if (IstGedeckt(tag, userInput, synonyme))
                    behaltene.Add(tag);
            }
            return behaltene.Distinct().ToList();
        }

        // Erkennt die Stadtgröße regelbasiert, an Phi vorbei.
        // Reihenfolge ist wichtig: spezifische/größere Begriffe zuerst.
        public static List<string> GroesseErkennen(string eingabe)
        {
            string t = eingabe.ToLowerInvariant().Replace("ß", "ss");
            var treffer = new List<string>();

            // Metropole / Weltstadt
            if (Regex.IsMatch(t, @"\bmetropole") || Regex.IsMatch(t, @"\bweltstadt"))
                treffer.Add("metropole");

            // Großstadt
            if (Regex.IsMatch(t, @"\bgrossstadt") || Regex.IsMatch(t, @"\bgrosse stadt"))
                treffer.Add("grossstadt");

            // Mittelstadt
            if (Regex.IsMatch(t, @"\bmittelstadt") || Regex.IsMatch(t, @"\bmittelgross")
                || Regex.IsMatch(t, @"\bmittelgrosse"))
                treffer.Add("mittelstadt");

            // Kleinstadt
            if (Regex.IsMatch(t, @"\bkleinstadt") || Regex.IsMatch(t, @"\bstadtchen")
                || Regex.IsMatch(t, @"\bstädtchen") || Regex.IsMatch(t, @"\bkleine stadt"))
                treffer.Add("kleinstadt");

            // Dorf
            if (Regex.IsMatch(t, @"\bdorf") || Regex.IsMatch(t, @"\bdörflich")
                || Regex.IsMatch(t, @"\bdorflich"))
                treffer.Add("dorf");

            // Vage Angaben: "nicht zu groß" -> kleinstadt + mittelstadt
            if (Regex.IsMatch(t, @"nicht zu gross") || Regex.IsMatch(t, @"\büberschaubar")
                || Regex.IsMatch(t, @"\buberschaubar"))
            {
                if (!treffer.Contains("kleinstadt")) treffer.Add("kleinstadt");
                if (!treffer.Contains("mittelstadt")) treffer.Add("mittelstadt");
            }

            return treffer.Distinct().ToList();
        }

        private static bool IstGedeckt(
            string tag,
            string text,
            Dictionary<string, List<string>> synonyme)
        {
            if (GanzesWort(text, tag))
                return true;

            if (synonyme.TryGetValue(tag, out var liste))
            {
                foreach (var synonym in liste)
                {
                    if (GanzesWort(text, synonym))
                        return true;
                }
            }
            return false;
        }

        // Präfix-Match mit vorderer Wortgrenze: "klein" trifft "kleine",
        // aber "see" trifft nicht "Museen". ß/ss wird normalisiert.
        private static bool GanzesWort(string text, string wort)
        {
            string t = Normalisieren(text);
            string w = Normalisieren(wort);
            string muster = $@"\b{Regex.Escape(w)}";
            return Regex.IsMatch(t, muster);
        }

        private static string Normalisieren(string s)
        {
            return s.ToLowerInvariant().Replace("ß", "ss");
        }
    }
}