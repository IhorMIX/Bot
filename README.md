# Passport & Vehicle Document OCR Telegram Bot

A Telegram bot that extracts full name and VIN from photos of passports and vehicle registration certificates using Mindee OCR and C#.

---

## Overview

The bot receives photos of passports and vehicle documents, performs OCR (English and Ukrainian languages), parses the full name and VIN using regular expressions, and helps generate an insurance policy.

---

## Architecture & Modules

- **DocumentProcessor** — processes images and performs OCR using Mindee.
- **DocumentParser** — parses full name and VIN from extracted text using regex.
- **UpdateHandler** — handles Telegram messages and manages user states.
- **UserStateService** — stores user states and data.
- **PolicyGenerator** — generates the insurance policy text.
- **TelegramBotService** — manages the bot lifecycle and connection.

---

## OCR Providers
- International Passports
  - Endpoint: `https://api.mindee.net/v1/products/mindee/passport/v1/predict`
- Vehicle Identification Document
  - Custom endpoint: `https://api.mindee.net/v1/products/yukinon/vehicle_identification_document/v1/predict_async`

---

## Dependencies

- [.NET 8.0+](https://dotnet.microsoft.com/en-us/download)
- NuGet packages:
  - `Telegram.Bot`
  - `Mindee`

---

## Installation and Setup

1. Clone the repository or create a new project.

2. Install required NuGet packages:

   ```bash
   dotnet add package Telegram.Bot
   dotnet add package Mindee

3. Insert your tokens in appsettings.json:
    ```csharp
   "TelegramBotToken": "YOUR_TELEGRAM_BOT_TOKEN",
   "MindeeApiKey": "YOUR_MINDEE_API_KEY"

5. Run the application.

