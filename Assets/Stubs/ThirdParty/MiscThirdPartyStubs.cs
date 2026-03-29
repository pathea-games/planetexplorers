// Stub implementations for miscellaneous third-party libraries.
// These stubs exist solely so the Roslyn analyzer project compiles.
// THESE TYPES MUST NOT BE USED IN RUNTIME CODE.

using System;
using System.Collections.Generic;
using UnityEngine;

// AnimFollow - ragdoll physics library
namespace AnimFollow
{
    public class AnimFollow_AF : UnityEngine.MonoBehaviour
    {
        public float muscleSpring { get; set; }
        public float muscleDamper { get; set; }
        public UnityEngine.Animator animatorSource { get; set; }
        public bool active { get; set; }
        public UnityEngine.Transform master { get; set; }
        public void ResetModelInfo() { }
    }

    public class RagdollControl_AF : UnityEngine.MonoBehaviour
    {
        public AnimFollow_AF animFollow { get; set; }
        public float blendToRagdoll { get; set; }

        // Unity 4.x legacy active property (removed in Unity 5, but game source uses it)
        public new bool active { get; set; }

        // Fields accessed by PERagdollController
        public UnityEngine.Rigidbody[] slaveRigidBodies;
        public UnityEngine.Transform ragdollRootBone;
        public UnityEngine.Transform masterRootBone;
        public UnityEngine.Transform master;
        public UnityEngine.Animator anim;
        public bool falling;
        public bool gettingUp;
        public float settledSpeed;
        public bool shotByBullet;

        public void GoRagdoll() { }
        public void BlendToAnimated() { }

        // Virtual lifecycle hooks overridden by PERagdollController
        protected virtual bool IsGetupReady() => false;
        protected virtual void OnFallBegin() { }
        protected virtual void OnFallFinished() { }
        protected virtual void OnGetupBegin() { }
        protected virtual void OnGetupFinished() { }
    }
}

// InControl - cross-platform input library
namespace InControl
{
    public enum InputControlType
    {
        None,
        LeftStickX, LeftStickY, LeftStickButton,
        RightStickX, RightStickY, RightStickButton,
        DPadX, DPadY, DPadUp, DPadDown, DPadLeft, DPadRight,
        Action1, Action2, Action3, Action4,
        LeftBumper, RightBumper, LeftTrigger, RightTrigger,
        Start, Back, Select, System, Pause, Menu, Share, View, Options,
        LeftStickLeft, LeftStickRight, LeftStickUp, LeftStickDown,
        RightStickLeft, RightStickRight, RightStickUp, RightStickDown,
        TouchPadTap, TouchPadXAxis, TouchPadYAxis,
        ScrollWheel, TiltX, TiltY, TiltZ, Accelerometer, Gyroscope, Compass,
        Rotate90, Rotate180, Rotate270
    }

    public class InputDevice
    {
        public static InputDevice Null => null;
        public string Name => string.Empty;
        public bool IsAttached => false;
        public InputControl GetControl(InputControlType controlType) => null;
        public float GetAxis(InputControlType controlType) => 0f;
        public bool GetButton(InputControlType controlType) => false;
        public bool GetButtonDown(InputControlType controlType) => false;
        public bool GetButtonUp(InputControlType controlType) => false;
        public InputControl LeftStickX => null;
        public InputControl LeftStickY => null;
        public InputControl RightStickX => null;
        public InputControl RightStickY => null;
        public InputControl Action1 => null;
        public InputControl Action2 => null;
        public InputControl Action3 => null;
        public InputControl Action4 => null;
    }

    public class InputControl
    {
        public float Value => 0f;
        public bool IsPressed => false;
        public bool WasPressed => false;
        public bool WasReleased => false;
        public InputControlType Identifier => InputControlType.None;
        // Implicit conversion to float for axis value usage
        public static implicit operator float(InputControl control) => control?.Value ?? 0f;
    }

    public static class InputManager
    {
        public static InputDevice ActiveDevice => null;
        public static List<InputDevice> Devices => new List<InputDevice>();
        public static void Setup() { }
        public static void Update(float deltaTime) { }
        public static event Action<InputDevice, InputDeviceChange> OnDeviceAttached;
        public static event Action<InputDevice, InputDeviceChange> OnDeviceDetached;
        public static event Action<InputDevice, InputDeviceChange> OnActiveDeviceChanged;
    }

    public enum InputDeviceChange { Attached, Detached, None }

