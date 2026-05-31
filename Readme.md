# 3rdPersonCamFlip

## Version

Current active version: **alpha 0.05**

Plugin package version: `0.0.4-alpha.1`

## Description

3rdPersonCamFlip is a BepInEx IL2CPP mod for ANEURISM IV that adds shoulder camera control while using third-person view.

The mod can bind shoulder swapping to a dedicated key, extend the game's normal camera toggle sequence with an additional third-person shoulder state, or use mouse wheel left/right tilt as explicit shoulder orientation inputs.

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

### Mode 3: Mouse wheel tilt orientation

Mode 3 keeps the game's default camera toggle behavior and uses mouse wheel tilt to choose the shoulder side while already in third person.

In this mode:

```text
Mouse wheel left tilt -> Third person left shoulder
Mouse wheel right tilt -> Third person right/default shoulder
```

### Mode 4: Mouse wheel tilt entry

Mode 4 keeps middle mouse as the normal camera toggle and also lets mouse wheel tilt enter third person directly from first person with the requested shoulder side.

In this mode:

```text
First person + mouse wheel left tilt -> Third person left shoulder
First person + mouse wheel right tilt -> Third person right/default shoulder
Third person + mouse wheel left tilt -> Third person left shoulder
Third person + mouse wheel right tilt -> Third person right/default shoulder
Middle mouse -> Normal camera toggle
```

## Commands

The in-game console accepts:

```text
camflip
camflip help
camflip 1
camflip 2
camflip 3
camflip 4
camflip bind <KeyCode>
camflip invertwheel on
camflip invertwheel off
```

When the BepInEx console is enabled, the mod also accepts:

```text
3pcf camflip
3pcf camflip help
3pcf camflip 1
3pcf camflip 2
3pcf camflip 3
3pcf camflip 4
3pcf camflip bind <KeyCode>
3pcf camflip invertwheel on
3pcf camflip invertwheel off
3pcf help
```

Command behavior:

- `camflip` shows the current mode and keybind.
- `camflip help` shows command usage.
- `camflip 1` switches to Mode 1.
- `camflip 2` switches to Mode 2.
- `camflip 3` switches to Mode 3.
- `camflip 4` switches to Mode 4.
- `camflip bind <KeyCode>` changes the Mode 1 shoulder swap key.
- `camflip invertwheel on` reverses mouse wheel left/right tilt shoulder selection.
- `camflip invertwheel off` uses the default mouse wheel left/right tilt shoulder selection.

Examples:

```text
camflip bind V
camflip bind Mouse4
camflip bind None
```

Changing `CamFlip` saves the new value to `BepInEx/config/3rdPersonCamFlip.cfg`.

Changing `InvertWheelTilt` also saves to `BepInEx/config/3rdPersonCamFlip.cfg`.

## Default Keybind

```text
CamFlip = C
```

## History

- `src/3rdPersonCamFlip/` - active mod source.
- `versions/alpha/0.01/` - snapshot of the old `ThirdPersonCamFix - 0.0.4` source, now treated as the first alpha prototype.
- `versions/pre-alpha/` - predecessor snapshots from before alpha 0.01.
