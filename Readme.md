# 3rdPersonCamFlip

ANEURISM IV BepInEx IL2CPP mod for flipping the third-person camera shoulder.
<p>Press CamFlip while in third-person view to toggle the camera from left/right shoulder mode.

### Default keybind:
CamFlip = **C**

## Version:
Current active version: **alpha 0.02**

Plugin package version: `0.0.2-alpha.1`

## Source history:
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

## Console Commands

The in-game console accepts:

```text
camflip
camflip help
camflip 1
camflip 2
camflip bind <KeyCode>
```

When the BepInEx console is enabled, the mod also accepts:

```text
3pcf camflip
3pcf camflip help
3pcf camflip 1
3pcf camflip 2
3pcf camflip bind <KeyCode>
3pcf help
```

Examples:

```text
camflip bind V
camflip bind Mouse4
camflip bind None
```

Mode 1 uses the `CamFlip` bind to swap shoulders while in third person.
Mode 2 disables the separate swap bind and changes the normal camera toggle sequence to first person, third person default, third person alternate.

Changing `CamFlip` saves the new value to `BepInEx/config/3rdPersonCamFlip.cfg`.
