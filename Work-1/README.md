# ThaiPass Music App

A music player app in two versions, built with JavaScript and a simulated AI ThaiPass login.

## 1. JS version (open in a browser)
- `index.html` — double-click to open (Chrome / Edge)
- `app.js` — main code + simulated AI ThaiPass login
- `style.css` — dark purple theme

How to play: open `index.html` → press ▶ → 3 sample songs included, add your own MP3 files, search / shuffle / repeat / volume control.

## 2. Desktop version (.exe)
- `MusicPlayer.cs` — C# WinForms source code
- `MusicApp.exe` — double-click to run (Windows)

```sh
# Recompile (after editing .cs)
& "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /target:winexe /out:MusicApp.exe MusicPlayer.cs /reference:System.Windows.Forms.dll /reference:System.Drawing.dll
```

## Features
- Play / pause / next / previous
- Shuffle and repeat modes
- Seek bar and volume control
- Search songs in the playlist
- Upload your own `.mp3` / `.wav` files
- Auto-plays the next song when one finishes
