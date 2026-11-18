$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Set-Location (Join-Path $root 'src\FinanceShopping.Web')
$env:ASPNETCORE_ENVIRONMENT = 'Production'
$env:ASPNETCORE_URLS = 'http://0.0.0.0:5188'
# LAN demo is explicitly enabled by this script; localhost mode stays restricted.
$env:AllowedHosts = '*'
try {
    if (!(Test-Path 'appsettings.Local.json')) { throw 'Chay 01_CAI_DAT.cmd truoc.' }
    & dotnet build -c Release
    if ($LASTEXITCODE -ne 0) { throw 'Build that bai.' }
    Write-Host 'May tinh va dien thoai phai cung Wi-Fi. Dung Wi-Fi nha de demo.'
    Write-Host 'Tren dien thoai, mo http://DIA_CHI_IPV4_CUA_MAY_TINH:5188'
    Write-Host 'Cac IPv4 duoi day chi la ung vien; chon dia chi cua card Wi-Fi dang dung:'
    if (Get-Command Get-NetIPAddress -ErrorAction SilentlyContinue) {
        Get-NetIPAddress -AddressFamily IPv4 |
            Where-Object { $_.IPAddress -notmatch '^(127\.|169\.254\.)' } |
            Select-Object InterfaceAlias, IPAddress | Format-Table -AutoSize | Out-Host
    }
    Write-Host 'Script khong tu thay doi tuong lua. Neu bi chan, xem docs/DIEN_THOAI_VA_GIAO_DIEN.md.'
    Write-Host 'Nhan Ctrl+C de dung. Khong chay dong thoi voi 02_CHAY_UNG_DUNG.cmd.'
    & dotnet run -c Release --no-build --no-launch-profile
    if ($LASTEXITCODE -ne 0) { throw 'Ung dung dung vi loi. Xem nhat ky ben tren.' }
} catch { Write-Host $_ -ForegroundColor Red; exit 1 }
