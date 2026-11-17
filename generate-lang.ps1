# Generates android\app\src\main\java\com\zhengshengyou\engine\Lang.java
# from the single desktop string table src\Lang.cs, so both platforms share one translation set.
$ErrorActionPreference = "Stop"

$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)   # android\build -> android -> repo root
$cs = Join-Path $root "src\Lang.cs"
$out = Join-Path $root "android\app\src\main\java\com\zhengshengyou\engine\Lang.java"

$text = [IO.File]::ReadAllText($cs, [Text.Encoding]::UTF8)
$start = $text.IndexOf("static readonly string[,] TAB")
if ($start -lt 0) { throw "TAB table not found in Lang.cs" }
$block = $text.Substring($start)

# Parse the C# table with a tiny scanner: a row is { "key", "v1", ..., "v8" }.
function ReadLiteral([string]$s, [int]$from) {
    # $from points at the opening quote; returns the raw literal including quotes
    $i = $from + 1
    while ($i -lt $s.Length) {
        $ch = $s[$i]
        if ($ch -eq [char]'\') { $i += 2; continue }
        if ($ch -eq [char]'"') { return $s.Substring($from, $i - $from + 1), ($i + 1) }
        $i++
    }
    throw "Unterminated string at $from"
}

$rows = @()
$i = 0
while ($i -lt $block.Length) {
    $open = $block.IndexOf('{', $i)
    if ($open -lt 0) { break }
    $j = $open + 1
    while ($j -lt $block.Length -and [char]::IsWhiteSpace($block[$j])) { $j++ }
    if ($j -ge $block.Length -or $block[$j] -ne '"') { $i = $open + 1; continue }

    $keyLit, $j = ReadLiteral $block $j
    $key = $keyLit.Trim('"')
    $vals = @()
    while ($true) {
        while ($j -lt $block.Length -and ([char]::IsWhiteSpace($block[$j]) -or $block[$j] -eq ',')) { $j++ }
        if ($j -ge $block.Length) { throw "Row '$key' not closed" }
        if ($block[$j] -eq '}') { $j++; break }
        $lit, $j = ReadLiteral $block $j
        $vals += $lit
    }
    if ($vals.Count -ne 8) { throw "Row '$key' has $($vals.Count) values, expected 8" }
    $rows += ,@($key, $vals)
    $i = $j
}

$sb = New-Object System.Text.StringBuilder
[void]$sb.AppendLine("package com.zhengshengyou.engine;")
[void]$sb.AppendLine()
[void]$sb.AppendLine("// GENERATED from src/Lang.cs by android/build/generate-lang.ps1 - do not edit by hand.")
[void]$sb.AppendLine("public final class Lang {")
[void]$sb.AppendLine("    public static final String[] CODES = { ""zh-CN"", ""zh-TW"", ""en"", ""fr"", ""ru"", ""ja"", ""es"", ""ar"" };")
[void]$sb.AppendLine("    public static final String[] NAMES = { ""简体中文"", ""繁體中文"", ""English"", ""Français"", ""Русский"", ""日本語"", ""Español"", ""العربية"" };")
[void]$sb.AppendLine("    public static int idx = 0;")
[void]$sb.AppendLine()
[void]$sb.AppendLine("    private static final String[][] TAB = new String[][] {")
foreach ($r in $rows) {
    [void]$sb.AppendLine("        { """ + $r[0] + """, " + ($r[1] -join ", ") + " },")
}
[void]$sb.AppendLine("    };")
[void]$sb.AppendLine()
[void]$sb.AppendLine("    private Lang() { }")
[void]$sb.AppendLine()
[void]$sb.AppendLine("    public static String code() { return CODES[idx]; }")
[void]$sb.AppendLine("    public static boolean isRtl() { return CODES[idx].equals(""ar""); }")
[void]$sb.AppendLine()
[void]$sb.AppendLine("    public static void setCode(String code) {")
[void]$sb.AppendLine("        for (int i = 0; i < CODES.length; i++) if (CODES[i].equals(code)) { idx = i; return; }")
[void]$sb.AppendLine("        idx = 0;")
[void]$sb.AppendLine("    }")
[void]$sb.AppendLine()
[void]$sb.AppendLine("    public static String t(String key) {")
[void]$sb.AppendLine("        for (String[] row : TAB) if (row[0].equals(key)) {")
[void]$sb.AppendLine("            String v = row[idx + 1];")
[void]$sb.AppendLine("            return (v == null || v.length() == 0) ? row[1] : v;")
[void]$sb.AppendLine("        }")
[void]$sb.AppendLine("        return key;")
[void]$sb.AppendLine("    }")
[void]$sb.AppendLine()
[void]$sb.AppendLine("    /** Fills {0}..{7} placeholders; never throws on a malformed string. */")
[void]$sb.AppendLine("    public static String f(String key, Object... args) {")
[void]$sb.AppendLine("        String s = t(key);")
[void]$sb.AppendLine("        StringBuilder sb = new StringBuilder();")
[void]$sb.AppendLine("        int i = 0;")
[void]$sb.AppendLine("        while (i < s.length()) {")
[void]$sb.AppendLine("            int open = s.indexOf('{', i);")
[void]$sb.AppendLine("            int close = open < 0 ? -1 : s.indexOf('}', open);")
[void]$sb.AppendLine("            if (open < 0 || close < 0) { sb.append(s.substring(i)); break; }")
[void]$sb.AppendLine("            sb.append(s, i, open);")
[void]$sb.AppendLine("            int n = -1;")
[void]$sb.AppendLine("            try { n = Integer.parseInt(s.substring(open + 1, close)); } catch (Exception e) { }")
[void]$sb.AppendLine("            if (n >= 0 && args != null && n < args.length) sb.append(args[n]);")
[void]$sb.AppendLine("            else sb.append(s, open, close + 1);")
[void]$sb.AppendLine("            i = close + 1;")
[void]$sb.AppendLine("        }")
[void]$sb.AppendLine("        return sb.toString();")
[void]$sb.AppendLine("    }")
[void]$sb.AppendLine("}")

[IO.File]::WriteAllText($out, $sb.ToString(), (New-Object Text.UTF8Encoding($false)))
Write-Host ("Generated " + $out + " with " + $rows.Count + " keys")
