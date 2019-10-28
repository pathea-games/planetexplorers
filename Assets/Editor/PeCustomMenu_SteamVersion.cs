using UnityEditor;
using System.Collections.Generic;

public class PeCustomMenu_SteamVersion : EditorWindow
{
    [MenuItem("PeCustomMenu/SteamVersion/Enable")]
    static void DebugEnable()
    {
        SetEnable("SteamVersion", true);
    }

    [MenuItem("PeCustomMenu/SteamVersion/Enable", true)]
    static bool ValidateDebugEnable()
    {
        var defines = GetDefinesList(EditorUserBuildSettings.selectedBuildTargetGroup);
        return !defines.Contains("SteamVersion");
    }

    [MenuItem("PeCustomMenu/SteamVersion/Disable")]
    static void DebugDisable()
    {
        SetEnable("SteamVersion", false);
    }

    [MenuItem("PeCustomMenu/SteamVersion/Disable", true)]
    static bool ValidateDebugDisable()
    {
        var defines = GetDefinesList(EditorUserBuildSettings.selectedBuildTargetGroup);
        return defines.Contains("SteamVersion");
    }

    static List<string> GetDefinesList(BuildTargetGroup group)
    {
        return new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));
    }

    static void SetEnable(string defineName, bool enable)
    {
        var defines = GetDefinesList(EditorUserBuildSettings.selectedBuildTargetGroup);
        if (enable)
        {
            if (defines.Contains(defineName))
                return;

            defines.Add(defineName);
        }
        else
        {
            if (!defines.Contains(defineName))
                return;

            defines.RemoveAll(iter => Equals(iter, defineName));
        }

        string defineStr = string.Join(";", defines.ToArray());
        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defineStr);
    }
}
