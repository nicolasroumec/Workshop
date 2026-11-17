# Workshop

[Español](README.md) · **English**

Personal console toolbox for day-to-day development: pick a project folder and run analysis tools on it.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org) (only for the duplicate code tool, which runs `jscpd` through `npx`)

## Run

```
dotnet run --project src/Workshop
```

The first time, choose **Agregar proyecto** (add project) and paste the folder of the project to analyze. The last used project is remembered.

## Tools

| Tool | What it does |
|---|---|
| **CSS muerto** (dead CSS) | Lists classes defined in global CSS files (not `.razor.css`) that are never used in `.razor`, `.cs`, `.cshtml`, `.html`, `.js` or `.ts` files. Classes built by interpolation (`badge-{code}`) are flagged for manual review instead of reported as dead. |
| **Código duplicado** (duplicate code) | Runs [jscpd](https://github.com/kucherenko/jscpd) on C#, Razor and CSS files, shows the largest duplicated blocks and opens the full HTML report. |

The user interface is in Spanish; the code is in English.

## Test

```
dotnet test
```

Tests run on [Microsoft.Testing.Platform](https://aka.ms/mtp-overview) (xUnit v3), enabled in `global.json`.

## Add a tool

Create a class in `src/Workshop/Tools/` that implements `ITool`. It is registered automatically at startup and shows up in the menu; dependencies such as `IAnsiConsole` or `ProjectStore` can be requested through the constructor.

## Data

Saved projects and generated reports live in `%APPDATA%\Workshop`, outside the repository.
