using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Cinemachine;

namespace ThirdPersonCamFlip
{
    [HarmonyPatch(typeof(CinemachineBrain))]
    internal static class CinemachineBrainPatches
    {
        private static readonly Dictionary<int, float> OriginalFramingX       = new();
        private static readonly Dictionary<int, float> OriginalFramingScreenX = new();
        private static readonly Dictionary<int, float> OriginalTransposerX    = new();
        private static readonly Dictionary<int, float> OriginalShoulderX      = new();
        private const int MaxRememberedComponents = 64;
        private static int _lastSyncedManagerId;
        private static float _lastSyncedTargetOffset = float.NaN;
        private static float _nextErrorLog;
        private static bool Verbose => ThirdPersonCamFlipPlugin.VerboseLogging;

        /// <summary>
        /// Prefix on CinemachineBrain.LateUpdate.
        /// Runs BEFORE Cinemachine positions the camera, so collision/obstruction uses the mirrored offset.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch("LateUpdate")]
        private static void LateUpdate_Prefix(CinemachineBrain __instance)
        {
            if (!ShoulderState.Enabled || !ShoulderState.InThirdPerson)
                return;

            var active = __instance?.ActiveVirtualCamera;
            if (active == null)
                return;

            float blend = ShoulderState.CurrentShoulderSign;

            try
            {
                TryApplyCameraManagerOffset(blend);
            }
            catch (System.Exception ex)
            {
                LogPatchError(ex);
            }
        }

        public static void ClearCachedState()
        {
            OriginalFramingX.Clear();
            OriginalFramingScreenX.Clear();
            OriginalTransposerX.Clear();
            OriginalShoulderX.Clear();
            _lastSyncedManagerId = 0;
            _lastSyncedTargetOffset = float.NaN;
        }

        public static bool IsDefaultShoulderVisuallyRight()
        {
            const float centerTolerance = 0.01f;

            try
            {
                var mgr = CameraManager.Instance;
                var framing = mgr?.framing;
                if (framing == null)
                    return true;

                int id = framing.GetInstanceID();
                float baseSX = OriginalFramingScreenX.TryGetValue(id, out float remembered)
                    ? remembered
                    : framing.m_ScreenX;

                if (Mathf.Abs(baseSX - 0.5f) <= centerTolerance)
                    return true;

                // With Cinemachine Framing Transposer, a target framed left of center means the camera is
                // over the visual right shoulder; mirrored framing is the visual left shoulder.
                return baseSX < 0.5f;
            }
            catch
            {
                return true;
            }
        }

        private static bool TryApplyCameraManagerOffset(float blend)
        {
            // blend is already smoothed in Controller (+/-1). Clamp just in case.
            float clamped = Mathf.Clamp(blend, -1f, 1f);

            CameraManager mgr;
            try
            {
                mgr = CameraManager.Instance;
            }
            catch
            {
                return false;
            }

            if (mgr == null)
                return false;

            // Use the manager's referenced CinemachineFramingTransposer as the single source of truth.
            var framing = mgr.framing;
            if (framing == null)
                return false;

            int id = framing.GetInstanceID();
            float baseX = RememberOriginal(OriginalFramingX, id, framing.m_TrackedObjectOffset.x);
            float baseSX = RememberOriginal(OriginalFramingScreenX, id, framing.m_ScreenX);

            // Map sign (-1..1) to blend t (0 = right, 1 = left) for smooth lerp.
            float t = Mathf.InverseLerp(1f, -1f, clamped);

            bool hasNativeOffset = Mathf.Abs(baseX) > 0.0001f;
            float mirrorX = hasNativeOffset ? -Mathf.Abs(baseX) : baseX; // keep zero if native X is zero

            float desiredX  = Mathf.Lerp(baseX, mirrorX, t);
            float desiredSX = Mathf.Lerp(baseSX, 1f - baseSX, t);

            bool changed = false;
            if (Mathf.Abs(framing.m_TrackedObjectOffset.x - desiredX) > 0.0001f)
            {
                var trackedOffset = framing.m_TrackedObjectOffset;
                trackedOffset.x   = desiredX;
                framing.m_TrackedObjectOffset = trackedOffset;
                changed = true;
            }

            if (Mathf.Abs(framing.m_ScreenX - desiredSX) > 0.0001f)
            {
                framing.m_ScreenX = Mathf.Clamp01(desiredSX);
                changed = true;
            }

            if (changed || NeedsTargetOffsetSync(mgr, desiredX))
                TrySyncTargetOffset(mgr, desiredX);

            LogOffsets(baseX, framing.m_TrackedObjectOffset.x, desiredX, baseSX, framing.m_ScreenX, desiredSX, clamped);

            return changed;
        }

