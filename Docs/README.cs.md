# K9 Trails Analyzer

Tento projekt vznikl jako pomůcka pro psovody zabývající se mantrailingem nebo praktickými stopami. 
Umožňuje načíst GPX záznamy z tréninků a analyzovat je (délka, stáří apod.). 
Podstatnou součástí je i knihovna pro generování overlay videí (trasa psa a kladeče) pro použití do videí se záznamem práce psa. Na videu pak vidíte zároveň práci psa i jeho pozici na trailu, vzdálenost od trasy kladeče atd. Na videu je zobrazen i směr a síla větru! Je to vynikající pomůcka pro analýzu práce psa.


**K9 Trails Analyzer** je aplikace pro Windows, která zpracovává GPX soubory s GPS trasami kladeče (figuranta) a psa a poskytuje statistiky, jako je celková vzdálenost, stáří tras a průměrná rychlost psa. 
Testováno pro aplikace Geo Tracker, OpenTracks, Mapy.com, The Mantrailing App, Locus map a další aplikace. 

## Funkce

- **Čtení dat z GPX souborů**: Aplikace načítá GPX soubory z vybrané složky.
- **Filtrování podle data**.
- **Výpočet vzdálenosti**: Výpočet celkové délky tras ve vybraném období.
- **Výpočet stáří tras**: Pokud je v souboru zaznamenána trasa kladeče i psa, aplikace vypočítá stáří trasy.
- **Výpočet rychlosti**: Umožňuje vypočítat průměrnou rychlost psa na každé trase.
- **Export**: Výsledky lze exportovat do formátů RTF, TXT nebo CSV pro tisk nebo další analýzu v Excelu.
- **Zobrazení grafu**: Aplikace nabízí vizualizaci dat ve formě grafu, který zobrazuje celkovou vzdálenost, délky jednotlivých tras, stáří tras a rychlost psa v čase.
- ** Vytvoření videa na kterém je trasa kladeče a pohyb psa v reálném čase na transparentním pozadí. Ve videu je šipka znázorňující směr a sílu větru. Toto video může být ve vhodném editoru (jako například Shotcut) spojeno se záznamem trailu z akční kamery.

## 🛠️ Instalace

1. Stáhněte si soubor ZIP ze sekce [Releases](https://github.com/mwrnckx/K9-Trails-AnalyzerII/releases).
2. Rozbalte jej do libovolné složky.
3. Spusťte `K9TrailsAnalyzer.exe`.

🔧 Pro export videa se ujistěte, že je nainstalován nebo  v PATH přístupný soubor `ffmpeg.exe`. (Pokud pro střih videí používáte [Shotcut](https://shotcut.org/), je to v pořádku - ten obsahuje ffmpeg.)

## Závislosti

- .NET 8.0
- Aplikace používá externí software <a href="https://ffmpeg.org/">ffmpeg</a>, který musí být nainstalován pokud chcete vytvářet videa. Pokud k úpravě videa používáte <a href="https://www.shotcut.org/"> Shotcut</a>, tak ten obsahuje ffmpeg a máte po starostech 😉.

## 📂 Project structure

This solution contains two projects:

- `K9-Trails-Analyzer` — main Windows Forms application (GPX analysis)
- `TrackVideoExporter` — class library used to generate overlay videos


## Lokalizace
- **English**
- **Česky**
- **Deutch**
- **Polski**
- **Русский**
- **Українська**

## Licence
Tento projekt je licencován pod licencí MIT - podrobnosti naleznete v souboru LICENSE.md.

