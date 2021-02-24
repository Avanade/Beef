$nugetServer = "https://api.nuget.org/v3/index.json"
$publishFolder = "nuget-publish"
$apiKey = $args[0]

if ($apiKey.Length -eq 0) {
	Write-Host "WARNING!" -ForegroundColor Yellow -BackgroundColor Red
	Write-Host "All existing NuGet packages in publish folder will be deleted."
	Write-Host ""
	Write-Host "Enter API Key to continue (empty key indicates no publish requirement):"

	$apiKey = Read-Host
	Write-Host ""
}

# Create publish folder if it doesn't exist; otherwise, delete existing.
if (!(Test-Path -Path $publishFolder)) {
	Write-Host "Publish folder doesn't exist - creating folder." -ForegroundColor Yellow 
	New-Item -ItemType directory -Path $publishFolder
}
else {
    Write-Host "Deleting existing NuGet packages from publish folder." -ForegroundColor Yellow 
    $removeFiles = -join($publishFolder, "\", "*.nupkg")
    Remove-Item $removeFiles
}

# Set the projects to publish.
$projectsToPublish = @(
    "src\Beef.Core",
    "src\Beef.AspNetCore.WebApi",
    "src\Beef.Data.Database",
    "src\Beef.Data.Database.Cdc",
	"src\Beef.Data.EntityFrameworkCore",
	"src\Beef.Data.OData",
	"src\Beef.Data.Cosmos",
	"src\Beef.Events",
	"src\Beef.Events.EventHubs",
	"src\Beef.Grpc",
	"tools\Beef.CodeGen.Core",
    "tools\Beef.Database.Core",
    "tools\Beef.Test.NUnit",
	"templates\Beef.Template.Solution")

# Generate NuGet packages using dotnet pack.
foreach ($project in $projectsToPublish) {
    $packCommand = -join("dotnet pack ", $project, " -o ", $publishFolder) #, " -c Release --no-build")

    Write-Host ""
    Write-Host $packCommand -ForegroundColor Yellow 

    Invoke-Expression $packCommand
}

# Publish packages to NuGet repository (where a key has been provided and is longer than 20 characters).
if ($apiKey.Length -gt 20) {
	Get-ChildItem -Path $publishFolder -Filter *.nupkg | ForEach-Object {
		$package = -join($publishFolder, "\", $_.Name)
		$pushCommand = -join("dotnet nuget push --source ", $nugetServer, " --api-key ", $apiKey, " ", $package, " --skip-duplicate")

		Write-Host ""
		Write-Host $pushCommand -ForegroundColor Yellow

		Invoke-Expression $pushCommand
	}
}