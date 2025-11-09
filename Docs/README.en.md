# 🐾 K9 Trails Analyzer

**K9 Trails Analyzer** is a tool designed for dog handlers and trainers involved in **mantrailing** or **practical tracking**.  
It was created to **objectively evaluate training sessions** and **better understand the dog’s behaviour on the trail**.  
The application allows you to **load and analyse GPX recordings** from training sessions, measuring distances, speed, trail age and the dog’s tracking accuracy.  

In addition to analysis, it also includes a **unique overlay video generation library**, which displays **the dog’s trail, the trail layer’s route, wind direction and strength**, and other data in real time.  
These overlays can easily be combined with footage from an action camera to create a **comprehensive trail video** – perfect for education, debriefing or sharing insights.

From version **1.0.26**, the application also includes a **scoring system**, suitable for **mantrailing** and **practical tracking competitions**.

---

## 💡 Main Features

- **GPX File Loading**  
  Automatically recognises and merges dog and trail layer routes even when stored in separate files.  
  Supported sources include **Geo Tracker, OpenTracks, Mapy.com, The Mantrailing App, [Locus Map](https://www.locusmap.app)**, and others.

- **Trail Analysis**
  - Calculates **total distance**, **trail age**, and **average speed**  
  - Allows **filtering by date** or training period  
  - Computes **the dog’s deviation** from the layer’s route  

- **Visualisation**
  - Graphs showing trail lengths, speeds, and ages over time  
  - Option to export graphs and data  

- **Trail Notes**  
  A dedicated form for adding notes to each trail.  
  Notes are automatically saved as PNG overlays and can be displayed in the resulting video.

- **🎬 Overlay Video Creation**  
  Generates videos showing the dog’s and layer’s routes, as well as wind direction and strength – on a transparent background.  
  The resulting video can be combined in editors such as [Shotcut](https://shotcut.org/) with footage from an action camera.

- **🏅 Scoring System for Competitions**
  - Finding the trail layer  
  - Working speed  
  - Tracking accuracy  
  - Reading the dog’s signals  

- **📊 Data Export**  
  Exports results to **CSV**, **TXT**, or **RTF** formats – suitable for printing or further analysis in Excel.

---

## 🧱 Installation

1. Download the latest version from [Releases](https://github.com/mwrnckx/K9-Trails-Analyzer/releases)  
2. Unzip it to any folder  
3. Run `K9TrailsAnalyzer.exe`  
4. A sample set of GPX files is included in the **Samples** folder and loads automatically on first launch  
   The target folder can be changed anytime from the **File** menu.

---

## ⚙️ Technical Information

- Requires **.NET 8.0 Runtime**  
- Uses **[FFmpeg](https://ffmpeg.org/)** for video generation  
  (included in the ZIP package – no installation required)

---

## 📂 Project Structure

This solution contains two projects:

- **K9-Trails-Analyzer** – the main Windows Forms application  
  Used for reading and analysing GPX data.

- **[TrackVideoExporter](TrackVideoExporter.md)** – a class library for generating overlay videos  
  Can also be used independently in other projects (for training or analytical purposes).

---

## 🌍 Localisation

The application is multilingual and supports:

- 🇬🇧 English  
- 🇨🇿 Czech  
- 🇩🇪 German  
- 🇵🇱 Polish  
- 🇷🇺 Russian  
- 🇺🇦 Ukrainian  

---

## 📜 Licence

The project is distributed as **UNLICENSED** – see the `UNLICENSE` file for details.  
In other words: you’re free to use it, but the author takes no responsibility for any damage it causes  
(even if, according to its analysis, your dog finds your neighbour’s barbecue – or worse, his sausages 🐕🌭).

---
