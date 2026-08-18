# <img src="Assets/SimpleBrowser.ico" alt="Icon" width="32"/> SimpleBrowser

A lightweight, tab-based desktop web browser built with C# and .NET 10, powered by the Avalonia UI framework.

On Linux, firefox is slow to start, and Chrome is a memory hog that requires keyrings and password entering all the time. 
SimpleBrowser is designed to be fast, efficient, and user-friendly, providing essential browsing features without unnecessary bloat.

<img src="Assets/screenshot.png" alt="Main Window" />

## Features

- **Tabbed Browsing** – Open and manage multiple tabs simultaneously
- **Navigation Controls** – Back, Forward, Refresh, and Stop buttons with a loading progress indicator
- **Address Bar** – URL input with automatic `https://` protocol detection
- **Bookmarks** – Add, remove, and clear bookmarks with persistent JSON storage
- **Browsing History** – Automatically logs visited pages with timestamps, persisted between sessions
- **Sidebar Panels** – Toggle bookmarks and history sidebars for quick access
- **Multiple Windows** – Open additional browser windows
- **Downloads Shortcut** – Quick-open the system Downloads folder
- **Modern UI** – Fluent Design styling with dark theme support

## Tech Stack

| Technology | Version |
|---|---|
| .NET | 10.0 |
| Avalonia UI | 12.1.0 |
| CommunityToolkit.Mvvm | 8.4.2 |
| FluentIcons.Avalonia | latest |

## Architecture

The project follows the **MVVM** pattern with a dedicated service layer:

```
SimpleBrowser/
├── Models/             # BookmarkModel, HistoryItemModel
├── ViewModels/         # MainViewModel, TabViewModel, AddBookmarkViewModel
├── Views/              # MainWindow, AddBookmarkDialog (Avalonia AXAML)
└── Services/
    ├── Abstractions/   # IBookmarkService, IHistoryService interfaces
    ├── BookmarkService.cs
    ├── HistoryService.cs
    ├── JsonBookmarksRepository.cs
    └── JsonHistoryItemRepository.cs
```

Data is persisted via a repository pattern backed by local JSON files.

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Build & Run

```powershell
git clone https://github.com/atshaw1994/SimpleBrowser.git
cd SimpleBrowser
dotnet run
```

## License

See [LICENSE.txt](LICENSE.txt) for details.