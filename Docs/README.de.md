# K9 Trails Analyzer

Dieses Projekt wurde als Werkzeug für Hundeführer entwickelt, die sich mit Mantrailing oder Geruchsspurverfolgung beschäftigen.  
Es ermöglicht das Laden von GPX-Dateien aus Trainingseinheiten und deren Analyse (Entfernung, Alter der Spur usw.).  
Ein zentraler Bestandteil des Projekts ist eine Bibliothek zur Erstellung von Overlay-Videos (Anzeige der Positionen von Hund und Spur), die mit Videoaufnahmen der Arbeit des Hundes kombiniert werden können. Das fertige Video zeigt sowohl das Verhalten des Hundes als auch seine Echtzeit-Position auf der Spur, inklusive Abstand zum Fährtenleger und visualisierter Windrichtung und -stärke!  
Ein leistungsstarkes Werkzeug zur Analyse der Leistung deines Hundes.

**K9 Trails Analyzer** ist eine Windows-Anwendung zur Verarbeitung von GPX-Dateien, die GPS-Tracks des Fährtenlegers und des Hundes enthalten, und stellt Statistiken wie Gesamtdistanz, Spuralter und Durchschnittsgeschwindigkeit bereit.  
Getestet mit Apps wie Geo Tracker, OpenTracks, Mapy.com, The Mantrailing App, Locus Map und anderen.

---

## 💡 Funktionen

- **Liest GPX-Dateien**: Lade Tracks aus einem beliebigen Ordner.
- **Datumsfilterung**: Fokussiere auf Spuren innerhalb eines ausgewählten Zeitfensters.
- **Distanzberechnung**: Berechne die Gesamt- und Einzeltrack-Distanz.
- ⏳ **Spuralter**: Wenn sowohl Fährtenleger- als auch Hundespur verfügbar sind, berechnet die App, wie alt die Spur war, als der Hund ihr folgte.
- **Geschwindigkeitsanalyse**: Berechnet die durchschnittliche Geschwindigkeit des Hundes pro Spur.
- **Exportoptionen**: Exportiere Ergebnisse in RTF, TXT oder CSV für den Druck oder zur weiteren Analyse in Excel.
- **Diagramme und Visualisierung**: Zeigt Zusammenfassungen wie Gesamtdistanz, Tracklängen, Spuralter und Hundegeschwindigkeit über die Zeit.
- 🎬 **Videoerstellung**: Erstelle transparente Overlay-Videos, die die Bewegungen von Fährtenleger und Hund in Echtzeit zeigen. Ein Windpfeil visualisiert Windrichtung und -stärke. Dieses Overlay kann mit Actioncam-Videos in einem Editor wie [Shotcut](https://shotcut.org/) kombiniert werden.

---

## 🛠️ Installation

1. Lade die ZIP-Datei aus dem [Releases-Bereich](https://github.com/mwrnckx/K9-Trails-AnalyzerII/releases) herunter.
2. Entpacke sie in einen beliebigen Ordner.
3. Starte `K9TrailsAnalyzer.exe`.
4. In der Anwendung befinden sich mehrere gpx-Dateien im Ordner Samples. Diese Dateien werden nach dem Start des Programms verarbeitet (bis Sie den Zielordner ändern). So können Sie die Funktionalität des Programms mit diesen Dateien sicher testen. Der Zielordner kann im Menü "Datei" geändert werden.

---

## 🧱 Abhängigkeiten

- [.NET 8.0 Runtime](https://dotnet.microsoft.com/en-us/download)
- Die Anwendung verwendet externe Software: [ffmpeg](https://ffmpeg.org/)  
  Die aktuelle `ffmpeg.exe` ist bereits in der ZIP-Datei enthalten – keine manuelle Einrichtung erforderlich.

---

## 📂 Projektstruktur

Dieses Repository enthält zwei Teilprojekte:

- **K9-Trails-Analyzer** – die Haupt-Windows-Forms-Anwendung (GPX-Analyse)
- **[TrackVideoExporter](TrackVideoExporter.md)** – eine Klassenbibliothek zur Erstellung von Overlay-Videos
  - Diese Bibliothek ist modular aufgebaut und kann auch unabhängig in eigener Software verwendet werden.

---

## 🌍 Lokalisierung

- **English**
- **Česky**
- **Deutsch**
- **Polski**
- **Русский**
- **Українська**

---

## 📜 Lizenz

Dieses Projekt ist **(naja, technisch gesehen… eigentlich nicht 😉)** unter der UNLICENSED-Lizenz lizenziert — siehe die Datei `UNLICENSE` für Details.

