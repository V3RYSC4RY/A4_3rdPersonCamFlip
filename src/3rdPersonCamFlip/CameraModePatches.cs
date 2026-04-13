using HarmonyLib;

namespace ThirdPersonCamFlip
{
    [HarmonyPatch(typeof(CameraManager), "set_IsFirstPerson")]
    internal static class CameraModePatches
    {
        [HarmonyPrefix]
        private static bool IsFirstPerson_Setter_Prefix(bool value)
        {
            if (ThirdPersonCamFlipPlugin.Mode?.Value != 2)
                return true;

            bool wasFirstPerson;
            try
            {
                wasFirstPerson = CameraManager.IsFirstPerson;
            }
            catch
            {
                return true;
            }

            if (value)
            {
                if (!wasFirstPerson && ShoulderState.IsSwapped)
                {
                    ShoulderState.UseDefaultShoulder();
                    ThirdPersonCamFlipPlugin.LogSource?.LogDebug("[3PCF] Mode 2 advanced to the alternate third person shoulder.");
                    return false;
                }

                ShoulderState.UseSwappedShoulder();
                return true;
            }

            ShoulderState.UseSwappedShoulder();
            return true;
        }
    }
}
