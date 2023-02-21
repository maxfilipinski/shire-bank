param(
    [string]$OutDir = ".\out"
)

dotnet publish .\ShireBank.Client\ShireBank.Client.csproj -c Release -r win-x64 -o $OutDir\Client --self-contained -p:PublishSingleFile=true -p:PublishTrimmed=true
dotnet publish .\ShireBank.Inspector\ShireBank.Inspector.csproj -c Release -r win-x64 -o $OutDir\Inspector --self-contained -p:PublishSingleFile=true -p:PublishTrimmed=true
dotnet publish .\ShireBank.Server\ShireBank.Server.csproj -c Release -r win-x64 -o $OutDir\Server --self-contained -p:PublishSingleFile=true -p:PublishTrimmed=true