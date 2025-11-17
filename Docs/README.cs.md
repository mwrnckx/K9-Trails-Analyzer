# 🐾 K9 Trails Analyzer

**K9 Trails Analyzer** je nástroj určený pro psovody a trenéry zabývající se **mantrailingem** nebo **praktickým stopováním**.  
Vznikl z potřeby **objektivně vyhodnocovat tréninky** a **lépe chápat chování psa při práci na stopě**.  
Aplikace umožňuje **načítat a analyzovat GPX záznamy** z tréninků, měřit vzdálenosti, rychlost, stáří stopy i přesnost práce psa.  

Kromě analýzy nabízí i **unikátní knihovnu pro tvorbu overlay videí**, která zobrazují **trasu psa, trasu kladeče, směr a sílu větru** a další doplňkové informace v reálném čase.  
Tyto překryvy lze snadno kombinovat s videem z akční kamery a vytvořit tak **komplexní záznam trailu** – ideální pro výuku, rozbory i sdílení zkušeností.

Od verze **1.0.26** je do aplikace postupně začleňován i **bodovací systém**, použitelný pro **závody v mantrailingu** a **praktickém stopování**.

---

## 💡 Hlavní funkce

- **Načítání GPX souborů**  
  Automatické rozpoznání tras psa a kladeče i v oddělených souborech, s možností jejich sloučení.  
  Podporovány jsou záznamy z aplikací jako **Geo Tracker, OpenTracks, Mapy.cz, The Mantrailing App, [Locus Map](https://www.locusmap.app)** aj.

- **Analýza tras**
  - Výpočet **celkové délky**, **stáří** a **průměrné rychlosti**  
  - Možnost **filtrování podle data** a tréninkového období  
  - Výpočet **odchylek psa od trasy kladeče**

- **Vizualizace**
  - Grafy délky tras, rychlosti a stáří trailů v čase  
  - Možnost exportu grafů i dat  

- **Poznámky ke stopě**  
  Editační formulář umožňuje zaznamenat komentáře ke každému tréninku.  
  Poznámky se automaticky ukládají do overlay videa (formát PNG), takže se zobrazí i ve výsledném videu.

- **🎬 Tvorba overlay videí**  
  Generování videí s trasou psa, trasou kladeče, směrem a silou větru – s transparentním pozadím.  
  Výsledné video lze kombinovat v editorech (např. [Shotcut](https://shotcut.org/)) s kamerovým záznamem psa v terénu.

- **🏅 Bodovací systém pro závody**
  - Nález kladeče  
  - Rychlost práce  
  - Přesnost práce  
  - Čtení psa psovodem  
  - Vyhledání stopy při startu

- **📊 Export výsledků**  
  Export do formátů **CSV**, **TXT**, **RTF** – vhodné pro tisk nebo další zpracování v Excelu.

---

## 🧱 Instalace

1. Stáhni si aktuální verzi ze sekce [Releases](https://github.com/mwrnckx/K9-Trails-Analyzer/releases)  
2. Rozbal ZIP do libovolné složky  
3. Spusť `K9TrailsAnalyzer.exe`  
4. Pro vyzkoušení je v adresáři **Samples** několik ukázkových GPX souborů, které se automaticky načtou při prvním spuštění  
   Cílovou složku lze kdykoli změnit v menu **Soubor**

---

## ⚙️ Technické informace

- Vyžaduje **.NET 8.0 Runtime**  
- Pro generování videí využívá **[FFmpeg](https://ffmpeg.org/)**  
  (součástí balíčku ZIP – není nutná žádná instalace)

---

## 📂 Struktura projektu

Toto řešení obsahuje dva projekty:

- **K9-Trails-Analyzer** – hlavní aplikace (Windows Forms)  
  Slouží k načítání a analýze GPX dat.

- **[TrackVideoExporter](TrackVideoExporter.md)** – knihovna tříd pro generování overlay videí  
  Lze ji použít i samostatně v jiných projektech (např. pro výukové nebo analytické účely).

---

## 🌍 Lokalizace

Aplikace je vícejazyčná a podporuje tyto jazyky:

- 🇬🇧 English  
- 🇨🇿 Čeština  
- 🇩🇪 Deutsch  
- 🇵🇱 Polski  
- 🇷🇺 Русский  
- 🇺🇦 Українська  

---

## 📜 Licence

Projekt je šířen jako **UNLICENSED** – detaily viz soubor `UNLICENSE`.  
Jinými slovy: program můžeš volně používat, ale autor neručí za žádné škody, které ti způsobí (ani kdyby pes podle jeho analýzy našel místo kladeče sousedův gril — nebo horší, jeho klobásy 🐕🌭).

---
