using UnityEditor;
using System.Collections.Generic;

public class PeCustomMenu_LocalTest : EditorWindow
{
    [MenuItem("PeCustomMenu/LocalTest/Enable")]
    static void DebugEnable()
    {
        SetEnable("LOCALTEST", true);
    }

    [MenuItem("PeCustomMenu/LocalTest/Enable", true)]
    static bool ValidateDebugEnable()
    {
        var defines = GetDefinesList(EditorUserBuildSettings.selectedBuildTargetGroup);
        return !defines.Contains("LOCALTEST");
    }

    [MenuItem("PeCustomMenu/LocalTest/Disable")]
    static void DebugDisable()
    {
        SetEnable("LOCALTEST", false);
    }

    [MenuItem("PeCustomMenu/LocalTest/Disable", true)]
    static bool ValidateDebugDisable()
    {
        var defines = GetDefinesList(EditorUserBuildSettings.selectedBuildTargetGroup);
        return defines.Contains("LOCALTEST");
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
