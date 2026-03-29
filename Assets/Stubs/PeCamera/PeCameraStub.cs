// Stub for Assets/PeCamera/Scripts/PeCamera.cs
// The real PeCamera.cs was excluded from compilation due to CameraForge.Pose vs
// UnityEngine.Pose ambiguity (CS0104). This minimal stub satisfies references to
// the PeCamera static class from other game scripts.
// THESE TYPES MUST NOT BE USED IN RUNTIME CODE.

using UnityEngine;
using System;

public static class PeCamera
{
    public enum ControlMode { MMOControl, FirstPerson, ThirdPerson, FreeLook, Cinematic }

    public static bool is1stPerson { get; set; }
    public static bool isFreeLook => false;
    public static bool cursorLocked { get; set; }
    public static Vector2 cursorPos => Vector2.zero;
    // mousePos: game code adds Vector3 (e.g. 10 * Vector3.forward) to this — must be Vector3.
    public static Vector3 mousePos => Vector3.zero;
    public static Ray mouseRay => default;
    public static Ray cursorRay => default;
    public static bool fpCameraCanRotate { get; set; }
    public static object cameraModeData { get; set; }
    public static Action<ControlMode> onControlModeChange;

    public static Transform cutsceneTransform { get; set; }

    public static void Init() { }
    public static void Update() { }
    public static void SetBool(string name, bool value) { }
    public static void SetFloat(string name, float value) { }
    public static void SetVar(string name, object value) { }
    public static void PlayAttackShake(float intensity, float duration) { }
    public static void PlayAttackShake() { }
    public static void PlayShakeEffect(float intensity, float duration) { }
    public static void PlayShakeEffect(int type, float duration, int flags) { }
    public static void ApplySkCameraEffect(object effect) { }
    public static void ApplySkCameraEffect(int camEffId, object entity) { }
    public static void SetTransform(Transform transform) { }
    public static void SetTransform(string varName, Transform transform) { }
    public static void CrossFade(string stateName, float transitionDuration = 0.25f) { }
    public static void CrossFade(string stateName, float transitionDuration, float speed) { }
    public static void UpdateColor(object param1, object param2) { }
    public static void SetGlobalVector(string name, Vector4 value) { }
    public static void RecordHistory() { }
}
