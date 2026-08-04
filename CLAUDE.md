# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

OnlyV is a Windows WPF desktop tool that generates Bible verse text as images (or projects them directly to a second monitor), typically for use at Kingdom Halls / meetings. It extracts verse text from a JW.org EPUB bible file, renders it onto a themed background image, and displays the result full-screen on a secondary display or saves it as a PNG.

`OnlyVThemeCreator` is a companion WPF app for authoring the `.onlyv` theme files that OnlyV renders with.

## Solution structure

Visual Studio solution (`OnlyV.sln`), .NET Framework 4.8, C# (`LangVersion` 7.1/7.3), `packages.config`-style NuGet (not PackageReference). There is no `dotnet` CLI-based build — this requires MSBuild/Visual Studio tooling.

Projects:
- **OnlyV** — the main WPF application (entry point, `WinExe`).
- **OnlyV.VerseExtraction** — parses the JW.org NWT EPUB file and extracts book/chapter/verse text (`BibleEpubParser`, `BibleTextReader`). Has no WPF dependency.
- **OnlyV.ImageCreation** — renders extracted verse text onto a themed background to produce the final bitmap (`BibleTextImage`), including text splitting/fitting logic (`TextSplitting/TextSplitter.cs`).
- **OnlyV.Themes.Common** — shared theme model (`OnlyVTheme`, `Specs/*`) and `.onlyv` theme file read/write (`FileHandling/ThemeFile.cs`), plus WPF converters and window-positioning services shared between OnlyV and OnlyVThemeCreator.
- **OnlyVThemeCreator** — separate WPF app for creating/editing `.onlyv` theme files.
- **Tests** — MSTest project (`Tests.csproj`), see below.

## Build

Open `OnlyV.sln` in Visual Studio (2019+) and build, or from a Developer PowerShell:

```powershell
msbuild OnlyV.sln /p:Configuration=Release /p:Platform="Any CPU"
```

NuGet packages must be restored first (`packages.config` style):

```powershell
nuget restore OnlyV.sln
```

The installer is built separately via Inno Setup from `Installer/OnlyVSetup.iss`.

## Tests

MSTest tests live in `Tests/IntegrationTests.cs`. Almost all of them are gated on the presence of a real JW.org NWT EPUB file (`Tests\..\..\nwt_E.epub`, i.e. repo root) — each test returns immediately (no-op, not a failure) if that file isn't present, since the EPUB is copyrighted content not checked into the repo. To exercise these tests meaningfully you need a local copy of the EPUB at the solution root as `nwt_E.epub`.

Run via Visual Studio Test Explorer, or `vstest.console.exe Tests\bin\Debug\Tests.dll`.

## Architecture

**MVVM with MVVM Light (GalaSoft.MvvmLight) + CommonServiceLocator**, not modern `Microsoft.Extensions.DependencyInjection`. Composition root is `ViewModel/ViewModelLocator.cs`, which registers every service interface and view model with `SimpleIoc.Default`. It's instantiated as a static resource in `App.xaml` and exposed to XAML as `Locator`; views bind `DataContext="{Binding Source={StaticResource Locator}, Path=Main}"` etc. `OnlyVThemeCreator` follows the identical pattern with its own `ViewModelLocator`.

Cross-component communication uses MVVM Light's `Messenger` (pub/sub) rather than events where components shouldn't be directly coupled — see `PubSubMessages/*` (e.g. `ShutDownMessage`, `DragDropMessage`) and `Messenger.Default.Register<T>` calls in services like `DisplayWindowService`.

**Page navigation** is a single-window wizard, not multiple app windows: `MainViewModel.CurrentPage` swaps between page view models (`StartupViewModel` → `ScripturesViewModel` → `PreviewViewModel` / `SettingsViewModel` / `EditTextViewModel`), each backing an XAML page under `OnlyV/Pages/`. `MainWindow.xaml` hosts whichever page is current.

**Verse image pipeline** (the app's core function):
1. `IBibleVersesService` / `OnlyV.VerseExtraction.BibleEpubParser` parses the configured EPUB (`.epub` is a zip; see `Parser/EpubAsArchive.cs`) to pull book/chapter/verse text, using `Cache/*` to avoid re-parsing.
2. `OnlyV.Themes.Common.FileHandling.ThemeFile` loads the selected `.onlyv` theme (JSON-based, background image + font/color/shadow specs for title, body, verse numbers).
3. `OnlyV.ImageCreation.BibleTextImage.Generate(...)` composites verse text over the theme's background image, using `TextSplitting/TextSplitter` to fit text to the image dimensions.
4. The resulting bitmap is either shown full-screen via `IDisplayWindowService` (which places `Windows/DisplayWindow.xaml` on a chosen monitor using `IMonitorsService` + DPI-aware `WindowPlacement` from `OnlyV.Themes.Common`) or saved to disk via `IImageSavingService`.

**Options/settings** are persisted through `IOptionsService` (`OnlyV`) / `IOptionsService` (`OnlyVThemeCreator`, separate implementation) — JSON-serialized app options (`AppOptions/Options.cs`), not `Settings.settings`-based user settings (those exist too, but are minimal/legacy).

**Second-monitor projection** is a first-class concern: `IMonitorsService` enumerates displays, and `DisplayWindowService` explicitly manages a borderless `DisplayWindow` positioned on a non-primary monitor — this is the "project onto the data projector" use case the app is built around. There's also `Helpers/JwLib/JwLibHelper.cs`, which interacts with the separate JW Library app window (bring-to-front behavior) since OnlyV is commonly used alongside it.

## Localization

String resources live in `Properties/Resources*.resx` per project (main app, ImageCreation, Themes.Common, VerseExtraction, ThemeCreator each have their own). Non-English `.resx` files are machine-managed via Crowdin (`crowdin.yml` maps each project's `Resources.resx` to per-locale translations) — per `CONTRIBUTING.md`, do not hand-edit or include non-native `.resx` files in a PR; only change the neutral `Resources.resx`. There's a build quirk: `OnlyV.csproj`'s `PreBuildEvent` copies `Resources.no-NO.resx` to `Resources.no.resx` (Norwegian locale fallback).

## Coding conventions

StyleCop.Analyzers is referenced by the main projects and enforced via `OnlyV.ruleset` (mostly Microsoft CA rules at `Warning`, not `Error` — build won't fail on violations, but fix them anyway). Follow `.editorconfig` for formatting. Per `CONTRIBUTING.md`: keep PRs/commits atomic (one feature per commit), and coordinate before starting large changes.
