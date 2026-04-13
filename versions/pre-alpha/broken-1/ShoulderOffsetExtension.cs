using UnityEngine;
using Cinemachine;

namespace ThirdPersonCamFix
{
    /// <summary>
    /// CinemachineExtension that applies our shoulder offset
    /// after the main body stage has positioned the camera.
    /// </summary>
    public class ShoulderOffsetExtension : CinemachineExtension
    {
        public override void PostPipelineStageCallback(   // <-- was 'protected override'
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage,
            ref CameraState state,
            float deltaTime)
        {
            // Only care about the "Body" stage—after camera has been placed.
            if (stage != CinemachineCore.Stage.Body)
                return;

            // Only mess with things in third person and when enabled.
            if (!ShoulderState.InThirdPerson || !ShoulderState.Enabled)
                return;

            float offset = ShoulderState.CurrentOffset;
            if (Mathf.Abs(offset) < 0.0001f)
                return;

            // Move the camera along its right vector by offset
            Vector3 right = state.RawOrientation * Vector3.right;
            state.RawPosition += right * offset;
        }
    }
}
