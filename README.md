[![.NET](https://github.com/teamRokuro/Fp/actions/workflows/dotnet.yml/badge.svg)](https://github.com/teamRokuro/Fp/actions/workflows/dotnet.yml)
[![NuGet](https://img.shields.io/nuget/v/Fp.svg)](https://www.nuget.org/packages/Fp/)

## File processing

| Package                | Release |
|------------------------|---------|
| `Fp`           | [![NuGet](https://img.shields.io/nuget/v/Fp.svg)](https://www.nuget.org/packages/Fp/)|
| `Fp.Templates` | [![NuGet](https://img.shields.io/nuget/v/Fp.Templates.svg)](https://www.nuget.org/packages/Fp.Templates/) |

[Documentation](https://teamrokuro.github.io/Fp) | [Samples](https://github.com/Lucina/FpProcessorSamples)

### Libraries
* [Fp](src/Fp): Base file processing library
    - Minimum API: .NET Standard 2.0
### Scripting
* [fpx](src/fpx): Script execution program (thin wrapper of [dotnet-script](https://github.com/filipw/dotnet-script))
    - Requires [.NET 5 SDK](https://get.dot.net/) for execution
* [Dereliction](src/Dereliction): Basic Avalonia-based cross-platform script editor / testing program
    - Requires [.NET 5 SDK](https://get.dot.net/) for execution