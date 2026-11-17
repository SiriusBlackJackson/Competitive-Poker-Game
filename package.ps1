# Builds every Windows deliverable into dist\:
#   ZhengShangYou.exe            single-file game (no runtime needed beyond .NET Framework 4.x)
#   ZhengShangYou-Portable.zip   portable edition (settings stay next to the exe)
#   ZhengShangYou-Setup.exe      installer / uninstaller with the game embedded
#
#   powershell -ExecutionPolicy Bypass -File build\package.ps1
#
param(
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$dist = Join-Path $root "dist"
$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if (-not (Test-Path -LiteralPath $csc)) { $csc = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe" }

if (-not (Test-Path -LiteralPath $dist)) { New-Item -ItemType Directory -Path $dist | Out-Null }

if (-not $SkipBuild) { & (Join-Path $PSScriptRoot "build.ps1") -OutDir "dist" }

$exe = Join-Path $dist "ZhengShangYou.exe"
if (-not (Test-Path -LiteralPath $exe)) { throw "ZhengShangYou.exe not found - run build.ps1 first." }

# ---------- portable edition ----------
$stage = Join-Path $root "build\out\portable"
if (Test-Path -LiteralPath $stage) { Remove-Item -LiteralPath $stage -Recurse -Force }
New-Item -ItemType Directory -Path $stage -Force | Out-Null

Copy-Item -LiteralPath $exe -Destination $stage -Force
Copy-Item -LiteralPath (Join-Path $root "LICENSE") -Destination (Join-Path $stage "LICENSE.txt") -Force

# The marker file switches settings from %APPDATA% to settings.ini next to the exe.
[IO.File]::WriteAllText((Join-Path $stage "portable.txt"),
    "Portable mode: settings.ini is stored next to ZhengShangYou.exe instead of %APPDATA%`r`n", (New-Object Text.UTF8Encoding($false)))

$readme = @"
Zheng Shang You - Portable Edition  (8 languages)
================================================

EN  Double-click ZhengShangYou.exe. Nothing to install, no registry entries.
    Language: the language button on the title screen or in the menu, or -lang=en.
    Codes: zh-CN zh-TW en fr ru ja es ar. Settings live in settings.ini in this folder.

简中  双击 ZhengShangYou.exe 即可运行，无需安装、不写注册表。
      切换语言：开始页或菜单里的「语言」按钮，也可以 -lang=zh-CN。

繁中  雙擊 ZhengShangYou.exe 即可執行，無需安裝、不寫登錄檔。
      切換語言：開始頁或選單中的「語言」按鈕，或 -lang=zh-TW。

FR  Double-cliquez sur ZhengShangYou.exe. Aucune installation, aucune entrée de registre.
    Langue : bouton du menu, ou -lang=fr.

RU  Запустите ZhengShangYou.exe. Установка не требуется, реестр не изменяется.
    Язык: кнопка в меню или -lang=ru.

JA  ZhengShangYou.exe をダブルクリック。インストール不要、レジストリも変更しません。
    言語: メニューの言語ボタン、または -lang=ja。

ES  Haz doble clic en ZhengShangYou.exe. Sin instalación ni entradas de registro.
    Idioma: botón del menú o -lang=es.

AR  شغّل ZhengShangYou.exe بالنقر المزدوج. لا حاجة لتثبيت ولا تعديل في السجل.
    اللغة: زر القائمة أو ‎-lang=ar‎.

Requires Windows 7 or newer with .NET Framework 4.x (included in Windows 10/11).

Diagnostics:
      ZhengShangYou.exe -selftest     plays 5 rounds headless and writes selftest.txt
      ZhengShangYou.exe -snapshot     renders start/game/dialog/rules screenshots
      ZhengShangYou.exe -preview      renders a card sheet
"@
[IO.File]::WriteAllText((Join-Path $stage "README.txt"), $readme, (New-Object Text.UTF8Encoding($false)))

$zip = Join-Path $dist "ZhengShangYou-Portable.zip"
if (Test-Path -LiteralPath $zip) { Remove-Item -LiteralPath $zip -Force }
Compress-Archive -Path (Join-Path $stage "*") -DestinationPath $zip -CompressionLevel Optimal
Write-Host ("Portable: " + $zip)

# ---------- setup / uninstaller ----------
$setupExe = Join-Path $dist "ZhengShangYou-Setup.exe"
$installerSrc = Join-Path $root "build\installer\Installer.cs"
$icon = Join-Path $root "assets\icon\game.ico"

& $csc /nologo /target:winexe /codepage:65001 /optimize+ `
    /win32icon:"$icon" `
    "/resource:$exe,PAYLOAD" `
    /r:System.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll `
    /out:"$setupExe" `
    $installerSrc

if ($LASTEXITCODE -ne 0) { throw "Installer compilation failed." }
Write-Host ("Setup:    " + $setupExe)

Get-ChildItem -LiteralPath $dist | Select-Object Name, Length | Format-Table -AutoSize
