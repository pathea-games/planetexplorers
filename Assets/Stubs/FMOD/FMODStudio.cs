// Stub: FMOD and FMOD.Studio namespace types
// Provides EventDescription, EventInstance, ParameterInstance, Bank, Bus, VCA,
// PARAMETER_DESCRIPTION, System, RESULT, GUID, Sound, Channel.

namespace FMOD
{
    /// <summary>
    /// FMOD result codes. Game code checks for OK and error states.
    /// </summary>
    public enum RESULT
    {
        OK,
        ERR_INVALID_HANDLE,
        ERR_CHANNEL_STOLEN,
        ERR_EVENT_ALREADY_LOADED,
        ERR_EVENT_NOTFOUND,
        ERR_FILE_NOT_FOUND,
        ERR_INTERNAL,
        ERR_INVALID_PARAM,
        ERR_MEMORY,
        ERR_UNINITIALIZED
    }

    /// <summary>
    /// FMOD globally unique identifier.
    /// </summary>
    public struct GUID
    {
        public int Data1;
        public int Data2;
        public int Data3;
        public int Data4;
    }

    /// <summary>
    /// Low-level FMOD sound object.
    /// </summary>
    public class Sound
    {
    }

    /// <summary>
    /// Low-level FMOD channel for sound playback control.
    /// </summary>
    public class Channel
    {
    }
}

namespace FMOD.Studio
{
    /// <summary>
    /// Parameter description from an FMOD event. Contains name, min, max, default.
    /// Used by FMODAudioSourceRTE and FMODEventInspector for parameter sliders.
    /// </summary>
    public struct PARAMETER_DESCRIPTION
    {
        public string name;
        public float minimum;
        public float maximum;
        public float defaultvalue;
    }

    /// <summary>
    /// FMOD event description. Provides metadata about an event (3D, oneshot, distance, parameters).
    /// Game code calls getPath, is3D, isOneshot, getMinimumDistance, getMaximumDistance,
    /// getParameterCount, getParameterByIndex, createInstance.
    /// </summary>
    public class EventDescription
    {
        public RESULT getPath(out string path)
        {
            path = "";
            return RESULT.OK;
        }

        public RESULT createInstance(out EventInstance instance)
        {
            instance = new EventInstance();
            return RESULT.OK;
        }

        public RESULT is3D(out bool is3D)
        {
            is3D = false;
            return RESULT.OK;
        }

        public RESULT isOneshot(out bool isOneshot)
        {
            isOneshot = false;
            return RESULT.OK;
        }

        public RESULT getMinimumDistance(out float distance)
        {
            distance = 0f;
            return RESULT.OK;
        }

        public RESULT getMaximumDistance(out float distance)
        {
            distance = 0f;
            return RESULT.OK;
        }

        public RESULT getParameterCount(out int count)
        {
            count = 0;
            return RESULT.OK;
        }

        public RESULT getParameterByIndex(int index, out PARAMETER_DESCRIPTION parameter)
        {
            parameter = new PARAMETER_DESCRIPTION();
            return RESULT.OK;
        }

        public bool isValid()
        {
            return false;
        }
    }

    /// <summary>
    /// Live instance of an FMOD event. Provides playback control and parameter access.
    /// Game code calls start, stop, release, setParameterValue, set3DAttributes,
    /// getDescription, getParameterCount, getParameterByIndex.
    /// </summary>
    public class EventInstance
    {
        public RESULT start()
        {
            return RESULT.OK;
        }

        public RESULT stop(STOP_MODE mode = STOP_MODE.ALLOWFADEOUT)
        {
            return RESULT.OK;
        }

        public RESULT release()
        {
            return RESULT.OK;
        }

        public RESULT setParameterValue(string name, float value)
        {
            return RESULT.OK;
        }

        public RESULT set3DAttributes(ATTRIBUTES_3D attributes)
        {
            return RESULT.OK;
        }

        public RESULT getDescription(out EventDescription description)
        {
            description = new EventDescription();
            return RESULT.OK;
        }

        public RESULT getParameterCount(out int count)
        {
            count = 0;
            return RESULT.OK;
        }

        public RESULT getParameterByIndex(int index, out ParameterInstance instance)
        {
            instance = new ParameterInstance();
            return RESULT.OK;
        }

        public bool isValid()
        {
            return false;
        }
    }

    /// <summary>
    /// Live parameter instance on an EventInstance. Provides value get/set and description.
    /// </summary>
    public class ParameterInstance
    {
        public RESULT getValue(out float value)
        {
            value = 0f;
            return RESULT.OK;
        }

