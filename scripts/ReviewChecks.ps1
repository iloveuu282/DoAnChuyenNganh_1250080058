param([string]$Server='.\SQLEXPRESS',[switch]$SqlServer,[string]$OutputDirectory)
$ErrorActionPreference='Stop'
Push-Location (Split-Path $PSScriptRoot -Parent)
$previous=$env:FINANCE_TEST_SQLSERVER
try {
    $folder=$OutputDirectory
    if([string]::IsNullOrWhiteSpace($folder)){$folder=Join-Path 'output/checks' (Get-Date -Format 'yyyyMMdd-HHmmss-fff')}
    New-Item -ItemType Directory -Force $folder | Out-Null
    if($SqlServer){
        # The test executable appends a random run ID. Existing databases are never dropped.
        $env:FINANCE_TEST_SQLSERVER="Server=$Server;Database=FinanceShopping_Review_Test;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;Connect Timeout=10"
        $log=Join-Path $folder 'test_sqlserver.txt'
    }else{Remove-Item Env:FINANCE_TEST_SQLSERVER -ErrorAction SilentlyContinue;$log=Join-Path $folder 'test_sqlite.txt'}
    dotnet run --project tests/FinanceShopping.Checks -c Release *> $log
    $result=$LASTEXITCODE
    Get-Content $log
    Write-Host "Test log: $((Resolve-Path -LiteralPath $log).Path)"
    if($result -ne 0){throw "Tests failed. See $log"}
}finally{$env:FINANCE_TEST_SQLSERVER=$previous;Pop-Location}
