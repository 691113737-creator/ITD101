# ThaiPass Music App

แอปเล่นเพลง 2 แบบในที่เดียว

## 1. แบบ JS (เปิดในเบราว์เซอร์)
- `index.html` — ดับเบิลคลิกเพื่อเปิด (Chrome / Edge)
- `app.js` — โค้ดหลัก + จำลอง login AI ThaiPass
- `style.css` — ธีมเข้มม่วง

วิธีเล่น: เปิด `index.html` → กด ▶ → มี 3 เพลงตัวอย่าง, เพิ่ม MP3 ตัวเองได้, ค้นหา / สุ่ม / วนซ้ำ / ปรับเสียง

## 2. แบบ Desktop (.exe)
- `MusicPlayer.cs` — ซอร์ส C# WinForms
- `MusicApp.exe` — ดับเบิลคลิกเล่นได้เลย (Windows)

```sh
# คอมไพล์ใหม่ (ถ้าแก้ .cs)
& "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /target:winexe /out:MusicApp.exe MusicPlayer.cs /reference:System.Windows.Forms.dll /reference:System.Drawing.dll
```
