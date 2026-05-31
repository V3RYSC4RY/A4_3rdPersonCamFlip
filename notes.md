# 3rdPersonCamFlip Notes (chronological)

- 2026-04-13: Renamed active mod to 3rdPersonCamFlip, moved active source to `src/3rdPersonCamFlip`, and organized old snapshots under `versions/`. The old 0.0.4 snapshot is now the first alpha prototype: alpha 0.01.
- 2026-04-20: Added alpha 0.03 mode 3: mouse wheel left/right tilt selects left/right third-person shoulder orientation while in third person.
- 2026-04-20: Renamed mode 3 to user-facing 3a, kept `camflip 3` as a 3a alias, and added alpha 0.04 mode 3b for wheel-tilt direct entry from first person.
- 2026-05-31: Released alpha 0.05 with latest FPS test and camera behavior refinements.

- 2025-12-07: Initial baseline: FramingTransposer X mirroring in CMBrain prefix; right untouched, left negated; collisions OK on right, weak on left.
- 2025-12-07: Removed final camera transform offsets; avoided CameraManager trackedOffset writes (caused NRE/vertical shifts).
- 2025-12-07: Added screenX mirroring; baseX logs showed 0 (game uses screenX), so left still clipped/right-biased.
- 2025-12-07: Added PlayerCameraController.UpdateCamera postfix to shift cameraFollowPoint.localPosition.x by -FallbackShoulderOffset on left, restore on right. Framing X mirroring still only mirrors if nonzero; screenX mirrors; targetTrackedOffset synced. Build fixed by adding Entities.Player using.
- 2025-12-07: Smoothed follow shift using CurrentShoulderSign lerp (factor 0→1 as sign goes +1→-1) so side swap lerps with the game and is preserved across FP/TP toggles.
- 2025-12-07: Added smooth lerp in CMBrain patch for framing X and screenX based on smoothed shoulder sign; follow shift + screenX mirroring gives matching clamp on both shoulders; side is remembered across FP/TP; left/right swap feels natural.
- Next: test follow-shift impact on left distance/clipping; if still right-biased, consider adjusting constraint or deriving shift from collider radius instead of fallback.
- 2025-12-07: Quieted logging: collapsed load banner to one info line, added Config[Logging.Verbose] toggle, moved lifecycle/mode/toggle/offset logs to Debug gated by verbose; normal startup now emits only the single load line.
- 2025-12-07: Config file renamed to TPCF.cfg (custom ConfigFile) so users can edit Verbose without hunting the GUID file.
- 2025-12-07: Rebuilt and deployed updated DLL after config rename; BepInEx/plugins now has the latest ThirdPersonCamFix.dll.
- 2025-12-07: Added configurable SwapKey (default C) in TPCF.cfg and bumped plugin to 0.0.4 so shoulder swap key can be changed.
- Post-1.0 TODO: remove the verbose logging config if no longer needed after stable release.