    // InControlManager is a MonoBehaviour singleton (accessed via Instance).
    public class InControlManager : UnityEngine.MonoBehaviour
    {
        public static InControlManager Instance { get; private set; }
        public InputDevice ActiveDevice => null;
        public void Setup() { }
    }
}

// Jboy - JSON serialization library
namespace Jboy
{
    public enum JsonToken { None, ObjectStart, ObjectEnd, ArrayStart, ArrayEnd, PropertyName, String, Number, Boolean, Null }

    public class JsonReader
    {
        public JsonReader(string json) { }
        // Overload for out string (old usage)
        public JsonToken Read(out string propertyName) { propertyName = null; return JsonToken.None; }
        // Overload for out object (used in MyServer.cs etc.)
        public JsonToken Read(out object propertyName) { propertyName = null; return JsonToken.None; }
        public T ReadAs<T>() => default;
        public string ReadPropertyName() => string.Empty;
        // Overload that reads the next property and asserts its name equals expected
        public void ReadPropertyName(string expected) { }
        public void ReadObjectStart() { }
        public void ReadObjectEnd() { }
        public void Close() { }
    }

    public class JsonWriter
    {
        public JsonWriter() { }
        // Constructor overload: (prettyPrint, escapeUnicode, indentDepth)
        public JsonWriter(bool prettyPrint, bool escapeUnicode = false, int indentDepth = 4) { }
        public void WriteObjectStart() { }
        public void WriteObjectEnd() { }
        public void WriteArrayStart() { }
        public void WriteArrayEnd() { }
        public void WriteProperty(string name, string value) { }
        public void WriteProperty(string name, int value) { }
        public void WriteProperty(string name, bool value) { }
        public void WritePropertyName(string name) { }
        public override string ToString() => string.Empty;
    }

    public static class Json
    {
        public static T ReadObject<T>(JsonReader reader) => default;
        // Two-arg version: WriteObject(value, writer) used throughout game code
        public static void WriteObject<T>(T obj, JsonWriter writer) { }
        public static string WriteObject<T>(T obj) => string.Empty;
        public static T Deserialize<T>(string json) => default;
        public static string Serialize<T>(T obj) => string.Empty;
    }
}

// NAudio - .NET audio library
namespace NAudio.Wave
{
    public abstract class WaveStream : IDisposable
    {
        public abstract WaveFormat WaveFormat { get; }
        public virtual long Length => 0;
        public virtual long Position { get; set; }
        public virtual int Read(byte[] buffer, int offset, int count) => 0;
        public void Dispose() { }
    }

    public class WaveFormat
    {
        public int SampleRate { get; }
        public int Channels { get; }
        public int BitsPerSample { get; }
        public WaveFormat(int sampleRate, int channels) { }
    }

    public class WaveFileReader : WaveStream
    {
        public WaveFileReader(string fileName) { }
        public WaveFileReader(System.IO.Stream stream) { }
        public override WaveFormat WaveFormat => null;
    }

    public class Mp3FileReader : WaveStream
    {
        public Mp3FileReader(string fileName) { }
        public Mp3FileReader(System.IO.Stream stream) { }
        public override WaveFormat WaveFormat => null;
    }

    public interface IWaveProvider
    {
        WaveFormat WaveFormat { get; }
        int Read(byte[] buffer, int offset, int count);
    }

    public class WaveFileWriter : IDisposable
    {
        public WaveFileWriter(string filename, WaveFormat format) { }
        // Stream overload: NAudioPlayer.cs passes MemoryStream.
        public WaveFileWriter(System.IO.Stream stream, WaveFormat format) { }
        public void Write(byte[] data, int offset, int count) { }
        public void WriteSample(float sample) { }
        public void Flush() { }
        public void Dispose() { }
        public long Length => 0;
    }

    public class WaveFormatConversionStream : WaveStream
    {
        public WaveFormatConversionStream(WaveFormat targetFormat, WaveStream sourceStream) { }
        public override WaveFormat WaveFormat => null;
        // Static factory: CreatePcmStream wraps any WaveStream into PCM format.
        public static WaveFormatConversionStream CreatePcmStream(WaveStream sourceStream)
            => new WaveFormatConversionStream(null, sourceStream);
    }

    public class WaveOut : IDisposable
    {
        public void Init(IWaveProvider provider) { }
        public void Play() { }
        public void Pause() { }
        public void Stop() { }
        public float Volume { get; set; }
        public PlaybackState PlaybackState => PlaybackState.Stopped;
        public void Dispose() { }
    }

    public enum PlaybackState { Stopped, Playing, Paused }
}

