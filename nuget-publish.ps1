<#
    .SYNOPSIS
    Generates nuget pacakges and publishes if server values are provided

    .PARAMETER PublishFolder
    Local folder to publish the built nuget pacakges to

    .PARAMETER IncludeSymbols
    Create nuget symbols package

    .PARAMETER IncludeSource
    Include source in unget package

    .PARAMETER ApiKey
    ApiKey for publishing to server
    
    .PARAMETER NugetServer
    Url to the nuget server

    .PARAMETER ProjectsToPublish
    List of projects to Build and publish.

    .PARAMETER DeleteFromlocalCache
    Should the newly built packages be deleted from tle Nuget Cache Folder.

    .PARAMETER NugetCacheFolder
    Path to Nuget Cache folder.

    .INPUTS
    None. You cannot pipe objects to Add-Extension.

    .Example
    C:\PS> .\nuget-publish.ps1 -configuration 'Debug' -IncludeSymbols -IncludeSource
    Build all and publish locally. 
    Used to test locally changes, Create

    .Example
    C:\PS> .\nuget-publish.ps1 -NugetServer 'https://api.nuget.org/v3/index.json' -ApiKey 'key' -ProjectsToPublish @('Beef.CodeGen.Core', 'Beef.Database.Core')
    Build selected pacakges and push to remote server.
#>
param(
    [string]$PublishFolder = "$($env:USERPROFILE)\nuget-publish",
    [string]$configuration = 'Release',
    [switch]$IncludeSymbols,
    [switch]$IncludeSource,
    [string]$NugetServer,
    [string]$ApiKey,
    [switch]$DeleteFromlocalCache,
    [string]$NugetCacheFolder = "$($env:USERPROFILE)\.nuget\packages",
    [String[]]$ProjectsToPublish = @(
        "src\Beef.Abstractions",
        "src\Beef.Core",
        "src\Beef.AspNetCore.WebApi",
        "src\Beef.Data.Database",
        "src\Beef.Data.Database.Cdc",
	    "src\Beef.Data.EntityFrameworkCore",
	    "src\Beef.Data.OData",
	    "src\Beef.Data.Cosmos",
	    "src\Beef.Events",
	    "src\Beef.Events.EventHubs",
	    "src\Beef.Events.ServiceBus",
	    "src\Beef.Grpc",
	    "tools\Beef.CodeGen.Core",
        "tools\Beef.Database.Core",
        "tools\Beef.Test.NUnit",
	    "templates\Beef.Template.Solution")
    )

$ShouldPublishRemote = (![string]::IsNullOrEmpty($apiKey) -and ![string]::IsNullOrEmpty($NugetServer))

if ($ShouldPublishRemote) {
	Write-Warning "APIKey and NugetServer provided, All existing NuGet packages in publish folder will be deleted.`r`n"
}

# Create publish folder if it doesn't exist; otherwise, delete existing.
if (!(Test-Path -Path $publishFolder)) {
	Write-Host "Publish folder doesn't exist - creating folder." -ForegroundColor Yellow 
	New-Item -ItemType directory -Path $publishFolder
}
else {
    if($ShouldPublishRemote)
    {
        Write-Host 'Deleting existing NuGet packages from publish folder.' -ForegroundColor Yellow 
        $removeFiles = Join-Path $publishFolder '*.nupkg'
        Remove-Item $removeFiles
    }
}

# Generate NuGet packages using dotnet pack.
$successPackageRegexp = [regex] 'Successfully\screated\spackage\s''(?<packagePath>.*?)''.'
$packageDetailsRegexp = [regex] '(?<name>.*?)\.(?<version>\d*\.\d*\.\d*\.\d*.*)(?<!\.symbols)$'

foreach ($project in $projectsToPublish) {
    $packCommand = "dotnet pack $($project) -o $($publishFolder) -c $($configuration)" # --no-build")
    if($IncludeSource) { $packCommand = "$($packCommand) --include-source"}
    if($IncludeSymbols) { $packCommand = "$($packCommand) --include-symbols"}

    Write-Host "`r`n$($packCommand)" -ForegroundColor Yellow 

    $output = ""
    Invoke-Expression $packCommand -OutVariable output

    if($DeleteFromlocalCache)
    {
        $successPackageRegexp.Matches($output) | foreach{
            $packagePath = $_.Groups['packagePath']

            $packageName = [System.IO.Path]::GetFileNameWithoutExtension($packagePath) 
            $packageDetails = $packageDetailsRegexp.Matches($packageName)
            if($packageDetails.Success)
            {
                $packageCachePath = [IO.Path]::Combine($NugetCacheFolder, $packageDetails[0].Groups['name'].value, $packageDetails[0].Groups['version'].value)
                if((Test-Path $packageCachePath) `
                    -and $packageCachePath -ne $nugetCacheFolder `
                    -and ![string]::IsNullOrWhiteSpace($packageDetails[0].Groups['name'].value) `
                    -and ![string]::IsNullOrWhiteSpace($packageDetails[0].Groups['version'].value))
                {
                    write-host "Deleting local cache for package`r`n`t$packageCachePath"
                    Remove-Item $packageCachePath -Recurse
                }
                else
                {
                    write-host "Local cache for package does not exist`r`n`t$packageCachePath"
                }
            }
        }
    }
}

# Publish packages to NuGet repository (where a key and NugetServer have been provided).
if ($ShouldPublishRemote) {
	Get-ChildItem -Path $publishFolder -Filter *.nupkg | ForEach-Object {
		$package = Join-Path $publishFolder $_.Name
		$pushCommand = "dotnet nuget push --source $($nugetServer) --api-key $($apiKey) $($package) --skip-duplicate"

		Write-Host "`r`n$($pushCommand)" -ForegroundColor Yellow

		Invoke-Expression $pushCommand
	}
}