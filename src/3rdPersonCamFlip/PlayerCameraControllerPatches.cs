using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Entities.Player;

namespace ThirdPersonCamFlip
{
    [HarmonyPatch(typeof(PlayerCameraController))]
    internal static class PlayerCameraControllerPatches
    {
        private static readonly Dictionary<int, Vector3> OriginalFollowLocalPos = new();

        [HarmonyPostfix]
        [HarmonyPatch("UpdateCamera")]
        private static void UpdateCamera_Postfix(PlayerCameraController __instance)
        {
            if (!ShoulderState.Enabled || !ShoulderState.InThirdPerson)
                return;

            var follow = __instance.cameraFollowPoint;
            if (follow == null)
                return;

            int id = follow.GetInstanceID();

            // Capture original local position once.
            if (!OriginalFollowLocalPos.ContainsKey(id))
                OriginalFollowLocalPos[id] = follow.localPosition;

            var orig = OriginalFollowLocalPos[id];

            if (ShoulderState.CurrentShoulderSign >= 0f)
            {
                // Restore original on right.
                if ((follow.localPosition - orig).sqrMagnitude > 0.000001f)
                    follow.localPosition = orig;
                return;
            }

            // On left, shift along local X by a smoothed factor (uses CurrentShoulderSign lerp).
            float factor = Mathf.Clamp01(-ShoulderState.CurrentShoulderSign); // 0 on right (+1), 1 on full left (-1)
            float shift = Mathf.Abs(ShoulderState.FallbackShoulderOffset) * factor;
            Vector3 newPos = orig;
            newPos.x = orig.x - shift;

            if ((follow.localPosition - newPos).sqrMagnitude > 0.000001f)
                follow.localPosition = newPos;
        }
    }
}
