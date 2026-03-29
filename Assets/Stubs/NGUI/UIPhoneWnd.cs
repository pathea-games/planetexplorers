// Stub for UIPhoneWnd and UIHelpCtrl.
// The actual UIPhoneWnd.cs depends on RadioManager which uses AudioType.UNKNOWN
// (not present in the Unity 6 DLL). This minimal stub keeps dependent files compilable.
// THESE TYPES MUST NOT BE USED IN RUNTIME CODE.
using UnityEngine;

public class UIPhoneWnd : UIBaseWidget
{
    public enum PageSelect
    {
        Page_Scan,
        Page_Help,
        Page_Rail,
        Page_Diplomacy,
        Page_Message,
        Page_MonsterHandbook,
        Page_Radio,
    }

    // mUIHelp: help sub-panel accessed by UINPCTalk.cs.
    public UIHelpCtrl mUIHelp;

    public override void Show() { }
    public void Show(PageSelect page) { }
    public bool OpenRadio { get; set; }
    public bool InitRadio { get; set; }
    public object InitRadioData() => null;
    public bool CheckOpenRadio() => false;
}

public class UIHelpCtrl : UIBaseWidget
{
    public void ChangeSelect(int id) { }
}

// TutorialData: tutorial/help content data. Defined in UIHelpCtrl.cs (excluded).
// Stub provides constants and methods referenced by PeArchiveMgrs.cs, UINPCTalk.cs, etc.
public class TutorialData
{
    public const int BuildingId = 1;
    public const int Building_1Id = 16;
    public const int PlantSolarId = 9;
    public const int RepairMachineId = 8;
    public const int GetOnVehicle = 15;
    public const int ColonyID0 = 20;
    public const int ColonyID1 = 4;
    public const int ColonyID2 = 5;
    public const int ColonyID3 = 6;
    public const int ColonyID4 = 7;
    public const int ColonyID5 = 10;
    public const int ColonyID6 = 17;
    public const int ColonyID7 = 18;
    public const int ColonyID8 = 19;

    public static void Deserialize(byte[] data) { }
    public static byte[] Serialize() => System.Array.Empty<byte>();
    public static void Clear() { }
    public static void LoadData() { }
    public static int[] SkillIDs = new int[] { 21, 22, 23, 24, 25 };
    // Returns bool: code uses if(!AddActiveTutorialID(...)) pattern.
    // Two-arg overload also used by UINPCTalk and CSUI_MainWndCtrl.
    public static bool AddActiveTutorialID(int id) => false;
    public static bool AddActiveTutorialID(int id, bool immediate) => false;
}
