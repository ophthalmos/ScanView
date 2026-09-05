# Prüft die Übersetzungs-Konsistenz des Lng-Systems (deutscher Text = resx-Schlüssel):
# LNG001: Ein übersetzbarer deutscher Text (Lng.T im Code, Text/ToolTipText/HeaderText-Zuweisungen,
#         Combo-Listen, Kürzel-Tupel) hat in einer Sprachdatei keinen Eintrag — vermutlich wurde
#         deutscher Text geändert, ohne die Schlüssel nachzuziehen.
# LNG002: Ein resx-Schlüssel kommt im Code nicht mehr vor — Altlast nach einer Umformulierung.
# Grenzen: String-Konstanten und mehrzeilige Fallbacks expliziter Schlüssel sieht der Scanner nicht
# (siehe Ignorierliste). Ausgabe im MSBuild-Warnungsformat; läuft als Build-Target nach jedem Build
# (s. ScanView.csproj) und jederzeit manuell:  powershell -ExecutionPolicy Bypass -File check-lng.ps1

$root = $PSScriptRoot
$languages = "en", "fr", "es"

# Texte, die absichtlich in keiner Sprachdatei stehen (Markennamen, sprachneutrale Angaben,
# Designer-Platzhalter, die zur Laufzeit sofort überschrieben werden)
$ignore = @(
    "ScanView", "OK", "PDF",
    "English", "Español", "Français", # Eigennamen der Sprachen — erscheinen bewusst in der jeweiligen Sprache
    "300 dpi · Graustufen · A4 · Flachbett",       # Designer-Platzhalter; zur Laufzeit dynamisch zusammengesetzt
    " Scanner und Kameras",                        # Designer-Platzhalter; zur Laufzeit " " + Lng.T("Scanner und Kameras")
    "&Title:", "Su&bject:", "&Keywords:", "Aut&hor:", # PDF-Konventionsnamen — bewusst in allen Sprachen original (Wilhelms Entscheidung)
    "C:\Program Files\ScanView\tessdata",          # Designer-Platzhalter; zur Laufzeit der echte Pfad
    "https://github.com/tesseract-ocr/tessdata_best" # Link-Beschriftung (URL, sprachneutral)
)

# ---------------------------------------------------------------- Schlüssel der Sprachdateien
$langKeys = @{}
foreach ($code in $languages) {
    $file = Join-Path $root "Languages\lng.$code.resx"
    [xml]$xml = Get-Content $file -Raw -Encoding UTF8
    $keys = New-Object 'System.Collections.Generic.HashSet[string]'
    foreach ($data in $xml.root.data) { [void]$keys.Add([string]$data.name) }
    $langKeys[$code] = $keys
}

# ---------------------------------------------------------------- verwendete Schlüssel sammeln
function ConvertFrom-CSharpLiteral([string]$s) {
    [regex]::Replace($s, '\\(.)', { param($m)
        switch ($m.Groups[1].Value) { 'n' { "`n" } 'r' { "`r" } 't' { "`t" } default { $m.Groups[1].Value } } })
}

