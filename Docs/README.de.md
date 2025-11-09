# 🐾 K9 Trails Analyzer

**K9 Trails Analyzer** ist ein Werkzeug für Hundeführer und Trainer, die sich mit **Mantrailing** oder **praktischer Fährtenarbeit** beschäftigen.  
Es wurde entwickelt, um **Trainingseinheiten objektiv auszuwerten** und **das Verhalten des Hundes auf der Spur besser zu verstehen**.  
Die Anwendung ermöglicht das **Laden und Analysieren von GPX-Aufzeichnungen** aus Trainings, einschließlich Messung von Distanzen, Geschwindigkeit, Spuralter und Genauigkeit der Arbeit des Hundes.  

Neben der Analyse enthält sie eine **einzigartige Bibliothek zur Erstellung von Overlay-Videos**, die **die Spur des Hundes, die Route des Fährtenlegers, Windrichtung und -stärke** sowie weitere Informationen in Echtzeit anzeigen.  
Diese Overlays können leicht mit Actioncam-Aufnahmen kombiniert werden, um ein **komplettes Trail-Video** zu erstellen – ideal für Schulungen, Auswertungen oder Erfahrungsaustausch.

Seit Version **1.0.26** enthält die Anwendung zudem ein **Punktesystem**, das sich für **Mantrailing-** und **Fährtenprüfungen** eignet.

---

## 💡 Hauptfunktionen

- **GPX-Dateien laden**  
  Erkennt automatisch Hundespur und Fährtenlegerroute, auch wenn sie in getrennten Dateien gespeichert sind, und bietet deren Zusammenführung an.  
  Unterstützt werden Aufzeichnungen aus **Geo Tracker, OpenTracks, Mapy.com, The Mantrailing App, [Locus Map](https://www.locusmap.app)** u.a.

- **Spuranalyse**
  - Berechnung von **Gesamtdistanz**, **Spuralter** und **durchschnittlicher Geschwindigkeit**  
  - **Filterung nach Datum** oder Trainingszeitraum  
  - Berechnung der **Abweichung des Hundes** von der Route des Fährtenlegers  

- **Visualisierung**
  - Diagramme zu Spur­längen, Geschwindigkeiten und Alter im Zeitverlauf  
  - Möglichkeit zum Export von Diagrammen und Daten  

- **Notizen zur Spur**  
  Formular zur Eingabe von Anmerkungen zu jeder Spur.  
  Notizen werden automatisch als PNG in das Overlay-Video eingebettet und im Video angezeigt.

- **🎬 Erstellung von Overlay-Videos**  
  Generiert Videos mit Hundespur, Fährtenlegerroute sowie Windrichtung und -stärke – auf transparentem Hintergrund.  
  Diese Videos können in Editoren wie [Shotcut](https://shotcut.org/) mit Actioncam-Aufnahmen kombiniert werden.

- **🏅 Punktesystem für Wettkämpfe**
  - Finden des Fährtenlegers  
  - Arbeitsgeschwindigkeit  
  - Arbeitsgenauigkeit  
  - Lesen des Hundes durch den Hundeführer  

- **📊 Export von Ergebnissen**  
  Export in **CSV**, **TXT** oder **RTF** – geeignet zum Drucken oder für die Weiterverarbeitung in Excel.

---

## 🧱 Installation

1. Lade die aktuelle Version unter [Releases](https://github.com/mwrnckx/K9-Trails-Analyzer/releases) herunter  
2. Entpacke das ZIP-Archiv in einen beliebigen Ordner  
3. Starte `K9TrailsAnalyzer.exe`  
4. Im Ordner **Samples** befinden sich Beispiel-GPX-Dateien, die beim ersten Start automatisch geladen werden  
   Der Zielordner kann jederzeit im Menü **Datei** geändert werden.

---

## ⚙️ Technische Informationen

- Benötigt **.NET 8.0 Runtime**  
- Verwendet **[FFmpeg](https://ffmpeg.org/)** zur Videoerstellung  
  (bereits im ZIP-Paket enthalten – keine Installation erforderlich)

---

## 📂 Projektstruktur

Das Lösungsprojekt enthält zwei Teilprojekte:

- **K9-Trails-Analyzer** – Hauptanwendung (Windows Forms)  
  Zum Laden und Analysieren von GPX-Daten.

- **[TrackVideoExporter](TrackVideoExporter.md)** – Klassenbibliothek zur Erstellung von Overlay-Videos  
  Kann auch unabhängig in anderen Projekten verwendet werden (z. B. zu Schulungs- oder Analysezwecken).

---

## 🌍 Sprachen

Die Anwendung ist mehrsprachig und unterstützt:

- 🇬🇧 Englisch  
- 🇨🇿 Tschechisch  
- 🇩🇪 Deutsch  
- 🇵🇱 Polnisch  
- 🇷🇺 Russisch  
- 🇺🇦 Ukrainisch  

---

## 📜 Lizenz

Das Projekt wird unter **UNLICENSED** veröffentlicht – Details siehe Datei `UNLICENSE`.  
Mit anderen Worten: Du darfst das Programm frei verwenden, aber der Autor übernimmt keine Haftung für eventuelle Schäden  
(selbst wenn dein Hund laut Analyse statt des Fährtenlegers den Grill des Nachbarn – oder schlimmer, seine Würstchen – findet 🐕🌭).

---