namespace NAudio.Flac
{
    public class FlacReader : NAudio.Wave.WaveStream
    {
        public FlacReader(string fileName) { }
        public FlacReader(System.IO.Stream stream) { }
        public override NAudio.Wave.WaveFormat WaveFormat => null;
    }
}

// DunGen - dungeon generation library
namespace DunGen
{
    public class DungeonGenerator : UnityEngine.MonoBehaviour
    {
        public DungeonGenerator() { }
        public DungeonGenerator(string dungeonFlowPath) { }
        public int Seed { get; set; }
        public int ChosenSeed { get; set; }
        public float LengthMultiplier { get; set; }
        public bool AllowImmediateRepeats { get; set; }
        public Dungeon CurrentDungeon => null;
        public void Generate() { }
        public bool Generate(object manager) => true;
        public void GenerateWithSeed(int seed) { }
        public bool GenerateWithSeed(object manager, int seed) => true;
        public event Action<DungeonGenerator> OnGenerationStatusChanged;
        public GenerationStatus Status => GenerationStatus.None;
    }

    public enum GenerationStatus { None, Failed, Complete, InProgress }

    public class LockedDoor : UnityEngine.MonoBehaviour
    {
        public bool IsOpen { get; set; }
        public void Open() { }
    }

    public class Dungeon : UnityEngine.MonoBehaviour
    {
        public System.Collections.Generic.List<LockedDoor> LockedDoorList => new System.Collections.Generic.List<LockedDoor>();
    }

    public class Tile : UnityEngine.MonoBehaviour
    {
        public Dungeon Dungeon => null;
    }

    public class DoorwayPairFinder { }
}

namespace DunGen.Graph
{
    public class DungeonFlow : UnityEngine.ScriptableObject { }
    public class GraphNode { }
    public class GraphLine { }
}

// SSAOPro - Screen Space Ambient Occlusion image effect
public class SSAOPro : UnityEngine.MonoBehaviour
{
    public float Intensity { get; set; }
    public float Radius { get; set; }
    public float Distance { get; set; }
    public float Bias { get; set; }
    public float LumContribution { get; set; }
    public Color OcclusionColor { get; set; }
    public bool DebugAO { get; set; }
}

// HumanPhyCtrl - custom game physics controller for humanoid characters
public class HumanPhyCtrl : UnityEngine.MonoBehaviour
{
    public bool isGrounded => false;
    public bool grounded { get; set; }
    public bool spineInWater => false;
    public bool headInWater => false;
    public bool feetInWater => false;
    public bool fallGround => false;
    public bool freezeUpdate { get; set; }
    public bool useRopeGun { get; set; }
    public bool m_IsContrler { get; set; }
    public float gravity { get; set; }
    public float forwardGroundAngle => 0f;
    public float moveSpeed { get; set; }
    public float mSpeedTimes { get; set; }
    public float netMoveSpeedScale { get; set; }
    public float m_AirDrag { get; set; }
    public UnityEngine.Vector3 m_SubAcc { get; set; }
    public UnityEngine.LayerMask m_GroundLayer { get; set; }
    public UnityEngine.Vector3 velocity { get; set; }
    public UnityEngine.Vector3 inertiaVelocity { get; set; }
    public UnityEngine.Vector3 desiredMovementDirection { get; set; }
    public UnityEngine.Vector3 currentDesiredMovementDirection { get; set; }
    public UnityEngine.Rigidbody _rigidbody => null;
    public float speed { get; set; }
    public void Move(UnityEngine.Vector3 motion) { }
    public void Jump(float height) { }
    public void ResetSpeed(float speed = 0f) { }
    public void ResetInertiaVelocity() { }
    public void CancelMoveRequest() { }
    public void ApplyMoveRequest(UnityEngine.Vector3 direction, float speed = 0f) { }
    public void ApplyImpact(UnityEngine.Vector3 impulse) { }
}

// SmoothFollower - simple smoothed position follower utility
public class SmoothFollower
{
    public SmoothFollower(float smoothTime) { }
    public UnityEngine.Vector3 Update(UnityEngine.Vector3 targetPosition, float deltaTime) => targetPosition;
    public UnityEngine.Vector3 Update(UnityEngine.Vector3 targetPosition, float deltaTime, bool snap) => targetPosition;
}

// NavmeshController - A* pathfinding NavMesh controller component
public class NavmeshController : UnityEngine.MonoBehaviour
{
    public float speed { get; set; }
    public float acceleration { get; set; }
    public bool canMove { get; set; }
    public void Move(UnityEngine.Vector3 delta) { }
}
