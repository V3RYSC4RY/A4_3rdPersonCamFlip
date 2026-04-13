using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystems;
using Managers;
using Mirror;

namespace ThirdPersonCamFlip
{
    [HarmonyPatch(typeof(CommandManager), "Awake")]
    internal static class GameConsoleCommandPatches
    {
        [HarmonyPostfix]
        private static void Awake_Postfix(CommandManager __instance)
        {
            Register(__instance);
        }

        private static void Register(CommandManager commandManager)
        {
            if (commandManager == null)
                return;

            try
            {
                if (CommandManager.TryGetCommand("camflip", out _))
                    return;

                var execute = DelegateSupport.ConvertDelegate<Il2CppSystem.Action<Il2CppStringArray, NetworkConnectionToClient>>(
                    new System.Action<Il2CppStringArray, NetworkConnectionToClient>(ExecuteCamFlip)
                );
                var command = new CommandManager.Command("camflip", string.Empty, true, execute);
                commandManager.RegisterCommand(command);
                ThirdPersonCamFlipPlugin.LogSource?.LogInfo("[3PCF] Registered in-game console command: camflip");
            }
            catch (System.Exception ex)
            {
                ThirdPersonCamFlipPlugin.LogSource?.LogWarning("[3PCF] Failed to register in-game console command: " + ex);
            }
        }

        private static void ExecuteCamFlip(Il2CppStringArray args, NetworkConnectionToClient _)
        {
            string[] managedArgs = ToManagedArgs(args);
            string message = CamFlipCommand.Execute(managedArgs);

            ThirdPersonCamFlipPlugin.LogSource?.LogInfo("[3PCF] " + message.Replace("\n", " "));

            try
            {
                ConsoleManager.Log("[3PCF] " + message, ConsoleManager.LogType.Client);
            }
            catch
            {
                // If the UI console is unavailable, the BepInEx log still receives the result.
            }
        }

        private static string[] ToManagedArgs(Il2CppStringArray args)
        {
            if (args == null || args.Length == 0)
                return new string[0];

            string[] managedArgs = new string[args.Length];
            for (int i = 0; i < args.Length; i++)
                managedArgs[i] = args[i];

            return managedArgs;
        }
    }
}
