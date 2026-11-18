$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
Set-Location (Join-Path $root 'src\FinanceShopping.Web')
$env:ASPNETCORE_ENVIRONMENT='Production'
$env:ASPNETCORE_URLS='http://localhost:5188'
try {
    if (!(Test-Path 'appsettings.Local.json')) { throw 'Chay 01_CAI_DAT.cmd truoc.' }
    & dotnet build -c Release
    if ($LASTEXITCODE -ne 0) { throw 'Build that bai.' }
    Start-Process 'http://localhost:5188'
    & dotnet run -c Release --no-build --no-launch-profile
    if ($LASTEXITCODE -ne 0) { throw 'Ung dung dung vi loi. Doc nhat ky ben tren.' }
} catch {Write-Host $_ -ForegroundColor Red;exit 1}
