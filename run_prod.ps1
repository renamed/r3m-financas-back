# check whether git branch is main
if ((git rev-parse --abbrev-ref HEAD) -eq "main") {    
    Write-Information "You are on the main branch. Proceeding with production build..."
    dotnet build --configuration Release
}
else {
    Write-Warning "You are not on the main branch. We'll run the last production build instead."
}

& dotnet ./R3M.Financas.Back.Api/bin/Release/net9.0/R3M.Financas.Back.Api.dll --urls "http://localhost:7050"