$literal = '"((?:[^"\\]|\\.)*)"'
$verbatim = '@"((?:[^"]|"")*)"'
$tokens = '@"(?<v>(?:[^"]|"")*)"|"(?<s>(?:[^"\\]|\\.)*)"|' + "'" + '(?:\\.|[^' + "'" + '\\])' + "'" + '|//[^\r\n]*|/\*[\s\S]*?\*/'
$used = New-Object 'System.Collections.Generic.HashSet[string]'
$sources = Get-ChildItem $root -Recurse -Include *.cs -File | Where-Object { $_.FullName -notmatch '\\(obj|bin|\.claude)\\' }
foreach ($file in $sources) {
    $text = Get-Content $file.FullName -Raw -Encoding UTF8
    # 1) direkte Lng.T-Aufrufe (auch innerhalb interpolierter Strings; Verbatim-Variante mit @"…")
    foreach ($m in [regex]::Matches($text, "Lng\.T\(\s*$literal")) {
        [void]$used.Add((ConvertFrom-CSharpLiteral $m.Groups[1].Value))
    }
    foreach ($m in [regex]::Matches($text, "Lng\.T\(\s*$verbatim")) {
        [void]$used.Add($m.Groups[1].Value.Replace('""', '"'))
    }
    # 2) Ternary-Argumente: Lng.T(bedingung ? "A" : "B") — beide Zweige sind Schlüssel
    foreach ($m in [regex]::Matches($text, "Lng\.T\([^`"()]*\?\s*$literal\s*:\s*$literal\s*\)")) {
        [void]$used.Add((ConvertFrom-CSharpLiteral $m.Groups[1].Value))
        [void]$used.Add((ConvertFrom-CSharpLiteral $m.Groups[2].Value))
    }
    # 3) übersetzte Eigenschafts-Zuweisungen (Designer wie Code; nur reine Literale bis zum Semikolon)
    foreach ($m in [regex]::Matches($text, "\b(?:Text|ToolTipText|ShortcutKeyDisplayString|HeaderText)\s*=\s*$literal\s*;")) {
        [void]$used.Add((ConvertFrom-CSharpLiteral $m.Groups[1].Value))
    }
    # 4) Combo-Listen des Designers und Dictionary-Werte (["ita"] = "Italienisch") — beides
    #    übersetzt die Anwendung zur Laufzeit über Lng.T
    foreach ($m in [regex]::Matches($text, "Items\.AddRange\(new object\[\]\s*\{([^}]*)\}")) {
        foreach ($s in [regex]::Matches($m.Groups[1].Value, $literal)) { [void]$used.Add((ConvertFrom-CSharpLiteral $s.Groups[1].Value)) }
    }
    foreach ($m in [regex]::Matches($text, "\]\s*=\s*$literal\s*,?\s*(?:\r?\n|\})")) {
        [void]$used.Add((ConvertFrom-CSharpLiteral $m.Groups[1].Value))
    }
    # 5) Kürzel-Tupel ("Kürzel", "Kurztext") in TaskDlg.ShortcutRows und den Kürzellisten der Formulare
    foreach ($m in [regex]::Matches($text, "\(\s*$literal\s*,\s*$literal")) {
        $key = ConvertFrom-CSharpLiteral $m.Groups[1].Value
        if ($key -match '^(Strg|F\d|Alt\+|Bild|2×|Esc|Entf)') {
            [void]$used.Add($key)
            [void]$used.Add((ConvertFrom-CSharpLiteral $m.Groups[2].Value))
        }
    }
    # 6) die Detail-Spalte der ShortcutRows (drittes Tupel-Element)
    foreach ($m in [regex]::Matches($text, "ShortcutRows\s*=\s*\[(.*?)\];", 'Singleline')) {
        foreach ($s in [regex]::Matches($m.Groups[1].Value, $literal)) { [void]$used.Add((ConvertFrom-CSharpLiteral $s.Groups[1].Value)) }
    }
    # 7) Rettungsregel gegen Fehlalarme: Jedes Literal, das exakt einem vorhandenen resx-Schlüssel
    #    entspricht, gilt als verwendet — deckt Felder, switch-Ausdrücke, Dialog-Filter usw. ab,
    #    ohne LNG001 aufzuweichen (es zählen nur Texte, die bereits übersetzt sind). Tokenisiert
    #    Strings, Kommentare und Zeichenliterale gemeinsam, damit Anführungszeichen in Kommentaren
    #    die Paarung nicht verschieben.
    foreach ($m in [regex]::Matches($text, $tokens)) {
        $value = $null
        if ($m.Groups['v'].Success) { $value = $m.Groups['v'].Value.Replace('""', '"') }
        elseif ($m.Groups['s'].Success) { $value = ConvertFrom-CSharpLiteral $m.Groups['s'].Value }
        if ($value -and ($languages | Where-Object { $langKeys[$_].Contains($value) })) { [void]$used.Add($value) }
    }
}

# ---------------------------------------------------------------- LNG001: fehlende Übersetzungen
$findings = 0
foreach ($key in $used | Sort-Object) {
    if ($key -notmatch '\p{L}') { continue }     # ohne Buchstaben (z.B. "0/0") gibt es nichts zu übersetzen
    if ($key -match "`n") { continue }           # mehrzeilig geht nicht als resx-Schlüssel — läuft über explizite Schlüssel
    if ($key -match '\{\D') { continue }         # Fragment eines interpolierten Strings, kein Schlüssel ({0} bleibt erlaubt)
    if ($key -match '^(F\d+|Alt\+.+|A\d|A–Z|Z–A|US-Letter|\d+ dpi)$') { continue } # sprachneutrale Kürzel und Formate
    if ($ignore -contains $key) { continue }
    foreach ($code in ($languages | Where-Object { -not $langKeys[$_].Contains($key) })) {
        Write-Output "Languages\lng.$code.resx : warning LNG001: Übersetzung fehlt für Schlüssel: `"$key`""
        $findings++
    }
}

# ---------------------------------------------------------------- LNG002: verwaiste resx-Schlüssel
foreach ($code in $languages) {
    foreach ($key in $langKeys[$code] | Sort-Object) {
        if ($ignore -contains $key) { continue }
        if (-not $used.Contains($key)) {
            Write-Output "Languages\lng.$code.resx : warning LNG002: Verwaister Schlüssel (im Code nicht gefunden): `"$key`""
            $findings++
        }
    }
}

if ($findings -eq 0) { Write-Output "check-lng: Alle Übersetzungen konsistent ($($used.Count) Schlüssel geprüft)." }
exit 0
