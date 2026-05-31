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
        private const int MaxRememberedFollowPoints = 64;

        public static void ClearCachedState()
        {
            OriginalFollowLocalPos.Clear();
        }

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
            {
                if (OriginalFollowLocalPos.Count >= MaxRememberedFollowPoints)
                    OriginalFollowLocalPos.Clear();

                OriginalFollowLocalPos[id] = follow.localPosition;
            }

            var orig = OriginalFollowLocalPos[id];

            if (ShoulderState.CurrentShoulderSign >= 0f)
            {
                // Restore original on right.
                if ((follow.localPosition - orig).sqrMagnitude > 0.000001f)
                    follow.localPosition = orig;
                return;
            }

            // On the mirrored fallback side, shift along local X by a smoothed factor.
            float factor = Mathf.Clamp01(-ShoulderState.CurrentShoulderSign); // 0 on default (+1), 1 on alternate (-1)
            float shift = Mathf.Abs(ShoulderState.FallbackShoulderOffset) * factor;
            Vector3 newPos = orig;
            newPos.x = orig.x - shift;

            if ((follow.localPosition - newPos).sqrMagnitude > 0.000001f)
                follow.localPosition = newPos;
        }
    }
}