        public RESULT setValue(float value)
        {
            return RESULT.OK;
        }

        public RESULT getDescription(out PARAMETER_DESCRIPTION description)
        {
            description = new PARAMETER_DESCRIPTION();
            return RESULT.OK;
        }
    }

    /// <summary>
    /// FMOD audio bank. Contains events, VCAs, buses, and string data.
    /// Game code calls getPath, getEventList, getVCAList, getBusList, getStringCount, getStringInfo.
    /// </summary>
    public class Bank
    {
        public RESULT getPath(out string path)
        {
            path = "";
            return RESULT.OK;
        }

        public RESULT loadSampleData()
        {
            return RESULT.OK;
        }

        public RESULT getEventList(out EventDescription[] events)
        {
            events = new EventDescription[0];
            return RESULT.OK;
        }

        public RESULT getVCAList(out VCA[] vcas)
        {
            vcas = new VCA[0];
            return RESULT.OK;
        }

        public RESULT getBusList(out Bus[] buses)
        {
            buses = new Bus[0];
            return RESULT.OK;
        }

        public RESULT getStringCount(out int count)
        {
            count = 0;
            return RESULT.OK;
        }

        public RESULT getStringInfo(int index, out GUID id, out string path)
        {
            id = new GUID();
            path = "";
            return RESULT.OK;
        }

        // Overload accepting global::System.Guid (used by DebugCMD.cs which declares 'Guid _guid').
        public RESULT getStringInfo(int index, out global::System.Guid id, out string path)
        {
            id = global::System.Guid.Empty;
            path = "";
            return RESULT.OK;
        }
    }

    /// <summary>
    /// FMOD mixer bus. Provides volume control.
    /// </summary>
    public class Bus
    {
        public RESULT setVolume(float volume)
        {
            return RESULT.OK;
        }

        public RESULT getVolume(out float volume)
        {
            volume = 1f;
            return RESULT.OK;
        }

        public RESULT getPath(out string path)
        {
            path = "";
            return RESULT.OK;
        }
    }

    /// <summary>
    /// FMOD VCA (Voltage Controlled Amplifier). Provides grouped volume control.
    /// </summary>
    public class VCA
    {
        public RESULT setVolume(float volume)
        {
            return RESULT.OK;
        }

        public RESULT getVolume(out float volume)
        {
            volume = 1f;
            return RESULT.OK;
        }

        public RESULT getPath(out string path)
        {
            path = "";
            return RESULT.OK;
        }
    }

    /// <summary>
    /// FMOD Studio system handle. Provides bank listing and event lookup.
    /// Accessed via FMOD_StudioSystem.instance.System.
    /// </summary>
    public class System
    {
        public RESULT getBankList(out Bank[] banks)
        {
            banks = new Bank[0];
            return RESULT.OK;
        }

        public RESULT getEvent(string path, out EventDescription eventDescription)
        {
            eventDescription = new EventDescription();
            return RESULT.OK;
        }

        public RESULT getBus(string path, out Bus bus)
        {
            bus = new Bus();
            return RESULT.OK;
        }

        public RESULT getVCA(string path, out VCA vca)
        {
            vca = new VCA();
            return RESULT.OK;
        }
    }

    /// <summary>
    /// Stop mode for EventInstance.stop().
    /// </summary>
    public enum STOP_MODE
    {
        ALLOWFADEOUT,
        IMMEDIATE
    }

    /// <summary>
    /// 3D attributes for spatial audio positioning.
    /// </summary>
    public struct ATTRIBUTES_3D
    {
        public VECTOR position;
        public VECTOR velocity;
        public VECTOR forward;
        public VECTOR up;
    }

    /// <summary>
    /// FMOD 3D vector (not UnityEngine.Vector3).
    /// </summary>
    public struct VECTOR
    {
        public float x;
        public float y;
        public float z;
    }
}

namespace FMODUnity
{
    /// <summary>
    /// FMODUnity namespace placeholder for any FMODUnity-specific references.
    /// </summary>
    public static class RuntimeManager
    {
    }
}

#if UNITY_EDITOR
/// <summary>
/// FMOD editor extension used by FMODEventInspector for event audition and parameter editing.
/// Provides static methods: GetEventDescription, AuditionEvent, StopEvent, SetEventParameterValue.
/// </summary>
public static class FMODEditorExtension
{
    public static FMOD.Studio.EventDescription GetEventDescription(string id)
    {
        return null;
    }

    public static void AuditionEvent(FMODAsset asset) { }

    public static void StopEvent() { }

    public static void SetEventParameterValue(int index, float value) { }
}
#endif
