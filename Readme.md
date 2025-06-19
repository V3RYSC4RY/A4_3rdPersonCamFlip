# Abandoned; I refunded the game and will no longer be working on this. Fork if you want to.

# ANEURISM IV's EULA means modding is not allowed. Unless they like you; then you can mod the game. But only if it's mods they like. This at-will retroactive EULA is bullshit and breaks Australian Consumer Law and I don't plan to deal with it.

## Requirements:

### Install BepInExPack_IL2CPP from Thunderstore

1. Navigate to https://thunderstore.io/c/sons-of-the-forest/p/BepInEx/BepInExPack_IL2CPP/
2. Get the archive of the pack by clicking `Manual Download`
3. Follow the instructions for `Manual Installation`
    1. Extract the archive into a folder. Do not extract into the game folder.
    2. Move the contents of the `BepInExPack` folder into the game folder (where the game executable is located).
    3. Run the game.
        - If you are on `Linux`, you must add the following to your Launch Options in Steam: `WINEDLLOVERRIDES="winhttp.dll=n,b" %command%`
4. Follow the instructions for the specific `Release` version of the mod.

### Where do I get your compiled mods and not their source?
Get the mods you want from this repository's `releases` section: https://github.com/FatigueDev/aneurism_iv_modding/releases


### How do I make my own mods?

1. Follow the instructions [above to install BepInEx](#install-bepinexpack_il2cpp-from-thunderstore)
2. Clone the project by heading into a folder and calling `git clone https://github.com/FatigueDev/aneurism_iv_modding.git`
3. Create a folder in the root of the cloned project named `libs`
4. Copy core assemblies you may need from `ANEURISM IV/BepInEx/core/` to `aneurism_iv_modding/libs/`
5. Copy interop assemblies you may need from `ANEURISM IV/BepInEx/interop/` to `aneurism_iv_modding/libs/`
6. Change directory to `aneurism_iv_modding` root
7. Run `dotnet build ./some_mod/some_mod.csproj` 

The `Directory.Build.props` file will read any `.dll` files that are in `aneurism_iv_modding/libs/` and projects built in the hierarchy will automatically use them as requirements.

**Files currently in use are**:</br>
0Harmony.dll</br>
Il2Cppmscorlib.dll</br>
Il2CppSystem.Drawing.dll</br>
Il2CppSystem.Xml.Linq.dll</br>
UnityEngine.InputLegacyModule.dll</br>
Assembly-CSharp.dll</br>
Il2CppSystem.Configuration.dll</br>
Il2CppSystem.Globalization.dll</br>
Mirror.dll</br>
UnityEngine.InputModule.dll</br>
BepInEx.Core.dll</br>
Il2CppSystem.Core.dll</br>
Il2CppSystem.Numerics.dll</br>
UnityEngine.AssetBundleModule.dll</br>
Unity.RenderPipelines.Core.Runtime.dll</br>
BepInEx.Unity.IL2CPP.dll</br>
Il2CppSystem.Data.dll</br>
Il2CppSystem.Runtime.Serialization.dll</br>
UnityEngine.CoreModule.dll</br>
Il2CppInterop.Runtime.dll</br>
Il2CppSystem.dll</br>
Il2CppSystem.Xml.dll</br>
UnityEngine.dll</br>

