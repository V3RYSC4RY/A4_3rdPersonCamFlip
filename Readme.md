# 3rdPersonCamFlip

ANEURISM IV BepInEx IL2CPP mod for flipping the third-person camera shoulder.

Current active version: **alpha 0.01**  
Plugin package version: `0.0.1-alpha.1`

## Repository Layout

- `src/3rdPersonCamFlip/` - active mod source.
- `versions/alpha/0.01/` - snapshot of the old `ThirdPersonCamFix - 0.0.4` source, now treated as the first alpha prototype.
- `versions/pre-alpha/` - predecessor snapshots from before alpha 0.01.
- `libs/` - local BepInEx, Unity, and game assemblies used for compiling. This folder is ignored by git.
- `bin/` - build output. This folder is ignored by git.

## Build

Copy the required BepInEx, Unity, and game assemblies into `libs/`, then run:

```powershell
dotnet build .\src\3rdPersonCamFlip\3rdPersonCamFlip.csproj -c Release
```

The compiled mod will be written to:

```text
bin/3rdPersonCamFlip/3rdPersonCamFlip.dll
```

`update_3rdpersoncamflip.ps1` builds the active project and copies the DLL into the local ANEURISM IV BepInEx plugins folder.

## Version Archive

The active project is the one under `src/3rdPersonCamFlip/`.

Older source snapshots should stay under `versions/` using this convention:

```text
versions/
  alpha/
    0.01/
  pre-alpha/
    0.0.2/
    0.0.3/
    broken-1/
    broken-2/
```

When making a new release, copy the active source into a new folder under `versions/alpha/` or a later release channel before continuing work.
