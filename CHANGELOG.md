# Changelog

All notable changes to this project are recorded here. Versions follow
[Semantic Versioning](https://semver.org/).

## [1.0.0] — 2026-11-17

First public release.

### Added
* Three-player, five-round game with the regional 15-step strength ladder
  (`5 > 8 > Big Joker > Small Joker > 3 > 2 > A > K > Q > J > 10 > 9 > 7 > 6 > 4`).
* Round 1 led by the holder of the 4 of Hearts, whose first play must contain it.
* Two-step trades from round 2: mandatory tribute (highest card counted before receiving),
  then an optional request that may be refused up to 3 times and may not target the
  giver's highest card or the card just tributed.
* All played cards are shown in the centre of the table together with who played them;
  the centre also announces the next leader when all three pass.
* 30-second turn clock with automatic pass (automatic smallest legal lead when leading).
* Procedural card faces drawn with GDI+ (rank, pips, joker art, "Z" card back).
* 8 languages — 简体中文, 繁體中文, English, Français, Русский, 日本語, Español, العربية —
  switchable from the title screen, the rules screen and mid-game; interface, rules text,
  game log, dialogs and scoreboard all follow the language.
* Right-to-left layout and font fallback for Arabic; auto-shrinking text for long translations.
* Language persistence: `%APPDATA%\ZhengShangYou\settings.ini`, or `settings.ini` beside the exe
  in portable mode (`portable.txt` present).
* Command-line switches: `-lang=xx`, `-selftest`, `-snapshot`, `-preview`.
* Release files: `ZhengShangYou.exe`, `ZhengShangYou-Portable.zip`,
  `ZhengShangYou-Setup.exe` (installer + uninstaller with the game embedded), `ZhengShangYou.apk`.
* The installer itself speaks the same 8 languages (drop-down, instant re-render, RTL layout for
  Arabic) and starts the game with the selected language.
* Android edition: same rules engine and string table, touch table in landscape, same 8
  languages — language pill in the table header, Language entry in the menu, Arabic dialogs
  mirrored, launcher name localized.

### Notes
* Requires Windows 7 or newer; no runtime to install (.NET Framework 4.x is bundled with Windows).
