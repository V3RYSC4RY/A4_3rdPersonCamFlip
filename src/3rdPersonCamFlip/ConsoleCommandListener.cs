using System;
using System.Threading;
using UnityEngine;

namespace ThirdPersonCamFlip
{
    internal static class ConsoleCommandListener
    {
        private static Thread _thread;
        private static bool _running;

        public static void Start()
        {
            if (_running)
                return;

            _running = true;
            _thread = new Thread(ReadCommands)
            {
                IsBackground = true,
                Name = "3rdPersonCamFlip Console Commands"
            };
            _thread.Start();
        }

        private static void ReadCommands()
        {
            ThirdPersonCamFlipPlugin.LogSource?.LogInfo("[3PCF] Console commands ready. Use: camflip bind <KeyCode>");

            while (_running)
            {
                string line;
                try
                {
                    line = Console.ReadLine();
                }
                catch (Exception ex)
                {
                    ThirdPersonCamFlipPlugin.LogSource?.LogDebug("[3PCF] Console command listener stopped: " + ex.Message);
                    return;
                }

                if (line == null)
                    return;

                Handle(line);
            }
        }

        private static void Handle(string line)
        {
            line = line.Trim();
            if (line.Length == 0)
                return;

            string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return;

            int commandIndex = 0;
            string root = parts[0].ToLowerInvariant();
            if (root == "3pcf" || root == "3rdpersoncamflip")
            {
                commandIndex = 1;
                if (parts.Length <= commandIndex)
                {
                    Log(CamFlipCommand.Help());
                    return;
                }
                root = parts[commandIndex].ToLowerInvariant();

                if (root == "help")
                {
                    Log(CamFlipCommand.Help());
                    return;
                }
            }

            if (root != "camflip")
                return;

            int argCount = parts.Length - commandIndex - 1;
            string[] args = new string[argCount];
            Array.Copy(parts, commandIndex + 1, args, 0, argCount);
            Log(CamFlipCommand.Execute(args));
        }

        private static void Log(string message)
        {
            ThirdPersonCamFlipPlugin.LogSource?.LogInfo("[3PCF] " + message.Replace("\n", " "));
        }
    }
}
