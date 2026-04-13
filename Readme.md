# 3rdPersonCamFlip

## Version

Current active version: **alpha 0.02**

Plugin package version: `0.0.2-alpha.1`

## Description

3rdPersonCamFlip is a BepInEx IL2CPP mod for ANEURISM IV that adds shoulder camera control while using third-person view.

The mod can either bind shoulder swapping to a dedicated key or extend the game's normal camera toggle sequence with an additional third-person shoulder state.

## Modes

### Mode 1: CamFlip keybind

Mode 1 keeps the game's default camera toggle behavior and uses the `CamFlip` keybind to swap shoulders while already in third person.

In this mode, the camera flow remains:

```text
First person -> Third person -> First person
```

While in third person, pressing `CamFlip` swaps between the default and alternate shoulder views.

### Mode 2: Camera toggle sequence

Mode 2 disables the separate shoulder swap key and adds the alternate shoulder view directly into the game's normal camera toggle sequence.

In this mode, the camera flow becomes:

```text
First person -> Third person default -> Third person alternate -> First person
```

## Commands

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

Command behavior:

- `camflip` shows the current mode and keybind.
- `camflip help` shows command usage.
- `camflip 1` switches to Mode 1.
- `camflip 2` switches to Mode 2.
- `camflip bind <KeyCode>` changes the Mode 1 shoulder swap key.

Examples:

```text
camflip bind V
camflip bind Mouse4
camflip bind None
```

Changing `CamFlip` saves the new value to `BepInEx/config/3rdPersonCamFlip.cfg`.

## Default Keybind

```text
CamFlip = C
```

## Build

Copy the required BepInEx, Unity, and game assemblies into `libs/`, then run:

```powershell
dotnet build .\src\3rdPersonCamFlip\3rdPersonCamFlip.csproj -c Release
```

The compiled mod will be written to:

```text
bin/3rdPersonCamFlip/3rdPersonCamFlip.dll
```

## History

- `src/3rdPersonCamFlip/` - active mod source.
- `versions/alpha/0.01/` - snapshot of the old `ThirdPersonCamFix - 0.0.4` source, now treated as the first alpha prototype.
- `versions/pre-alpha/` - predecessor snapshots from before alpha 0.01.
