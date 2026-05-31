using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;

namespace ThirdPersonCamFlip
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class ThirdPersonCamFlipPlugin : BasePlugin
    {
        public const string PluginGuid    = "com.veryscary.a4.3rdpersoncamflip";
        public const string PluginName    = "3rdPersonCamFlip";
        public const string PluginVersion = "0.0.4-alpha.7";
        public const string ReleaseName   = "alpha 0.05";

        internal static ThirdPersonCamFlipPlugin Instance;
        internal static ConfigFile              CustomConfig;
        internal static ConfigEntry<KeyCode>    CamFlip;
        internal static ConfigEntry<int>        Mode;
        internal static ConfigEntry<bool>       InvertWheelTilt;
        internal static ManualLogSource         LogSource;
        internal static bool                    VerboseLogging;
        internal static Harmony                 HarmonyInstance;
        internal static string                  ConfigPath;

        public override void Load()
        {
            Instance  = this;
            LogSource = Log;

            // Use a concise config filename instead of the default GUID.
            ConfigPath = Path.Combine(Paths.ConfigPath, "3rdPersonCamFlip.cfg");
            bool cfgExisted = File.Exists(ConfigPath);
            CustomConfig = new ConfigFile(ConfigPath, false);
            CustomConfig.SaveOnConfigSet = false;

            var verbose = CustomConfig.Bind("Logging", "Verbose", false, "Enable extra debug logs for troubleshooting.");
            VerboseLogging = verbose.Value;

            CamFlip = CustomConfig.Bind("Input", "CamFlip", KeyCode.C, "Key used to flip the third-person camera shoulder.");
            Mode = CustomConfig.Bind("Input", "Mode", 1, "1 = separate CamFlip bind; 2 = add swapped shoulder as a third camera toggle state; 3 = 3a wheel tilt selects side in third person; 4 = 3b wheel tilt enters third person from first person with selected side.");
            InvertWheelTilt = CustomConfig.Bind("Input", "InvertWheelTilt", false, "Invert mouse wheel left/right tilt shoulder selection.");

            if (!cfgExisted)
            {
                CustomConfig.Save();
                EnsureHeaderBlock(
                    ConfigPath,
                    new[]
                    {
                        "# 3rd Person Cam Flip",
                        "## <h1> 3rd-person camera view flipping between left or right shoulder",
                        "#",
                        "#   <h2> Features:",
                        "#     - Custom keybind support",
                        "#     - Supports all mouse or keyboard inputs",
                        "#     - 4 different Camera Modes:",
                        "#         <b>1</b> - Press the Cam Flip Key to swap shoulders while in third-person view",
                        "#         <b>2</b> - Adds an additional left-shoulder view to the native camera cycle (1st-person > 3rd-person Left > 3rd-person Right)",
                        "#         <b>3</b> - In 3rd-person, horizontal mouse-wheel-tilt flips shoulder view explicitly",
                        "#         <b>4</b> - In 1st or 3rd-person, horizontal mouse-wheel-tilt enters third-person in the respective direction; subsequent tilts flip shoulders"
                    });
            }

            try
            {
                EnsureModernMetadata();
            }
            catch (Exception ex)
            {
                LogSource?.LogWarning("[3PCF] Metadata normalization skipped due to error: " + ex.Message);
            }

            SaveConfigInPlace();

            // Register IL2CPP MonoBehaviour
            ClassInjector.RegisterTypeInIl2Cpp<CameraShoulderController>();
            // Attach controller to the BepInEx host GameObject
            AddComponent<CameraShoulderController>();

            // Harmony init + patch
            HarmonyInstance = new Harmony(PluginGuid);
            HarmonyInstance.PatchAll();

            ConsoleCommandListener.Start();

            Log.LogInfo($"[3PCF] Loaded {ReleaseName} (IL2CPP type registered, Harmony patched, Mode={Mode.Value}, CamFlip={CamFlip.Value}, InvertWheelTilt={InvertWheelTilt.Value})");
        }

        internal static void SaveConfigInPlace()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ConfigPath))
                    return;

                if (!File.Exists(ConfigPath))
                {
                    CustomConfig?.Save();
                    return;
                }

                UpdateConfigValueInPlace(ConfigPath, "Logging", "Verbose", VerboseLogging ? "true" : "false");
                UpdateConfigValueInPlace(ConfigPath, "Input", "CamFlip", CamFlip == null ? "C" : CamFlip.Value.ToString());
                UpdateConfigValueInPlace(ConfigPath, "Input", "Mode", Mode == null ? "1" : Mode.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                UpdateConfigValueInPlace(ConfigPath, "Input", "InvertWheelTilt", InvertWheelTilt != null && InvertWheelTilt.Value ? "true" : "false");
            }
            catch (Exception ex)
            {
                LogSource?.LogWarning("[3PCF] Failed to save cfg in place: " + ex.Message);
            }
        }

        private static void UpdateConfigValueInPlace(string configPath, string section, string key, string value)
        {
            string original = File.ReadAllText(configPath);
            string nl = original.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
            bool hadTrailingNewline = original.EndsWith("\r\n", StringComparison.Ordinal) || original.EndsWith("\n", StringComparison.Ordinal);
            string[] lines = original.Replace("\r\n", "\n").Split('\n');

            string currentSection = string.Empty;
            bool updated = false;
            for (int i = 0; i < lines.Length; i++)
            {
                string raw = lines[i];
                string trimmed = raw.Trim();
                if (trimmed.StartsWith("[", StringComparison.Ordinal) && trimmed.EndsWith("]", StringComparison.Ordinal))
                {
                    currentSection = trimmed.Substring(1, trimmed.Length - 2).Trim();
                    continue;
                }

                if (!string.Equals(currentSection, section, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (trimmed.StartsWith("#", StringComparison.Ordinal) || trimmed.StartsWith(";", StringComparison.Ordinal))
                    continue;

                int sep = raw.IndexOf('=');
                if (sep <= 0)
                    continue;

                string left = raw.Substring(0, sep);
                string foundKey = left.Trim();
                if (!string.Equals(foundKey, key, StringComparison.OrdinalIgnoreCase))
                    continue;

                string normalized = (value ?? string.Empty).Trim();
                if (string.Equals(foundKey, "CamFlip", StringComparison.OrdinalIgnoreCase))
                    lines[i] = foundKey + " = " + normalized;
                else
                    lines[i] = left + "= " + normalized;
                updated = true;
                break;
            }

            if (!updated)
                return;

            string rewritten = string.Join(nl, lines);
            if (hadTrailingNewline)
                rewritten += nl;
            File.WriteAllText(configPath, rewritten, new UTF8Encoding(false));
        }

        private static void EnsureModernMetadata()
        {
            if (string.IsNullOrWhiteSpace(ConfigPath) || !File.Exists(ConfigPath))
                return;

            string original = File.ReadAllText(ConfigPath);
            string acceptableValues = FindCommentLine(original, "# Acceptable values:");

            string camFlipValue = ReadConfigValue(original, "Input", "CamFlip", CamFlip == null ? "C" : CamFlip.Value.ToString());
            string modeValue = ReadConfigValue(original, "Input", "Mode", Mode == null ? "1" : Mode.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            string invertValue = ReadConfigValue(original, "Input", "InvertWheelTilt", InvertWheelTilt != null && InvertWheelTilt.Value ? "true" : "false");
            string verboseValue = ReadConfigValue(original, "Logging", "Verbose", VerboseLogging ? "true" : "false");

            if (string.IsNullOrWhiteSpace(acceptableValues))
                acceptableValues = "# Acceptable values: (see Unity KeyCode values)";

            string[] lines =
            {
                "# 3rd Person Cam Flip",
                "## <h1> 3rd-person camera view flipping between left or right shoulder",
                "#",
                "#   <h2> Features:",
                "#     - Custom keybind support",
                "#     - Supports all mouse or keyboard inputs",
                "#     - 4 different Camera Modes:",
                "#         <b>1</b> - Press the Cam Flip Key to swap shoulders while in third-person view",
                "#         <b>2</b> - Adds an additional left-shoulder view to the native camera cycle (1st-person > 3rd-person Left > 3rd-person Right)",
                "#         <b>3</b> - In 3rd-person, horizontal mouse-wheel-tilt flips shoulder view explicitly",
                "#         <b>4</b> - In 1st or 3rd-person, horizontal mouse-wheel-tilt enters third-person in the respective direction; subsequent tilts flip shoulders",
                string.Empty,
                "[Input]",
                string.Empty,
                "## Key used to flip the third-person camera shoulder.",
                acceptableValues,
                "# @order 1",
                "# @type String",
                "# @label Cam Flip Key",
                "# @info Keybind to flip third-person shoulder [Mode 1]",
                "# @default C",
                "CamFlip = " + (camFlipValue ?? string.Empty).Trim(),
                string.Empty,
                "## 1 = separate CamFlip bind; 2 = add swapped shoulder as a third camera toggle state; 3 = 3a wheel tilt selects side in third person; 4 = 3b wheel tilt enters third person from first person with selected side.",
                "# @options 1|2|3|4",
                "# @order 2",
                "# @type Int32",
                "# @label Camera Mode",
                "# @info Sets view behavior (1-4)",
                "# @default 1",
                "Mode = " + modeValue,
                string.Empty,
                "# @order 3",
                "# @type Boolean",
                "# @label Invert Wheel Tilt",
                "# @info Invert horizontal mouse-wheel-tilt direction",
                "# @default false",
                "InvertWheelTilt = " + invertValue,
                string.Empty,
                "[Logging]",
                string.Empty,
                "## Enable debug logs for troubleshooting.",
                "# @order 4",
                "# @type Boolean",
                "# @label Verbose Logging",
                "# @info Enable debug logs for troubleshooting",
                "# @default false",
                "Verbose = " + verboseValue,
                string.Empty
            };

            File.WriteAllText(ConfigPath, string.Join("\r\n", lines), new UTF8Encoding(false));
        }

        private static string FindCommentLine(string text, string prefix)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(prefix))
                return string.Empty;

            string[] lines = text.Replace("\r\n", "\n").Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = (lines[i] ?? string.Empty).Trim();
                if (line.StartsWith(prefix, StringComparison.Ordinal))
                    return line;
            }

            return string.Empty;
        }

        private static string ReadConfigValue(string text, string section, string key, string fallback)
        {
            if (string.IsNullOrWhiteSpace(text))
                return fallback ?? string.Empty;

            string[] lines = text.Replace("\r\n", "\n").Split('\n');
            string currentSection = string.Empty;
            for (int i = 0; i < lines.Length; i++)
            {
                string raw = lines[i] ?? string.Empty;
                string trimmed = raw.Trim();
                if (trimmed.StartsWith("[", StringComparison.Ordinal) && trimmed.EndsWith("]", StringComparison.Ordinal))
                {
                    currentSection = trimmed.Substring(1, trimmed.Length - 2).Trim();
                    continue;
                }

                if (!string.Equals(currentSection, section, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (trimmed.StartsWith("#", StringComparison.Ordinal) || trimmed.StartsWith(";", StringComparison.Ordinal))
                    continue;

                int sep = raw.IndexOf('=');
                if (sep <= 0)
                    continue;

                string foundKey = raw.Substring(0, sep).Trim();
                if (!string.Equals(foundKey, key, StringComparison.OrdinalIgnoreCase))
                    continue;

                return raw.Substring(sep + 1).Trim();
            }

            return fallback ?? string.Empty;
        }

        private static void EnsureHeaderBlock(string configPath, string[] headerLines)
        {
            if (string.IsNullOrWhiteSpace(configPath) || headerLines == null || headerLines.Length == 0)
                return;
            if (!File.Exists(configPath))
                return;

            string original = File.ReadAllText(configPath);
            string nl = original.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
            bool hadTrailingNewline = original.EndsWith("\r\n", StringComparison.Ordinal) || original.EndsWith("\n", StringComparison.Ordinal);
            string[] lines = original.Replace("\r\n", "\n").Split('\n');
            int firstSection = GetFirstSectionLineIndex(lines);
            if (firstSection < 0)
                return;
            if (firstSection > 0)
                return;

            var outLines = new List<string>(headerLines.Length + 1 + lines.Length);
            outLines.AddRange(headerLines);
            outLines.Add(string.Empty);
            for (int i = firstSection; i < lines.Length; i++)
                outLines.Add(lines[i]);

            string updated = string.Join(nl, outLines);
            if (hadTrailingNewline)
                updated += nl;
            File.WriteAllText(configPath, updated, new UTF8Encoding(false));
        }

        private static int GetFirstSectionLineIndex(string[] lines)
        {
            if (lines == null)
                return -1;

            for (int i = 0; i < lines.Length; i++)
            {
                string t = (lines[i] ?? string.Empty).Trim();
                if (t.StartsWith("[", StringComparison.Ordinal) && t.EndsWith("]", StringComparison.Ordinal))
                    return i;
            }

            return -1;
        }
    }
}
