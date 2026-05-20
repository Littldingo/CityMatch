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

            // Lade-Anzeige hinzufügen und Referenz merken
            var ladeBubble = NachrichtHinzufuegenMitRueckgabe("Suche läuft...", false);

            string antwortJson = await ollama.FrageStellen(eingabe);
            SuchKriterien kriterien = JsonSerializer.Deserialize<SuchKriterien>(antwortJson);

            var alleTags = new List<string>();
            alleTags.AddRange(kriterien.klima);
            alleTags.AddRange(kriterien.landschaft);
            alleTags.AddRange(kriterien.charakter);
            alleTags.AddRange(kriterien.kultur);
            alleTags.AddRange(kriterien.aktivitaet);
            alleTags.AddRange(kriterien.kueche);

            var db = new DatenbankZugriff();
            var ergebnisse = db.FindeStaedte(alleTags);

            ErgebnisPanel.Children.Clear();
            int rang = 1;
            foreach (var (stadt, land, score) in ergebnisse)
            {
                EreignisHinzufuegen(rang, stadt, land, score);
                rang++;
            }

            // Lade-Anzeige entfernen
            ChatPanel.Children.Remove(ladeBubble);
            if (eingabe.Length < 10) 
            {
                NachrichtHinzufuegen("Bitte beschreiben Sie die Reisewünsche mit mindestens einem Satz", false);
            }
            else if (eingabe.Length >= 200)
            {
                NachrichtHinzufuegen("Bitte fassen Sie sich kürzer, die Anfrage konnte nicht verarbeitet werden", false);
            }
            else if(ergebnisse.Count == 0)
            {
                NachrichtHinzufuegen("Es konnten keine Ziele gefunden werden, bitte versuchen Sie eine andere Beschreibung", false);
            }

            else
            {
                NachrichtHinzufuegen("Hier sind die " + ergebnisse.Count + " besten Ziele:", false);
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
                Child = new TextBlock
                {
                    Text = text,
                    FontSize = 15,
                    TextWrapping = TextWrapping.Wrap
                }
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
                Child = new TextBlock
                {
                    Text = text,
                    FontSize = 15,
                    TextWrapping = TextWrapping.Wrap
                }
            };

            ChatPanel.Children.Add(bubble);
            ChatScroll.ScrollToEnd();
            return bubble;
        }

        private void EreignisHinzufuegen(int rang, string stadt, string land, int prozent)
        {
            var zeile = new Grid { Margin = new Thickness(0, 5, 0, 15) };
            zeile.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            zeile.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
            zeile.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            zeile.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

            var rangText = new TextBlock
            {
                Text = rang.ToString(),
                FontSize = 20,
                VerticalAlignment = VerticalAlignment.Center
            };
            var stadtText = new TextBlock
            {
                Text = stadt.ToString(),
                FontSize = 20,
                VerticalAlignment = VerticalAlignment.Center
            };
            var landText = new TextBlock
            {
                Text = land.ToString(),
                FontSize = 20,
                VerticalAlignment = VerticalAlignment.Center
            };
            var prozentText = new TextBlock
            {
                Text = prozent.ToString() + "%",
                FontSize = 20,
                VerticalAlignment = VerticalAlignment.Center
            };

            Grid.SetColumn(rangText, 0);
            zeile.Children.Add(rangText);
            Grid.SetColumn(stadtText, 1);
            zeile.Children.Add(stadtText);
            Grid.SetColumn(landText, 2);
            zeile.Children.Add(landText);
            Grid.SetColumn(prozentText, 3);
            zeile.Children.Add(prozentText);

            ErgebnisPanel.Children.Add(zeile);
        }
    }
}