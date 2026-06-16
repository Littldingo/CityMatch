using System.Linq;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CityMatch
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OllamaClient ollama = new OllamaClient();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void SuchenButton_Click(object sender, RoutedEventArgs e)
        {
            string eingabe = EingabeBox.Text;
            if (string.IsNullOrWhiteSpace(eingabe))
                return;

            if (BegrüßungsText != null)
            {
                BegrüßungsText.Visibility = Visibility.Collapsed;
            }

            NachrichtHinzufuegen(eingabe, true);
            EingabeBox.Clear();

            if (eingabe.Length < 10)
            {
                NachrichtHinzufuegen("Bitte beschreiben Sie die Reisewünsche mit mindestens einem Satz", false);
                return;
            }
            else if (eingabe.Length >= 400)
            {
                NachrichtHinzufuegen("Bitte fassen Sie sich kürzer, die Anfrage konnte nicht verarbeitet werden", false);
                return;
            }

            // Lade-Anzeige hinzufügen und Referenz merken
            var ladeBubble = NachrichtHinzufuegenMitRueckgabe("Suche läuft...", false);


            // KI-Antwort holen
            string antwortJson = await ollama.FrageStellen(eingabe);

           // Lade-Bubble entfernen, sobald die Antwort da ist
            ChatPanel.Children.Remove(ladeBubble);

            if (string.IsNullOrEmpty(antwortJson))
            {
                NachrichtHinzufuegen("Die Suche ist derzeit nicht verfügbar. Bitte prüfen Sie, ob der KI-Dienst läuft, und versuchen Sie es erneut.", false);
                return;
            }

            // JSON parsen mit try/catch
            SuchKriterien kriterien;
            try
            {
                kriterien = JsonSerializer.Deserialize<SuchKriterien>(antwortJson);
            }
            catch (JsonException ex)
            {
                NachrichtHinzufuegen("Die KI hat eine ungültige Antwort geliefert. Bitte versuchen Sie es erneut.", false);
                System.Diagnostics.Debug.WriteLine($"JSON-Fehler: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Rohantwort: {antwortJson}");
                return;
            }

            if (kriterien == null)
            {
                NachrichtHinzufuegen("Die KI hat keine verwertbare Antwort geliefert. Bitte versuchen Sie es erneut.", false);
                return;
            }

            // Filter: nur charakter durch den Türsteher (da halluziniert Phi)
            var dbSyn = new DatenbankZugriff();
            var synonyme = dbSyn.SynonymeLaden();
            kriterien.charakter = TagFilter.FiltereListe(kriterien.charakter, eingabe, synonyme);
            kriterien.groesse = TagFilter.GroesseErkennen(eingabe);

            // Tags sammeln
            var alleTags = new List<string>();
            alleTags.AddRange(kriterien.klima);
            alleTags.AddRange(kriterien.landschaft);
            alleTags.AddRange(kriterien.charakter);
            alleTags.AddRange(kriterien.kultur);
            alleTags.AddRange(kriterien.aktivitaet);
            alleTags.AddRange(kriterien.kueche);
            alleTags.AddRange(kriterien.groesse);

            /* Debug-Ausgabe im Chat
              
            nachrichthinzufuegen(
            "[debug] erkannte tags:\n" +
            /$"klima: {string.join(", ", kriterien.klima)}\n" +
            $"landschaft: {string.join(", ", kriterien.landschaft)}\n" +
            $"charakter: {string.join(", ", kriterien.charakter)}\n" +
            $"kultur: {string.join(", ", kriterien.kultur)}\n" +
            $"aktivität: {string.join(", ", kriterien.aktivitaet)}\n" +
            $"küche: {string.join(", ", kriterien.kueche)}\n" +
            $"größe: {string.join(", ", kriterien.groesse)}",
            false);
            */

            // Datenbank abfragen
            var db = new DatenbankZugriff();
            var ergebnisse = db.FindeStaedte(alleTags.Distinct().ToList());

            if (ergebnisse.Count == 0)
            {
                NachrichtHinzufuegen("Es konnten keine Ziele gefunden werden, bitte versuchen Sie eine andere Beschreibung", false);
                return;
            }
            else if (ergebnisse.Count == 1)
            {
                NachrichtHinzufuegen("Hier ist das beste Ziel:", false);
            }
            else
            {
                NachrichtHinzufuegen("Hier sind die " + ergebnisse.Count + " besten Ziele:", false);
            }

            ErgebnisPanel.Children.Clear();
            int rang = 1;
            foreach (var (stadt, land, score, fehlendeTags) in ergebnisse)
            {
                EreignisHinzufuegen(rang, stadt, land, score, fehlendeTags);
                rang++;
            }
        }

        private void EingabeBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SuchenButton_Click(sender, e);
                e.Handled = true;
            }
        }

        // Hilfsmethode: erzeugt eine TextBox, die wie ein TextBlock aussieht, aber Markieren erlaubt
        private TextBox MarkierbaresTextfeld(string text, double fontSize = 15)
        {
            return new TextBox
            {
                Text = text,
                FontSize = fontSize,
                TextWrapping = TextWrapping.Wrap,
                IsReadOnly = true,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                Padding = new Thickness(0),
                Cursor = Cursors.Arrow
            };
        }

        private void NachrichtHinzufuegen(string text, bool istUser)
        {
            var bubble = new Border
            {
                Background = istUser ? Brushes.LightBlue : Brushes.LightGray,
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(10),
                Margin = new Thickness(5),
                MaxWidth = 400,
                HorizontalAlignment = istUser ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                Child = MarkierbaresTextfeld(text)
            };

            ChatPanel.Children.Add(bubble);
            ChatScroll.ScrollToEnd();
        }

        private Border NachrichtHinzufuegenMitRueckgabe(string text, bool istUser)
        {
            var bubble = new Border
            {
                Background = istUser ? Brushes.LightBlue : Brushes.LightGray,
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(10),
                Margin = new Thickness(5),
                MaxWidth = 400,
                HorizontalAlignment = istUser ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                Child = MarkierbaresTextfeld(text)
            };

            ChatPanel.Children.Add(bubble);
            ChatScroll.ScrollToEnd();
            return bubble;
        }

        private void EreignisHinzufuegen(int rang, string stadt, string land, int prozent, List<string> fehlendeTags)
        {
            // Container für die ganze Zeile (Hauptzeile + optionale Warnzeile)
            var container = new StackPanel { Margin = new Thickness(0, 5, 0, 15) };

            // Hauptzeile mit Rang/Stadt/Land/Prozent
            var zeile = new Grid();
            zeile.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            zeile.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
            zeile.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            zeile.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

            var rangText = MarkierbaresTextfeld(rang.ToString(), 20);
            rangText.VerticalAlignment = VerticalAlignment.Center;

            var stadtText = MarkierbaresTextfeld(stadt, 20);
            stadtText.VerticalAlignment = VerticalAlignment.Center;

            var landText = MarkierbaresTextfeld(land, 20);
            landText.VerticalAlignment = VerticalAlignment.Center;

            var prozentText = MarkierbaresTextfeld(prozent.ToString() + "%", 20);
            prozentText.VerticalAlignment = VerticalAlignment.Center;

            Grid.SetColumn(rangText, 0);
            zeile.Children.Add(rangText);
            Grid.SetColumn(stadtText, 1);
            zeile.Children.Add(stadtText);
            Grid.SetColumn(landText, 2);
            zeile.Children.Add(landText);
            Grid.SetColumn(prozentText, 3);
            zeile.Children.Add(prozentText);

            container.Children.Add(zeile);

            // Warnzeile mit fehlenden Tags, nur wenn welche fehlen
            if (fehlendeTags != null && fehlendeTags.Count > 0)
            {
                var warnText = new TextBox
                {
                    Text = "⚠ Abstriche: " + string.Join(", ", fehlendeTags),
                    FontSize = 13,
                    FontStyle = FontStyles.Italic,
                    Foreground = Brushes.DarkOrange,
                    Margin = new Thickness(50, 2, 0, 0),
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true,
                    BorderThickness = new Thickness(0),
                    Background = Brushes.Transparent,
                    Padding = new Thickness(0),
                    Cursor = Cursors.Arrow
                };
                container.Children.Add(warnText);
            }

            ErgebnisPanel.Children.Add(container);
        }
    }
}