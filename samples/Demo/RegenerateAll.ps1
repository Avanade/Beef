###############################################################
#
# Removes all generated files then re-generates them running
# CodeGen 
#     Beef.Demo.CodeGen/Beef.Demo.CodeGen.csproj
# Database CodeGen
#     Beef.Demo.Database/Beef.Demo.Database.csproj
#
###############################################################

#Get the current script path
$path = $PSScriptRoot

Write-Host "Removing all generated files path:'$path'" -ForegroundColor Green
Get-ChildItem -LiteralPath $path -Filter 'generated' -Directory -Recurse | Get-ChildItem | Remove-Item -Recurse -Force

$CodeGenPath = (Join-Path $path 'Beef.Demo.CodeGen/Beef.Demo.CodeGen.csproj')
Write-Host "Running Beef.Demo.CodeGen path:'$CodeGenPath'" -ForegroundColor Green
dotnet run all --project $CodeGenPath

$DatabasePath = (Join-Path $path 'Beef.Demo.Database/Beef.Demo.Database.csproj')
Write-Host "Running Beef.Demo.Database path:'$DatabasePath'"-ForegroundColor Green
dotnet run all --project $DatabasePath