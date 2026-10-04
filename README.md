# SGReader.Net

Another program to extract sprites from the Impressions Games citybuilding games based on Pecunia SGReader (https://github.com/bvschaik/citybuilding-tools).

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Windows (WPF + `System.Drawing`)

UI stack: [WPF-UI](https://github.com/lepoco/wpfui) + CommunityToolkit.Mvvm.

## Build

```bash
dotnet build src/SGReader.sln -c Release
```

## Run

```bash
dotnet run --project src/SGReader -c Release
dotnet run --project src/SGReader.CLI -c Release -- <file.sg2|sg3>
```
