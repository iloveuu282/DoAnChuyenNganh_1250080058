$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src\FinanceShopping.Web'
Set-Location $project
try {
    $sdk = (& dotnet --version)
    if ($LASTEXITCODE -ne 0 -or $sdk -notmatch '^10\.') { throw 'Can .NET SDK 10. Chay dotnet --list-sdks de kiem tra.' }
    Write-Host 'Nhap Server name giong trong SSMS, dung Windows Authentication.'
    Write-Host 'Vi du: localhost | .\SQLEXPRESS | (localdb)\MSSQLLocalDB'
    $server = Read-Host 'Server name [Enter = (localdb)\MSSQLLocalDB]'
    if ([string]::IsNullOrWhiteSpace($server)) { $server = '(localdb)\MSSQLLocalDB' }
    if ($server.Contains(';')) { throw 'Server name khong duoc chua dau cham phay.' }
    $connection = "Server=$server;Database=FinanceShopping;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=False"
    @{ConnectionStrings=@{DefaultConnection=$connection}} | ConvertTo-Json -Depth 4 | Set-Content -Encoding UTF8 'appsettings.Local.json'
    & dotnet build -c Release
    if ($LASTEXITCODE -ne 0) { throw 'Build that bai. Kiem tra Internet/NuGet va SDK.' }
    & dotnet run -c Release --no-build --no-launch-profile -- --seed-demo
    if ($LASTEXITCODE -ne 0) { throw 'Khoi tao that bai. Kiem tra Server name, dich vu SQL Server va quyen tao database.' }
    Write-Host 'HOAN TAT. Chay 02_CHAY_UNG_DUNG.cmd. Demo: demo@example.com / Demo@2026!'
} catch { Write-Host $_ -ForegroundColor Red; exit 1 }
