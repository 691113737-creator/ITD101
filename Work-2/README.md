# ThaiPass Video App (Work-2)

แอปเล่นวิดีโอ 2 เวอร์ชัน ทำเหมือน Work-1 (Music) แต่เปลี่ยนเป็นวิดีโอ + มี AI ThaiPass จำลอง

## 1. JS version / WebApp (เปิดในเบราว์เซอร์)
- `index.html` — ดับเบิลคลิกเปิด (Chrome / Edge)
- `app.js` — โค้ดหลัก + จำลอง AI ThaiPass login + ปุ่ม AI แนะนำ
- `style.css` — ธีมน้ำเงิน-ดำ

วิธีเล่น: เปิด `index.html` → กด ▶ → มีวิดีโอตัวอย่าง 3 ไฟล์, เพิ่มไฟล์วิดีโอของตัวเองได้, ค้นหา / สุ่ม / วนซ้ำ / ปรับเสียง / เต็มจอ

## 2. Desktop version (.exe)
- `VideoPlayer.cs` — ซอร์สโค้ด C# WinForms (ใช้ MCI winmm.dll เหมือน Work-1)
- `VideoApp.exe` — ดับเบิลคลิกเพื่อรัน (Windows)

```sh
# คอมไพล์ใหม่ (หลังแก้ .cs)
& "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /target:winexe /out:VideoApp.exe VideoPlayer.cs /reference:System.Windows.Forms.dll /reference:System.Drawing.dll
```

## Features
- เล่น / หยุดชั่วคราว / หยุด / ถัดไป / ก่อนหน้า
- แถบกรอวิดีโอ + แสดงเวลา + ปรับเสียง
- ค้นหาวิดีโอในเพลย์ลิสต์
- อัปโหลดไฟล์ `.mp4 / .avi / .wmv / .mpg / .mov` ของตัวเอง
- เล่นไฟล์ถัดไปอัตโนมัติเมื่อจบ
- ปุ่ม ⛶ เต็มจอ (.exe) / Fullscreen (JS)
- 🤖 AI ThaiPass: แสดงชื่อผู้ใช้ ThaiPass (Demo) + ปุ่ม "✨ AI แนะนำ" สุ่มเลือกวิดีโอให้
