using UnityEngine;
using System.Collections;

public class NewUIText 
{
    public static UITextValue mMenuInventory = new UITextValue(8000507, "Inventory");
    public static UITextValue mMenuCreation = new UITextValue(8000508, "Creation");
    public static UITextValue mMenuReplicator = new UITextValue(8000509, "Replicator");
    public static UITextValue mMenuColony = new UITextValue(8000510, "Colony");
    public static UITextValue mMenuPhone = new UITextValue(8000511, "Phone");
    public static UITextValue mMenuMission = new UITextValue(8000512, "Mission");
    public static UITextValue mMenuCharacter = new UITextValue(8000513, "Character");
    public static UITextValue mMenuFollower = new UITextValue(8000514, "Follower");
    public static UITextValue mMenuInformation = new UITextValue(8000515, "Information");
    public static UITextValue mMenuWorkshop = new UITextValue(8000516, "Workshop");
    public static UITextValue mMenuAdmin = new UITextValue(8000517, "Admin");
    public static UITextValue mMenuStorage = new UITextValue(8000518, "Storage");
    public static UITextValue mMenuScan = new UITextValue(8000519, "Scan");
    public static UITextValue mMenuHelp = new UITextValue(8000520, "Help");
    public static UITextValue mMenuMonoRail = new UITextValue(8000521, "MonoRail");
    public static UITextValue mMenuDiplomacy = new UITextValue(8000522, "Diplomacy");
    public static UITextValue mMenuMessage = new UITextValue(8000523, "Message");
    public static UITextValue mMenuSpeciesWiki = new UITextValue(8000595, "SpeciesWiki");
    public static UITextValue mMenuRadio = new UITextValue(8000971, "MusicPlayer");
    public static UITextValue mMenuOptions = new UITextValue(8000524, "System");
    public static UITextValue mMenuBuild = new UITextValue(8000525, "Build");
    public static UITextValue mMenuSkill = new UITextValue(8000526, "Skill");
    public static UITextValue mMenuOnline = new UITextValue(8000527, "Online");
    public static UITextValue mMenuFriend = new UITextValue(8000528, "Friends");
    public static UITextValue mMenuMall = new UITextValue(8000529, "Store");
}

public class UITextValue
{
	public int mStrId;
	public string mDefaultValue;
	public string GetString()
	{
		string str = PELocalization.GetString(mStrId);
		return (str == "") ? mDefaultValue : str;
	}
	public UITextValue(int _mStrId,string _mDefaultValue)
	{
		mStrId = _mStrId;
		mDefaultValue = _mDefaultValue;
	}
}