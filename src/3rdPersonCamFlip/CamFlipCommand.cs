using System;
using UnityEngine;

namespace ThirdPersonCamFlip
{
    internal static class CamFlipCommand
    {
        private const int Mode1 = 1;
        private const int Mode2 = 2;
        private const int Mode3A = 3;
        private const int Mode3B = 4;

        public static string Execute(string[] args)
        {
            if (args == null || args.Length == 0)
                return Current();

            if (Is(args[0], "help"))
                return Help();

            if (IsModeName(args[0]))
                return SetMode(args[0]);

            if (Is(args[0], "bind"))
            {
                if (args.Length < 2)
                    return Usage();

                return Bind(args[1]);
            }

            if (Is(args[0], "invertwheel") || Is(args[0], "invert"))
            {
                if (args.Length < 2)
                    return SetInvertWheelTilt(!ThirdPersonCamFlipPlugin.InvertWheelTilt.Value);

                if (TryParseBool(args[1], out bool invert))
                    return SetInvertWheelTilt(invert);

                return "Invalid invert value: " + args[1] + "\nUse: camflip invertwheel on | off";
            }

            return "Unknown camflip command: " + args[0] + "\n" + Usage();
        }

        public static string Current()
        {
            return "Mode: " + FormatMode(ThirdPersonCamFlipPlugin.Mode.Value) + ", CamFlip bind: " + ThirdPersonCamFlipPlugin.CamFlip.Value + ", InvertWheelTilt: " + ThirdPersonCamFlipPlugin.InvertWheelTilt.Value;
        }

        public static string Usage()
        {
            return "Usage: camflip 1 | camflip 2 | camflip 3 | camflip 3a | camflip 3b | camflip bind <KeyCode> | camflip invertwheel on|off";
        }

        public static string Help()
        {
            return "Commands:\ncamflip\ncamflip help\ncamflip 1\ncamflip 2\ncamflip 3\ncamflip 3a\ncamflip 3b\ncamflip bind <KeyCode>\ncamflip invertwheel on\ncamflip invertwheel off";
        }

        public static string SetMode(string modeName)
        {
            if (!TryParseMode(modeName, out int mode))
                return "Invalid mode: " + modeName + "\n" + Usage();

            ThirdPersonCamFlipPlugin.Mode.Value = mode;
            ThirdPersonCamFlipPlugin.SaveConfigInPlace();

            if (mode == Mode1)
                return "CamFlip mode set to 1. Use the CamFlip bind to swap shoulders in third person.";

            if (mode == Mode2)
            {
                ShoulderState.UseDefaultShoulder();
                return "CamFlip mode set to 2. Camera toggle sequence is first person, third person default, third person alternate.";
            }

            if (mode == Mode3A)
                return "CamFlip mode set to 3a. Mouse wheel tilt selects left/right shoulder while already in third person.";

            return "CamFlip mode set to 3b. Mouse wheel tilt enters third person from first person and selects that shoulder; middle mouse still toggles normally.";
        }

        public static string Bind(string keyName)
        {
            if (string.IsNullOrWhiteSpace(keyName))
                return Usage();

            if (!Enum.TryParse(keyName, true, out KeyCode key))
                return "Invalid KeyCode: " + keyName + "\nExamples: C, V, Mouse4, JoystickButton4, None";

            ThirdPersonCamFlipPlugin.CamFlip.Value = key;
            ThirdPersonCamFlipPlugin.SaveConfigInPlace();
            return "CamFlip set to " + key + " and saved to 3rdPersonCamFlip.cfg";
        }

        private static string SetInvertWheelTilt(bool invert)
        {
            ThirdPersonCamFlipPlugin.InvertWheelTilt.Value = invert;
            ThirdPersonCamFlipPlugin.SaveConfigInPlace();
            return "InvertWheelTilt set to " + invert + " and saved to 3rdPersonCamFlip.cfg";
        }

        private static bool TryParseBool(string value, out bool result)
        {
            if (Is(value, "on") || Is(value, "true") || Is(value, "1") || Is(value, "yes"))
            {
                result = true;
                return true;
            }

            if (Is(value, "off") || Is(value, "false") || Is(value, "0") || Is(value, "no"))
            {
                result = false;
                return true;
            }

            result = false;
            return false;
        }

        private static bool Is(string value, string expected)
        {
            return string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsModeName(string value)
        {
            return TryParseMode(value, out _);
        }

        private static bool TryParseMode(string modeName, out int mode)
        {
            mode = 0;
            if (Is(modeName, "1"))
            {
                mode = Mode1;
                return true;
            }

            if (Is(modeName, "2"))
            {
                mode = Mode2;
                return true;
            }

            if (Is(modeName, "3") || Is(modeName, "3a") || Is(modeName, "3am"))
            {
                mode = Mode3A;
                return true;
            }

            if (Is(modeName, "3b"))
            {
                mode = Mode3B;
                return true;
            }

            return false;
        }

        private static string FormatMode(int mode)
        {
            if (mode == Mode3A)
                return "3a";

            if (mode == Mode3B)
                return "3b";

            return mode.ToString();
        }
    }
}
