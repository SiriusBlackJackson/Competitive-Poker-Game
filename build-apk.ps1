# Builds dist\ZhengShangYou.apk without Gradle:
#   aapt2 (resources) -> javac (java 8 bytecode) -> d8 (dex) -> zip -> zipalign -> apksigner
#
# Requires a JDK and the Android SDK build-tools + platform; the script also knows how to
# bootstrap a private toolchain under build\tools (see docs/ANDROID.md).
#
#   powershell -ExecutionPolicy Bypass -File android\build\build-apk.ps1
#
param(
    [string]$Keystore = "",
    [switch]$SkipSelfTest
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$tools = Join-Path $root "build\tools"
$jdk = Join-Path $tools "jdk"
$sdk = Join-Path $tools "sdk"

# CI can supply its own toolchain through the usual environment variables.
if (-not (Test-Path -LiteralPath (Join-Path $jdk "bin\javac.exe")) -and $env:JAVA_HOME) { $jdk = $env:JAVA_HOME }
if (-not (Test-Path -LiteralPath (Join-Path $sdk "platforms")) -and $env:ANDROID_HOME) { $sdk = $env:ANDROID_HOME }
if (-not (Test-Path -LiteralPath (Join-Path $sdk "platforms")) -and $env:ANDROID_SDK_ROOT) { $sdk = $env:ANDROID_SDK_ROOT }
$proj = Join-Path $root "android\app\src\main"
$java = Join-Path $jdk "bin\java.exe"
$javac = Join-Path $jdk "bin\javac.exe"
$keytool = Join-Path $jdk "bin\keytool.exe"

$bt = Join-Path $sdk "build-tools\33.0.2"
$aapt2 = Join-Path $bt "aapt2.exe"
$d8 = Join-Path $bt "d8.bat"
$zipalign = Join-Path $bt "zipalign.exe"
$apksigner = Join-Path $bt "apksigner.bat"
$androidJar = Join-Path $sdk "platforms\android-33\android.jar"

foreach ($p in @($java, $javac, $aapt2, $d8, $zipalign, $apksigner, $androidJar)) {
    if (-not (Test-Path -LiteralPath $p)) { throw "Missing Android build component: $p  (see docs/ANDROID.md)" }
}

$env:JAVA_HOME = $jdk
$env:PATH = (Join-Path $jdk "bin") + ";" + $env:PATH

$work = Join-Path $root "build\out\apk"
if (Test-Path -LiteralPath $work) { Remove-Item -LiteralPath $work -Recurse -Force }
New-Item -ItemType Directory -Path $work | Out-Null
New-Item -ItemType Directory -Path (Join-Path $work "classes") | Out-Null
New-Item -ItemType Directory -Path (Join-Path $work "dex") | Out-Null

# ---- 0. shared translations ----
& (Join-Path $PSScriptRoot "generate-lang.ps1")

$engineSrc = Join-Path $proj "java\com\zhengshengyou\engine"
$appSrc = Join-Path $proj "java\com\zhengshengyou\app"
$sources = @()
$sources += (Get-ChildItem -LiteralPath $engineSrc -Filter *.java | ForEach-Object { $_.FullName })
$sources += (Get-ChildItem -LiteralPath $appSrc -Filter *.java | ForEach-Object { $_.FullName })

# ---- 1. engine self test on the desktop JVM ----
if (-not $SkipSelfTest) {
    $jout = Join-Path $work "jvm"
    New-Item -ItemType Directory -Path $jout | Out-Null
    $engs = Get-ChildItem -LiteralPath $engineSrc -Filter *.java | ForEach-Object { $_.FullName }
    & $javac -encoding UTF-8 -nowarn -d $jout $engs
    if ($LASTEXITCODE -ne 0) { throw "Engine compilation failed" }
    & $java -cp $jout com.zhengshengyou.engine.SelfTest
    if ($LASTEXITCODE -ne 0) { throw "Engine self test failed" }
}

# ---- 2. resources ----
& $aapt2 compile --dir (Join-Path $proj "res") -o (Join-Path $work "res.zip")
if ($LASTEXITCODE -ne 0) { throw "aapt2 compile failed" }

& $aapt2 link -o (Join-Path $work "base.apk") -I $androidJar `
    --manifest (Join-Path $proj "AndroidManifest.xml") `
    --min-sdk-version 21 --target-sdk-version 33 `
    --version-code 1 --version-name 1.0.0 `
    (Join-Path $work "res.zip")
if ($LASTEXITCODE -ne 0) { throw "aapt2 link failed" }

# ---- 3. java -> dex ----
& $javac -encoding UTF-8 -nowarn -source 8 -target 8 -bootclasspath $androidJar -d (Join-Path $work "classes") $sources
if ($LASTEXITCODE -ne 0) { throw "javac failed" }

$classFiles = Get-ChildItem -LiteralPath (Join-Path $work "classes") -Recurse -Filter *.class | ForEach-Object { $_.FullName }
# d8.bat from build-tools 33 passes -Djava.ext.dirs, which JDK 17 rejects; call the jar directly.
$d8Lib = Join-Path $bt "lib"
& $java -cp "$d8Lib\*" com.android.tools.r8.D8 --release --min-api 21 --lib $androidJar --output (Join-Path $work "dex") $classFiles
if ($LASTEXITCODE -ne 0) { throw "d8 failed" }

# ---- 4. put classes.dex into the apk ----
Add-Type -AssemblyName System.IO.Compression.FileSystem
$apk = Join-Path $work "unsigned.apk"
Copy-Item -LiteralPath (Join-Path $work "base.apk") -Destination $apk -Force
$zip = [System.IO.Compression.ZipFile]::Open($apk, [System.IO.Compression.ZipArchiveMode]::Update)
[System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
    $zip, (Join-Path $work "dex\classes.dex"), "classes.dex") | Out-Null
$zip.Dispose()

# ---- 5. align + sign ----
$aligned = Join-Path $work "aligned.apk"
& $zipalign -f 4 $apk $aligned
if ($LASTEXITCODE -ne 0) { throw "zipalign failed" }

if ($Keystore -eq "") { $Keystore = Join-Path $tools "android.keystore" }
if (-not (Test-Path -LiteralPath $Keystore)) {
    & $keytool -genkeypair -v -keystore $Keystore -alias zhengshangyou `
        -keyalg RSA -keysize 2048 -validity 10000 `
        -storepass android -keypass android `
        -dname "CN=Zheng Shang You, O=ZhengShangYou, C=CN"
    if ($LASTEXITCODE -ne 0) { throw "keytool failed" }
}

$dist = Join-Path $root "dist"
if (-not (Test-Path -LiteralPath $dist)) { New-Item -ItemType Directory -Path $dist | Out-Null }
$final = Join-Path $dist "ZhengShangYou.apk"

& $apksigner sign --ks $Keystore --ks-pass pass:android --key-pass pass:android `
    --min-sdk-version 21 --v4-signing-enabled false --out $final $aligned
if ($LASTEXITCODE -ne 0) { throw "apksigner sign failed" }

& $apksigner verify --min-sdk-version 21 $final
if ($LASTEXITCODE -ne 0) { throw "apksigner verify failed" }

$item = Get-Item -LiteralPath $final
Write-Host ("APK: " + $item.FullName + "  (" + $item.Length + " bytes)")
