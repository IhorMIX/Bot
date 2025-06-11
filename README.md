# Passport & Vehicle Document OCR Telegram Bot

A Telegram bot that extracts full name and VIN from photos of passports and vehicle registration certificates using Tesseract OCR and C#.

---

## Overview

The bot receives photos of passports and vehicle documents, performs OCR (English and Ukrainian languages), parses the full name and VIN using regular expressions, and helps generate an insurance policy.

---

## Architecture & Modules

- **DocumentProcessor** — processes images and performs OCR using Tesseract.
- **DocumentParser** — parses full name and VIN from extracted text using regex.
- **UpdateHandler** — handles Telegram messages and manages user states.
- **UserStateService** — stores user states and data.
- **PolicyGenerator** — generates the insurance policy text.
- **TelegramBotService** — manages the bot lifecycle and connection.

---

## Dependencies

- [.NET 8.0+](https://dotnet.microsoft.com/en-us/download)
- NuGet packages:
  - `Telegram.Bot`
  - `Tesseract`
- Tesseract language data files (download and place in the `tessdata` folder):
  - [eng.traineddata](https://github.com/tesseract-ocr/tessdata/raw/main/eng.traineddata)
  - [ukr.traineddata](https://github.com/tesseract-ocr/tessdata/raw/main/ukr.traineddata)

---

## Installation and Setup

1. Clone the repository or create a new project.

2. Install required NuGet packages:

   ```bash
   dotnet add package Telegram.Bot
   dotnet add package Tesseract

3. Download the Tesseract language files and place them in the directory:

  C:\Program Files\Tesseract-OCR\tessdata

4. Insert your Telegram bot token in the TelegramBotService constructor:

   var botService = new TelegramBotService("YOUR_TELEGRAM_BOT_TOKEN");
   botService.Start();

5. Run the application.

