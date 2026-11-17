# Builds ZhengShangYou.exe from src\*.cs using the .NET Framework C# compiler
# that ships with Windows (no SDK, no NuGet, no runtime dependency).
#
#   powershell -ExecutionPolicy Bypass -File build\build.ps1
#
param(
    [string]$OutDir = "dist"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$out = Join-Path $root $OutDir
if (-not (Test-Path -LiteralPath $out)) { New-Item -ItemType Directory -Path $out | Out-Null }

$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if (-not (Test-Path -LiteralPath $csc)) { $csc = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe" }
if (-not (Test-Path -LiteralPath $csc)) { throw "csc.exe (.NET Framework 4.x) not found. Install .NET Framework 4.x or build on a Windows machine." }

$sources = @("Core.cs", "Lang.cs", "Cards2D.cs", "UI.cs", "Game.cs") | ForEach-Object { Join-Path $root ("src\" + $_) }
foreach ($s in $sources) { if (-not (Test-Path -LiteralPath $s)) { throw "Missing source: $s" } }

$exe = Join-Path $out "ZhengShangYou.exe"
$icon = Join-Path $root "assets\icon\game.ico"

& $csc /nologo /target:winexe /codepage:65001 /optimize+ `
    /win32icon:"$icon" `
    /r:System.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll `
    /out:"$exe" `
    $sources[0] $sources[1] $sources[2] $sources[3] $sources[4]

if ($LASTEXITCODE -ne 0) { throw "Compilation failed with exit code $LASTEXITCODE" }

$item = Get-Item -LiteralPath $exe
Write-Host ("Built " + $item.FullName + "  (" + $item.Length + " bytes)")
