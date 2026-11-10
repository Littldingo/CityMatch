[README-CityMatch.md](https://github.com/user-attachments/files/34630513/README-CityMatch.md)
# CityMatch

A WPF desktop application that finds matching travel destinations based on a free-text description of what the user is looking for. A local language model extracts keywords from the text, which are then matched against a city database.

Developed as the final project of my IT specialist training (Fachinformatiker für Anwendungsentwicklung) at BFW Schömberg.

![CityMatch](docs/screenshot.png)

## How it works

1. **Free-text input:** The user describes their ideal trip in their own words, e.g. *"a small, cozy town by the sea"*.
2. **Tag extraction:** The text is sent to a locally running language model (Phi-4-mini via Ollama). The model extracts tags from a fixed set of categories and returns them as JSON.
3. **Schema enforcement:** The request includes a JSON schema with allowed values for each category, so the model can only return valid tags.
4. **Matching:** The extracted tags are compared against the cities in a local SQLite database, and each city receives a weighted score.
5. **Result:** The best matches are displayed, including the criteria a city does *not* fulfil, so the user can see at a glance which compromises a destination involves.

### Tag categories

| Category | Examples |
|---|---|
| Climate | warm, temperate, cold, tropical |
| Landscape | mountains, nature, lake, city, beach, desert |
| Character | exotic, family-friendly, budget, luxury, rural, romantic |
| Culture | architecture, history, coffee houses, culture, art, museums |
| Activities | nightlife, shopping, hiking, party |
| Cuisine | Asian, meat-heavy, Mediterranean |
| Size | village, small town, medium-sized town, city, metropolis |

## Tech stack

- C# / WPF
- .NET Framework 4.7.2
- SQLite
- Ollama with Phi-4-mini (runs entirely locally, no cloud API)

## Requirements

- Windows
- Visual Studio (a recent version that supports `.slnx` solution files)
- [Ollama](https://ollama.com) installed and running

## Setup

1. Install Ollama and download the model:

   ```bash
   ollama pull phi4-mini
   ```

2. Make sure Ollama is running (it listens on `http://localhost:11434` by default).
3. Clone the repository and open `CityMatch.slnx` in Visual Studio.
4. Restore the NuGet packages and start the application.

The city database (`Staedte.db`) is included in the repository.

## Notes

The user interface, the tags and the city data are in German, as the application was developed for a German-speaking context.