        private static float _nextLog = 0f;

        private static bool NeedsTargetOffsetSync(CameraManager mgr, float value)
        {
            int id = mgr.GetInstanceID();
            if (id != _lastSyncedManagerId || Mathf.Abs(_lastSyncedTargetOffset - value) > 0.0001f)
                return true;

            try
            {
                return Mathf.Abs(mgr.targetTrackedOffset - value) > 0.0001f;
            }
            catch
            {
                return false;
            }
        }

        private static void TrySyncTargetOffset(CameraManager mgr, float value)
        {
            try
            {
                mgr.targetTrackedOffset = value;
                mgr.UpdateTrackedObjectOffset();
                _lastSyncedManagerId = mgr.GetInstanceID();
                _lastSyncedTargetOffset = value;
            }
            catch { /* ignore */ }
        }

        private static void LogPatchError(System.Exception ex)
        {
            if (ThirdPersonCamFlipPlugin.LogSource == null)
                return;

            float t = Time.unscaledTime;
            const float interval = 5f;
            if (t < _nextErrorLog)
                return;

            _nextErrorLog = t + interval;
            ThirdPersonCamFlipPlugin.LogSource.LogWarning("[3PCF] (PATCH) Failed to apply shoulder offset: " + ex.Message);
        }

        private static void LogOffsets(float baseX, float currentX, float desiredX, float baseSX, float currentSX, float desiredSX, float clamped)
        {
            if (ThirdPersonCamFlipPlugin.LogSource == null)
                return;
            if (!Verbose)
                return;

            float t = Time.unscaledTime;
            const float interval = 1f;
            if (t < _nextLog)
                return;

            _nextLog = t + interval;
            string side = clamped >= 0f ? "RIGHT" : "LEFT";
            ThirdPersonCamFlipPlugin.LogSource.LogDebug(
                $"[3PCF] framing baseX={baseX:F3} curX={currentX:F3} desX={desiredX:F3} baseSX={baseSX:F3} curSX={currentSX:F3} desSX={desiredSX:F3} side={side}"
            );
        }

        private static bool ApplyToCamera(ICinemachineCamera active, float blend)
        {
            // Unused while we drive CameraManager directly. Kept for reference.
            return false;
        }

        private static bool ApplyToVirtualCamera(CinemachineVirtualCamera vcam, float blend)
        {
            return false;
        }

        private static bool TryApplyShoulder(Cinemachine3rdPersonFollow follow, float blend)
        {
            float baseX    = RememberOriginal(OriginalShoulderX, follow.GetInstanceID(), follow.ShoulderOffset.x);
            float desiredX = ComputeLateral(baseX, blend);

            if (Mathf.Abs(follow.ShoulderOffset.x - desiredX) < 0.0001f)
                return true;

            var shoulder = follow.ShoulderOffset;
            shoulder.x   = desiredX;
            follow.ShoulderOffset = shoulder;
            return true;
        }

        private static bool TryApplyTransposer(CinemachineTransposer transposer, float blend)
        {
            float baseX    = RememberOriginal(OriginalTransposerX, transposer.GetInstanceID(), transposer.m_FollowOffset.x);
            float desiredX = ComputeLateral(baseX, blend);

            if (Mathf.Abs(transposer.m_FollowOffset.x - desiredX) < 0.0001f)
                return true;

            var followOffset = transposer.m_FollowOffset;
            followOffset.x   = desiredX;
            transposer.m_FollowOffset = followOffset;
            return true;
        }

        private static bool TryApplyFraming(CinemachineFramingTransposer framing, float blend)
        {
            float baseX    = RememberOriginal(OriginalFramingX, framing.GetInstanceID(), framing.m_TrackedObjectOffset.x);
            float desiredX = ComputeLateral(baseX, blend);

            if (Mathf.Abs(framing.m_TrackedObjectOffset.x - desiredX) < 0.0001f)
                return true;

            var trackedOffset   = framing.m_TrackedObjectOffset;
            trackedOffset.x     = desiredX;
            framing.m_TrackedObjectOffset = trackedOffset;
            return true;
        }

        private static float RememberOriginal(Dictionary<int, float> store, int id, float current)
        {
            if (!store.TryGetValue(id, out var original))
            {
                if (store.Count >= MaxRememberedComponents)
                    store.Clear();

                original   = current;
                store[id]  = original;
            }
            return original;
        }

        // Unused now (Cinemachine fallback removed), but left here in case we re-enable that path later.
        private static float ComputeLateral(float baseX, float blend) => baseX;
    }
}
