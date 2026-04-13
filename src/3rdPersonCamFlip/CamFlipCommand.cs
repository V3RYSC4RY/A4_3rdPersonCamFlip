using System;
using UnityEngine;

namespace ThirdPersonCamFlip
{
    internal static class CamFlipCommand
    {
        public static string Execute(string[] args)
        {
            if (args == null || args.Length == 0)
                return Current();

            if (Is(args[0], "help"))
                return Help();

            if (Is(args[0], "1") || Is(args[0], "2"))
                return SetMode(args[0]);

            if (Is(args[0], "bind"))
            {
                if (args.Length < 2)
                    return Usage();

                return Bind(args[1]);
            }

            return "Unknown camflip command: " + args[0] + "\n" + Usage();
        }

        public static string Current()
        {
            return "Mode: " + ThirdPersonCamFlipPlugin.Mode.Value + ", CamFlip bind: " + ThirdPersonCamFlipPlugin.CamFlip.Value;
        }

        public static string Usage()
        {
            return "Usage: camflip 1 | camflip 2 | camflip bind <KeyCode>";
        }

        public static string Help()
        {
            return "Commands:\ncamflip\ncamflip help\ncamflip 1\ncamflip 2\ncamflip bind <KeyCode>";
        }

        public static string SetMode(string modeName)
        {
            if (!int.TryParse(modeName, out int mode) || (mode != 1 && mode != 2))
                return "Invalid mode: " + modeName + "\n" + Usage();

            ThirdPersonCamFlipPlugin.Mode.Value = mode;
            ThirdPersonCamFlipPlugin.CustomConfig.Save();

            if (mode == 1)
                return "CamFlip mode set to 1. Use the CamFlip bind to swap shoulders in third person.";

            ShoulderState.UseDefaultShoulder();
            return "CamFlip mode set to 2. Camera toggle sequence is first person, third person default, third person alternate.";
        }

        public static string Bind(string keyName)
        {
            if (string.IsNullOrWhiteSpace(keyName))
                return Usage();

            if (!Enum.TryParse(keyName, true, out KeyCode key))
                return "Invalid KeyCode: " + keyName + "\nExamples: C, V, Mouse4, JoystickButton4, None";

            ThirdPersonCamFlipPlugin.CamFlip.Value = key;
            ThirdPersonCamFlipPlugin.CustomConfig.Save();
            return "CamFlip set to " + key + " and saved to 3rdPersonCamFlip.cfg";
        }

        private static bool Is(string value, string expected)
        {
            return string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);
        }
    }
}
