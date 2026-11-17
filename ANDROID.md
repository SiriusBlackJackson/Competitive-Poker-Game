# Android build

The Android edition is the same game as the desktop build: identical rules, identical deck,
and the same 8 languages. It is written in plain Java with no Gradle, Android Gradle Plugin,
Kotlin or third-party libraries, so a full APK can be produced from a bare JDK plus the
Android SDK command-line tools.

```
android/
  app/src/main/
    AndroidManifest.xml        package com.zhengshengyou.app
    java/com/zhengshengyou/engine/   rules engine (pure Java, no android.* imports)
      Card.java  Play.java  PlayRules.java  Game.java  Lang.java  SelfTest.java
    java/com/zhengshengyou/app/      UI
      MainActivity.java  TableView.java
    res/values/strings.xml
    res/mipmap-*/ic_launcher.png    generated from game.ico
  build/
    generate-lang.ps1          regenerates Lang.java from Lang.cs (single source of truth)
    build-apk.ps1              full APK pipeline
```

## Requirements

* JDK 17 (Temurin is fine)
* Android SDK with `platforms;android-33` and `build-tools;33.0.2`
* Windows PowerShell 7 (`pwsh`) if you use the scripts as shipped

Either install the SDK system-wide and export `ANDROID_HOME` / `JAVA_HOME`, or let the
scripts use the private toolchain that sits under `build\tools\jdk` and `build\tools\sdk`
(that is what the repository's own build used; the folder is git-ignored).

## Build

```powershell
pwsh -File android\build\build-apk.ps1
```

The script performs, in order:

1. `generate-lang.ps1` — rewrites `Lang.java` from `Lang.cs`, so translations never drift
   between the desktop and Android builds.
2. `javac` the engine into a temp folder and run `SelfTest` on the desktop JVM
   (300 complete five-round matches; checks the 4♥ first-play rule, 54 cards after dealing and
   after trading, distinct places, and that every language carries every key).
3. `aapt2 compile` + `aapt2 link` against `android.jar` (minSdk 21, target 33).
4. `javac` the app sources against `android.jar` (Java 8 bytecode), then `d8` to `classes.dex`.
5. Add `classes.dex` to the linked APK, `zipalign -f 4`, then `apksigner sign`.
6. Write `dist\ZhengShangYou.apk`.

`-SkipSelfTest` skips step 2. A debug key store is generated on first run at
`build\tools\android.keystore` (store password `android`); pass `-Keystore <path>` to sign
with your own key for a release upload.

## Languages

`Lang.java` exposes the same 108 keys as the desktop table:

```java
Lang.setCode("ar");                 // "zh-CN" "zh-TW" "en" "fr" "ru" "ja" "es" "ar"
String s = Lang.t("btn_play");      // plain lookup
String e = Lang.f("head_round", 2); // {0}-style placeholder
boolean rtl = Lang.isRtl();         // true for Arabic: layout mirrors
```

The chosen language is stored in `SharedPreferences` and applied on the next launch. Changing
it from the in-game menu re-renders the table, the dialogs and the rules text immediately —
the match in progress is not interrupted.

Three ways in, all live: the language pill in the top corner of the table (top-left when
Arabic flips the layout), the Language button on the start dialog, and the Language entry in
the menu. Dialogs are given an explicit layout direction, so Arabic mirrors the whole dialog
rather than only its text. The launcher name is localized too
(`res/values-zh-rCN`, `values-zh-rTW`, `values-ja`, …).

## Verification status

The build is verified by:

* a clean `aapt2 link` / `d8` / `apksigner verify` run,
* the engine self test above,
* `aapt2 dump badging` showing the expected package, version, SDK levels and launcher icon.

There is no emulator or physical device in the build environment, so the touch layout itself
has **not** been walked through on real hardware. The code paths are the ones exercised by the
desktop build (same engine, same state machine); anything that looks off on your device is
most likely spacing, which is all computed from one `dp` unit in `TableView.java`.

## Install

Copy `ZhengShangYou.apk` to the phone and open it (allow installation from unknown sources),
or `adb install -r ZhengShangYou.apk`. The game runs landscape, full screen, keeps the screen
awake, and can be exited from the menu.

## Rules of the road

* Do not hand-edit `engine/Lang.java` — edit `Lang.cs` and re-run `generate-lang.ps1`.
* The engine must stay free of `android.*` imports; that is what lets `SelfTest` run on the JVM.
* Bump `android:versionCode` / `versionName` in `AndroidManifest.xml` together with
  `CHANGELOG.md`.
