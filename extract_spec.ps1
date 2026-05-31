$xmlPath = Join-Path $PSScriptRoot 'spec_unzip\word\document.xml'
if (-not (Test-Path $xmlPath)) {
    Copy-Item (Join-Path $PSScriptRoot 'HR_System_Spec_v7_1.docx') (Join-Path $PSScriptRoot 'spec_temp.zip') -Force
    Expand-Archive -Path (Join-Path $PSScriptRoot 'spec_temp.zip') -DestinationPath (Join-Path $PSScriptRoot 'spec_unzip') -Force
}
[xml]$doc = Get-Content $xmlPath
$ns = New-Object System.Xml.XmlNamespaceManager($doc.NameTable)
$ns.AddNamespace('w', 'http://schemas.openxmlformats.org/wordprocessingml/2006/main')
$paras = $doc.SelectNodes('//w:p', $ns)
$lines = foreach ($p in $paras) {
    $ts = $p.SelectNodes('.//w:t', $ns)
    if ($ts.Count -gt 0) { ($ts | ForEach-Object { $_.'#text' }) -join '' }
}
$out = Join-Path $PSScriptRoot 'spec_text.txt'
$lines | Set-Content $out -Encoding utf8
Write-Output "Wrote $($lines.Count) paragraphs to $out"
