# SGReader.Net

Extract sprites from the Impressions Games city-building titles (Caesar III, Pharaoh, Zeus, Emperor, etc.), inspired by [Pecunia SGReader](https://github.com/bvschaik/citybuilding-tools).

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Windows (WPF + `System.Drawing`)

UI stack: [WPF-UI](https://github.com/lepoco/wpfui) + CommunityToolkit.Mvvm.

## Build

```bash
dotnet build src/SGReader.sln -c Release
```

## UI

```bash
dotnet run --project src/SGReader -c Release
```

Open `.sg2` / `.sg3` files (keep matching `.555` files next to them, or in a `555/` subfolder).

## CLI

```bash
# List file info
dotnet run --project src/SGReader.CLI -c Release -- path\to\file.sg3 --list

# Extract all sprites to PNGs
dotnet run --project src/SGReader.CLI -c Release -- path\to\file.sg3 -o path\to\output
```

Default output directory is `./<sg-name>/`, with one subfolder per bitmap.
