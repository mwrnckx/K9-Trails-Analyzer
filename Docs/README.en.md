# K9 Trails Analyzer

This project was created as a tool for dog handlers involved in mantrailing or scent tracking.  
It allows you to load GPX files from training sessions and analyze them (distance, trail aging, etc.).  
A key part of the project is a library for generating overlay videos (showing dog and trail layer positions) that can be combined with video recordings of the dog’s work. The final video lets you see both the dog's behavior and its real-time position on the trail, including distance from the tracklayer and visualized wind direction and strength!  
This is a powerful tool for analyzing your dog’s performance.

**K9 Trails Analyzer** is a Windows application that processes GPX files containing GPS tracks of the traillayer and the dog, and provides statistics such as total distance, trail age, and average speed.  
Tested with apps like Geo Tracker, OpenTracks, Mapy.com, The Mantrailing App, Locus Map, and others.

---

## 💡 Features

- **Reads GPX files**: Load tracks from any selected folder.
- **Date filtering**: Focus on trails within a selected time window.
- **Distance calculation**: Compute total and per-track distance.
- ⏳**Trail aging**: When both the traillayer and dog track are available, the app calculates how old the trail was when the dog followed it.
- **Speed analysis**: Calculates the average speed of the dog for each trail.
- **Export options**: Export results to RTF, TXT, or CSV for printing or further analysis in Excel.
- **Charts and visualization**: View summaries like total distance, track lengths, trail aging, and dog speed over time.
- 🎬 **Video generation**: Create transparent overlay videos showing both the traillayer’s and dog’s movements in real-time. A wind arrow visualizes wind direction and strength. This overlay can be combined with action camera footage using an editor like [Shotcut](https://shotcut.org/).

---

## 🛠️ Installation

1. Download the ZIP file from the [Releases section](https://github.com/mwrnckx/K9-Trails-AnalyzerII/releases).
2. Extract it to any folder.
3. Run `K9TrailsAnalyzer.exe`.
4. In the application there are several gpx files in the Samples folder. These files are processed after startup (until you change the destination folder). So you can safely test the program's functionality on these files. The target folder can be changed in the 'File' menu.

---

## 🧱 Dependencies

- [.NET 8.0 Runtime](https://dotnet.microsoft.com/en-us/download)
- The application uses external software: [ffmpeg](https://ffmpeg.org/)  
  The latest `ffmpeg.exe` is already included in the ZIP file — no manual setup needed.

---

## 📂 Project Structure

This repository contains two sub-projects:

- **K9-Trails-Analyzer** – the main Windows Forms application (GPX analysis)
- **[TrackVideoExporter](TrackVideoExporter.md)** – a class library for generating overlay videos
  - This library is modular and can also be used in your own software independently.

---

## 🌍 Localization

- **English**
- **Česky**
- **Deutsch**
- **Polski**
- **Русский**
- **Українська**

---

## 📜 License

This project is **(well, technically... is not 😉)** licensed under the UNLICENSED license — see the `UNLICENSE` file for details.

