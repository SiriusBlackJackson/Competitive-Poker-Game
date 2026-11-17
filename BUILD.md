# Build & release

Everything is compiled with the C# compiler that ships with Windows —
`C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe`. There is no SDK, NuGet,
or third-party dependency, and the produced exe only needs the .NET Framework 4.x
that Windows 7+ already has.

## One command

```powershell
powershell -ExecutionPolicy Bypass -File build\package.ps1
```

This produces, in `dist\`:

| File | What it is |
| --- | --- |
| `ZhengShangYou.exe` | the game, single file, ~130 KB |
| `ZhengShangYou-Portable.zip` | portable edition (`portable.txt` keeps settings beside the exe) |
| `ZhengShangYou-Setup.exe` | installer + uninstaller with the game embedded as a resource |

`build\build.ps1` alone compiles just the game exe.

## Source layout

| File | Contents |
| --- | --- |
| `Core.cs` | deck, card values, play-type validation and comparison |
| `Lang.cs` | the 8-language string table (108 keys × 8 languages) |
| `Cards2D.cs` | GDI+ card faces (real card look) and the card sheet renderer |
| `UI.cs` | GDI+ widgets, settings persistence, language dialog, rules window |
| `Game.cs` | `Program.Main`, `GameForm`: game loop, AI, trades, drawing |

Source order for `csc`: `Core.cs`, `Lang.cs`, `Cards2D.cs`, `UI.cs`, `Game.cs`.

## Self-checks

```powershell
dist\ZhengShangYou.exe -selftest    # plays 5 rounds headless, writes selftest.txt
dist\ZhengShangYou.exe -snapshot    # writes start.png, game.png, dialog.png, rank.png, rules.png, lang.png
dist\ZhengShangYou.exe -preview     # writes a card sheet png
```

`-selftest` asserts that the first lead of round 1 contains the 4♥ and that the
card total is 54 after every re-deal; it prints a `√` or `×` next to each check.

## Language handling

* `Lang.T(key)` looks a string up in the current language, falling back to
  Simplified Chinese when a translation is empty.
* `Lang.F(key, args)` wraps `string.Format` and falls back to the raw text on a
  malformed format string.
* Arabic switches the whole UI to right-to-left (`StringFormat` alignment flags)
  and swaps the font family to Segoe UI. Every other language uses Microsoft YaHei.
* Long translations are auto-fitted (`Ui.Fit`) instead of clipped.

## Installer behaviour

`ZhengShangYou-Setup.exe`:

* speaks the same 8 languages — a drop-down in the window re-renders every label, button and
  message, and Arabic flips the dialog to right-to-left; the chosen language is passed to the
  game as `-lang=xx` when it is started after installing
* installs to `%LOCALAPPDATA%\Programs\ZhengShangYou`
* copies itself there as `Uninstall.exe`
* creates a Desktop shortcut and a Start Menu shortcut
* registers an entry under `HKCU\Software\Microsoft\Windows\CurrentVersion\Uninstall\ZhengShangYou`
* removes everything (shortcuts, registry entry, folder) when run with `--uninstall`

Unattended switches used for testing: `--silent`, `--dir=<path>`, `--lang=<code>`, `--uninstall`.
