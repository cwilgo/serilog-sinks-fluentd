# Generate Nuget package artifacts for publishing

$projPath = "./src/Serilog.Sinks.Fluentd/Serilog.Sinks.Fluentd.csproj"
$solutionPropFile = $projPath

# Generate version with build number if pre-release
$verFull = Select-String -Path "$solutionPropFile" -Pattern "<Version>(.*?)</Version>" | % {$_.Matches.Groups[1].Value}
$verParts = $verFull.Split('-')
$verPrefix = $verParts[0]
if ($verParts.Length -eq 2) {
    $verSuffixParts = $verParts[1].Split('.')
    $verTag = $verSuffixParts[0]
}
else {
    $verTag = ""
}
if ( -not [string]::IsNullOrEmpty($verTag) ) {
    # build number is encoded current date-time
    $build = (Get-Date -Format yyyyMMddHHmmss)
    $verFull = "$verPrefix-$verTag.$build"
    $proj = Get-Content "$solutionPropFile"
    $proj = $proj -replace "<Version>(.*?)</Version>", "<Version>$verFull</Version>"
    Set-Content "$solutionPropFile" $proj
}

# Clear publish folder
Remove-Item -Recurse ./publish/*

# Build packages
Write-Output "Creating package(s) for version $verFull"
dotnet pack -c Release -o publish/ --include-symbols --include-source "$projPath"
