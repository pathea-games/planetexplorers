// Stub: FMODAudioSource MonoBehaviour
// ~24 references. Provides audio playback component with path, volume, pitch,
// distance settings, and EventInstance access for FMOD runtime.

using UnityEngine;

/// <summary>
/// FMOD audio source component. Game code accesses path, volume, pitch,
/// minDistance, maxDistance, is3D, audioInst, Play(), Stop(), playOnReset, rteActive.
/// </summary>
public class FMODAudioSource : MonoBehaviour
{
    public string path = "";
    public float volume = 1f;
    public float pitch = 1f;
    public float minDistance = 1f;
    public float maxDistance = 20f;
    public bool is3D;
    public bool playOnReset;
    public static bool rteActive;

    /// <summary>
    /// The active FMOD event instance for this source. Used by FMODAudioSourceRTE
    /// to query parameters and description.
    /// </summary>
    public FMOD.Studio.EventInstance audioInst;

    public string xml { get; set; }

    public void Play() { }

    public void Stop() { }

    public void SetParam(string name, float value) { }

    /// <summary>
    /// Static destroy helper referenced by game code as FMODAudioSource.Destroy(src).
    /// </summary>
    public static void Destroy(FMODAudioSource source)
    {
        if (source != null)
        {
            Object.Destroy(source);
        }
    }
}
