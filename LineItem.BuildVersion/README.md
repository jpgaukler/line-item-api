# LineItem.BuildVersion

A utility project that allows you to embed a custom application version string
directly into your compiled .NET assemblies, exposing a clean C# API to read them at runtime.

---

## How It Works

This project uses a hybrid approach combining **MSBuild automation** and **C# Reflection** to
handle version tracking cleanly without configuration file manipulation:

1. **Build-Time Injection:** When you build your application using the `-p:BuildVersion` flag, a custom MSBuild targets
   file (`BuildVersion.targets`) intercepts the build pipeline.
2. **Metadata Baking:** MSBuild dynamically generates a hidden `AssemblyMetadataAttribute` and bakes your version string
   directly into your application's compiled intermediate language (IL) bytecode binary.
3. **Runtime Retrieval:** The `BuildVersionProvider` class uses highly efficient, single-evaluation reflection to pull
   the embedded metadata out of the running assembly instantly.

---

## Project Structure

```text
LineItem.Versioning/
├── BuildVersion.targets      # The MSBuild automation rules
└── BuildVersionProvider.cs   # The C# helper class to read the version
```

---

## Usage

Add the following code to your application's `.csproj` file:

```xml
<ItemGroup>
    <ProjectReference Include="..\LineItem.BuildVersion\LineItem.BuildVersion.csproj"/>
</ItemGroup>

<Import Project="..\LineItem.BuildVersion\BuildVersion.targets"/>
```

Pass the version string to MSBuild using the `--property:BuildVersion` flag:

```powershell
dotnet build "LineItem.Api.csproj" --property:BuildVersion=1.0.0
```

Read the version string using the `BuildVersionProvider` class:

```csharp
var version = BuildVersionProvider.GetVersion();
```
