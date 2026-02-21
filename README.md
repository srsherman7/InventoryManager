# 📦 Inventory Manager

A clean, lightweight desktop inventory management app built with **C# WinForms** and **SQLite**. Designed for home sellers and small operations who need a simple way to track stock, prices, and inventory value without the overhead of a full business platform.

---

## Screenshots

<img width="1046" height="693" alt="invmgr1" src="https://github.com/user-attachments/assets/c4d62ef1-4cd6-4e92-8e83-e3105c7b525a" />
<img width="549" height="272" alt="invmgr2" src="https://github.com/user-attachments/assets/1b75ba17-7a38-4a49-83f7-27bc75d72806" />
<img width="1044" height="687" alt="invmgr3" src="https://github.com/user-attachments/assets/1cf2d984-f009-4712-8c7e-a4a5e435aad6" />
<img width="1046" height="693" alt="invmgr4" src="https://github.com/user-attachments/assets/ead32f58-50d3-409c-b5f1-6529c452d8f5" />


## Features

- **Live stats bar** — total item count and total inventory value always visible at the top
- **Quick quantity controls** — red `−` and green `+` buttons on every row to adjust stock instantly when a sale happens
- **Low stock color coding** — quantity turns red at 0, orange at 5 or below, green when healthy
- **Live search** — filters by product name or type as you type, partial matches supported
- **Full CRUD** — Add, Edit, and Delete items with a clean dialog
- **Double-click to edit** any row
- **Cell focus border** — blue outline on the active cell so you always know where you are
- **Backup database** — zips up your `.db` file and saves it wherever you choose (`Ctrl+B`)
- **Export to CSV** — exports your full inventory for use in Excel, Google Sheets, or anywhere else (`Ctrl+E`)
- **Persistent storage** — SQLite database stored in `%AppData%\InventoryManager\inventory.db`

---

## Tech Stack

| Layer | Technology |
|---|---|
| Language | C# 12 |
| Framework | .NET 8 WinForms |
| Database | SQLite via `Microsoft.Data.Sqlite` |
| Packaging | Single `.exe` publish support |

---

## Requirements

- Windows 10 or 11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) _(for building from source)_

---

## Getting Started

### Option A — Visual Studio 2022
1. Clone the repo
2. Open `InventoryManager.sln`
3. Press `F5` to build and run

### Option B — dotnet CLI
```bash
git clone https://github.com/yourusername/InventoryManager.git
cd InventoryManager/InventoryManager
dotnet restore
dotnet run
```

### Option C — Standalone EXE (no .NET install needed on target machine)
```bash
cd InventoryManager/InventoryManager
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish
```
Run `publish\InventoryManager.exe` on any Windows 10/11 machine.

---

## Usage

| Action | How |
|---|---|
| Add item | Click **+ Add Item** |
| Edit item | Select a row then click **Edit**, or double-click the row |
| Delete item | Select a row then click **Delete** |
| Sell 1 unit | Click the red **−** button on the row |
| Restock 1 unit | Click the green **+** button on the row |
| Search | Type in the search box — filters live as you type |
| Clear search | Click **X Clear** |
| Backup database | **File → Backup Database...** or `Ctrl+B` |
| Export to CSV | **File → Export to CSV...** or `Ctrl+E` |

---

## Project Structure

```
InventoryManager/
├── InventoryManager.sln
└── InventoryManager/
    ├── InventoryManager.csproj
    ├── Program.cs                  — Entry point
    ├── MainForm.cs                 — Main UI (stats bar, toolbar, grid, menus)
    ├── Models/
    │   └── InventoryItem.cs        — Data model
    ├── Data/
    │   └── DatabaseService.cs      — All SQLite CRUD operations
    └── Forms/
        └── ItemDialog.cs           — Add / Edit item dialog
```

---

## Database Location

Your inventory data is stored at:
```
C:\Users\<YourName>\AppData\Roaming\InventoryManager\inventory.db
```
You can open this file with [DB Browser for SQLite](https://sqlitebrowser.org/) to inspect or manually edit data. Use the built-in **Backup** feature to keep regular copies.

---

## Roadmap / Ideas

- [ ] Import from CSV
- [ ] Low stock threshold alerts
- [ ] Product categories / filtering by type
- [ ] Print inventory report
- [ ] Dark mode

---

## License

MIT — free to use, modify, and distribute.

---

## Contributing

Pull requests are welcome. For major changes please open an issue first to discuss what you would like to change.
