# K9 Trails Analyzer

Tento projekt vznikl jako pomůcka pro psovody zabývající se mantrailingem nebo praktickými stopami. 
Umožňuje načíst GPX záznamy z tréninků a analyzovat je (délka, stáří apod.). 
Podstatnou součástí je i knihovna pro generování overlay videí (trasa psa a kladeče) pro použití do videí se záznamem práce psa. Na videu pak vidíte zároveň práci psa i jeho pozici na trailu, vzdálenost od trasy kladeče atd. Na videu je zobrazen i směr a síla větru! Je to vynikající pomůcka pro analýzu práce psa.


**K9 Trails Analyzer** je aplikace pro Windows, která zpracovává GPX soubory s GPS trasami kladeče (figuranta) a psa a poskytuje statistiky, jako je celková vzdálenost, stáří tras a průměrná rychlost psa. 
Testováno pro aplikace Geo Tracker, OpenTracks, Mapy.com, The Mantrailing App, Locus map a další aplikace. 

##  💡 Funkce

- **Čtení dat z GPX souborů**: Aplikace načítá GPX soubory z vybrané složky.
- **Filtrování podle data**.
- **Výpočet vzdálenosti**: Výpočet celkové délky tras ve vybraném období.
- **Výpočet stáří tras**: Pokud je v souboru zaznamenána trasa kladeče i psa, aplikace vypočítá stáří trasy.
- **Výpočet rychlosti**: Umožňuje vypočítat průměrnou rychlost psa na každé trase.
- **Export**: Výsledky lze exportovat do formátů RTF, TXT nebo CSV pro tisk nebo další analýzu v Excelu.
- **Zobrazení grafů**: Aplikace nabízí vizualizaci dat ve formě grafů, které zobrazují celkovou vzdálenost, délky jednotlivých tras, stáří tras a rychlost psa v čase.
- 🎬 **Vytvoření videa** na kterém je trasa kladeče a pohyb psa v reálném čase na transparentním pozadí. Ve videu je šipka znázorňující směr a sílu větru. Toto video může být ve vhodném editoru (jako například [Shotcut](https://shotcut.org/)) spojeno se záznamem trailu z akční kamery.

## 🛠️ Instalace

1. Stáhněte si soubor ZIP ze sekce [Releases](https://github.com/mwrnckx/K9-Trails-AnalyzerII/releases).
2. Rozbalte jej do libovolné složky.
3. Spusťte `K9TrailsAnalyzer.exe`.
4. V aplikaci je "přibaleno" několik gpx souborů ve složce Samples. Po spuštění (dokud nezměníte cílovou složku) se zpracovávají tyto soubory. Můžete si tedy bez obav vyzkoušet funkci programu na těchto souborech. Cílovou složku lze změnit v menu 'Soubor'.


## 🧱 Závislosti

- .NET 8.0
- Aplikace používá externí software <a href="https://ffmpeg.org/">ffmpeg</a>. Nejnovější verze ffmpeg.exe je součástí souboru zip, takže se memusíte vůbec  o nic starat.

## 📂 Struktura projektu

Toto řešení obsahuje dva projekty:

- **K9-Trails-Analyzer** - hlavní aplikace ve formě Windows Forms (analýza GPX).
- **[TrackVideoExporter](TrackVideoExporter.md)** - knihovna tříd sloužící ke generování překryvných videí
  - tato knihovna je použitelná i samostatně, můžete ji použít ve svém vlastním projektu


## 🌍 Lokalizace
- **English**
- **Česky**
- **Deutch**
- **Polski**
- **Русский**
- **Українська**

## 📜 Licence
Tento projekt je (tedy vlastně není 😉 ) licencován pod licencí UNLICENSED - podrobnosti naleznete v souboru UNLICENSE.

