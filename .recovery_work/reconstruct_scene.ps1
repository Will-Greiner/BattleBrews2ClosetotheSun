param(
    [Parameter(Mandatory = $true)][string]$IntactPath,
    [Parameter(Mandatory = $true)][string]$LaterPath,
    [Parameter(Mandatory = $true)][string]$OutputPath
)

$ErrorActionPreference = 'Stop'

function Read-UnityYaml([string]$Path) {
    $text = [IO.File]::ReadAllText((Resolve-Path -LiteralPath $Path))
    $matches = [regex]::Matches($text, '(?m)^--- !u!(?<class>-?\d+) &(?<id>-?\d+)(?: stripped)?\r?\n')
    if ($matches.Count -eq 0) { throw "No Unity YAML objects found in $Path" }

    $header = $text.Substring(0, $matches[0].Index)
    $order = [Collections.Generic.List[string]]::new()
    $blocks = @{}
    $classes = @{}
    for ($i = 0; $i -lt $matches.Count; $i++) {
        $start = $matches[$i].Index
        $end = if ($i + 1 -lt $matches.Count) { $matches[$i + 1].Index } else { $text.Length }
        $id = $matches[$i].Groups['id'].Value
        $order.Add($id)
        $blocks[$id] = $text.Substring($start, $end - $start)
        $classes[$id] = [int]$matches[$i].Groups['class'].Value
    }

    return @{ Header = $header; Order = $order; Blocks = $blocks; Classes = $classes }
}

$intact = Read-UnityYaml $IntactPath
$later = Read-UnityYaml $LaterPath
$structuralClasses = @(1, 4, 224, 1001)
$builder = [Text.StringBuilder]::new($intact.Header)
$updated = 0
$skipped = 0
$intactIds = [Collections.Generic.HashSet[string]]::new([string[]]$intact.Order)

foreach ($id in $intact.Order) {
    $block = $intact.Blocks[$id]
    if ($later.Blocks.ContainsKey($id) -and $structuralClasses -notcontains $intact.Classes[$id]) {
        $laterBlock = $later.Blocks[$id]
        $localReferences = [regex]::Matches($laterBlock, '\{fileID: (-?\d+)\}') | ForEach-Object { $_.Groups[1].Value } | Where-Object { $_ -ne '0' }
        $hasMissingReference = @($localReferences | Where-Object { -not $intactIds.Contains($_) }).Count -gt 0
        if (-not $hasMissingReference) {
            $block = $laterBlock
            $updated++
        } else {
            $skipped++
        }
    }
    [void]$builder.Append($block)
}

[IO.File]::WriteAllText($OutputPath, $builder.ToString(), [Text.UTF8Encoding]::new($false))
Write-Output "Preserved $($intact.Order.Count) intact objects; carried forward $updated existing component records; skipped $skipped unsafe records."
