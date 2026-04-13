using HarmonyLib;
using UnityEngine;
using Cinemachine;

namespace ThirdPersonCamFix
{
    [HarmonyPatch(typeof(CinemachineBrain))]
    internal static class CinemachineBrainPatches
    {
        /// <summary>
        /// Postfix on CinemachineBrain.LateUpdate.
        /// This should run AFTER Cinemachine has positioned the camera,
        /// so we can nudge the final output camera sideways.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch("LateUpdate")]
        private static void LateUpdate_Postfix(CinemachineBrain __instance)
        {
            if (!ShoulderState.Enabled)
                return;

            if (!ShoulderState.InThirdPerson)
                return;

            float offset = ShoulderState.CurrentOffset;
            if (Mathf.Abs(offset) < 0.01f)
                return;

            try
            {
                // Use CinemachineBrain's output camera if available,
                // otherwise fall back to Camera.main.
                Camera cam = __instance.OutputCamera;
                if (cam == null)
                    cam = Camera.main;

                if (cam == null)
                    return;

                Transform t = cam.transform;
                if (t == null)
                    return;

                Vector3 right = t.right;
                t.position += right * offset;

                // Optional: uncomment for sanity check spam
                // ThirdPersonCamFixPlugin.LogSource?.LogDebug(
                //     $"[TPCF] (PATCH) Applied shoulder offset={offset}, camPos={t.position}"
                // );
            }
            catch (System.Exception ex)
            {
                ThirdPersonCamFixPlugin.LogSource?.LogError("[TPCF] (PATCH) CMBrain LateUpdate error: " + ex);
            }
        }
    }
}
