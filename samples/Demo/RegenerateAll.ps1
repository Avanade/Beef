Get-ChildItem -LiteralPath . -Filter 'generated' -Directory -Recurse | Get-ChildItem | Remove-Item -Recurse -Force
cd Beef.Demo.CodeGen
dotnet run all
cd ../Beef.Demo.Database
dotnet run all