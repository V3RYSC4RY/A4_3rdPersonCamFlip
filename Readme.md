# 3rdPersonCamFlip

ANEURISM IV BepInEx IL2CPP mod for flipping the third-person camera shoulder.

Current active version: **alpha 0.01**  
Plugin package version: `0.0.1-alpha.1`

## Repository Layout

- `src/3rdPersonCamFlip/` - active mod source.
- `versions/alpha/0.01/` - snapshot of the old `ThirdPersonCamFix - 0.0.4` source, now treated as the first alpha prototype.
- `versions/pre-alpha/` - predecessor snapshots from before alpha 0.01.

## Build

Copy the required BepInEx, Unity, and game assemblies into `libs/`, then run:

```powershell
dotnet build .\src\3rdPersonCamFlip\3rdPersonCamFlip.csproj -c Release
```

The compiled mod will be written to:

```text
bin/3rdPersonCamFlip/3rdPersonCamFlip.dll
```
