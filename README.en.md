# Workshop

[Español](README.md) · **English**

Personal console toolbox for day-to-day development: pick a project folder and run tools on it.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org) (only for the duplicate code tool, which runs `jscpd` through `npx`)
- [Git](https://git-scm.com) (to show the project branch, create tags and compute the version at build time)

## Run

```
dotnet run --project src/Workshop
```

The first time, choose **Agregar proyecto** (add project) and paste the folder of the project to analyze. The last used project is remembered.

## Install

When running from the repository on Windows, choose **Instalar en esta PC** (install on this PC). Workshop is placed in `%LOCALAPPDATA%\Programs\Workshop` and opens by typing `workshop` in a new terminal. To update it, run `git pull`, start the app from the repository and choose the option again, with `workshop` closed.

## Tools

| Tool | What it does |
|---|---|
| **Auditoría CSS** (CSS audit) | Lists classes defined in global CSS files (not `.razor.css`) that are never used in `.razor`, `.cs`, `.cshtml`, `.html`, `.js` or `.ts` files. Classes built by interpolation (`badge-{code}`) are flagged for manual review instead of reported as dead. It also checks CSS variables across all `.css` files (including `.razor.css`) and inline styles: those used with `var(--x)` but never declared (split by whether they have a fallback value) and those declared but never used. Bootstrap variables (`--bs-*`) count as declared. |
| **Código duplicado** (duplicate code) | Runs [jscpd](https://github.com/kucherenko/jscpd) on C#, Razor and CSS files, shows the largest duplicated blocks and opens the full HTML report. |
| **Crear tag** (create tag) | Shows the commits since the latest `vX.Y.Z` tag, suggests the next version and publishes an annotated tag to `origin`. It only works with no pending changes and on the latest commit of the default branch. |
| **Conversor Clarion** (Clarion converter) | Converts dates (`82455` ↔ `29/09/2026`), times (`4599263` ↔ `12:46:32.62`) and date-times between Clarion and readable formats. The direction is detected from what you type. |

The user interface is in Spanish; the code is in English.

## Version

The version comes from the latest git tag through [MinVer](https://github.com/adamralph/minver), so no file is edited. Build after creating the tag and from a clone with full history.

## Test

```
dotnet test
```

Tests run on [Microsoft.Testing.Platform](https://aka.ms/mtp-overview) (xUnit v3), enabled in `global.json`.

## Add a tool

Create a class in `src/Workshop/Tools/` that implements `ITool` (`Name`, `Icon`, `Description` and `RunAsync`). It is registered automatically at startup and shows up in the menu; dependencies such as `IAnsiConsole` or `ProjectStore` can be requested through the constructor.

## Data

Saved projects and generated reports live in `%APPDATA%\Workshop`, outside the repository.
