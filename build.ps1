$ErrorActionPreference = 'Stop'
$v = ([xml](Get-Content src\BareWin\BareWin.csproj)).Project.PropertyGroup.Version
dotnet publish src\BareWin -c Release -o out
New-Item dist -ItemType Directory -Force | Out-Null
wix build BareWin.wxs -arch x64 -o "dist\BareWin-$v-x64.msi"
Copy-Item out\BareWin.exe "dist\BareWin-$v.exe" -Force
Write-Output "dist\BareWin-$v.exe"
