// Global-scope stubs for types referenced without namespace qualification in game code.
// These exist solely so the Roslyn analyzer project compiles.
// THESE TYPES MUST NOT BE USED IN RUNTIME CODE.

using System;
using UnityEngine;

// OnPathDelegate is defined in Pathfinding namespace but referenced in some game files
// (e.g. NpcMgr.cs inside namespace Pathea) without a `using Pathfinding;` directive.
public delegate void OnPathDelegate(Pathfinding.Path path);

// ProceduralGridMover is in Pathfinding namespace but used in PathfindingUpdate.cs
// without a using directive.
public class ProceduralGridMover : UnityEngine.MonoBehaviour
{
    public UnityEngine.Transform target { get; set; }
    public float updateDistance { get; set; }
}

// AstarPath is in Pathfinding namespace but used in some files without `using Pathfinding;`
public class AstarPath : UnityEngine.MonoBehaviour
{
    public static AstarPath active { get; set; }
    public Pathfinding.NavGraph[] graphs;
    public Pathfinding.AstarData astarData => null;
    public static bool SkipOptScanOnStartUp { get; set; }
    public static void StartPath(Pathfinding.Path path) { }
    public Pathfinding.NNInfo GetNearest(UnityEngine.Vector3 position) => default;
    public Pathfinding.NNInfo GetNearest(UnityEngine.Vector3 position, Pathfinding.NNConstraint constraint) => default;
    public void Scan() { }
    public void Scan(int graphMask) { }
    public void UpdateGraphs(Pathfinding.GraphUpdateObject guo) { }
    public void UpdateGraphs(UnityEngine.Bounds bounds) { }
}

// Util is a UnitySteer helper class used in AiCharacterMotor scripts without namespace qualification.
public static class Util
{
    public static UnityEngine.Vector3 ConstantSlerp(UnityEngine.Vector3 from, UnityEngine.Vector3 to, float maxAngle)
        => UnityEngine.Vector3.Slerp(from, to, maxAngle);
    public static UnityEngine.Quaternion ConstantSlerp(UnityEngine.Quaternion from, UnityEngine.Quaternion to, float maxAngle)
        => UnityEngine.Quaternion.Slerp(from, to, maxAngle);
    public static UnityEngine.Vector3 ProjectOntoPlane(UnityEngine.Vector3 v, UnityEngine.Vector3 planeNormal)
        => v - UnityEngine.Vector3.Dot(v, planeNormal) * planeNormal;
    public static float SquaredDistanceFromPoint(UnityEngine.Vector3 a, UnityEngine.Vector3 b)
        => (a - b).sqrMagnitude;
    public static float DistanceFromPoint(UnityEngine.Vector3 a, UnityEngine.Vector3 b)
        => (a - b).magnitude;
    public static float LimitMaxDeviationAngle(float angle, float cosineOfConeHalfAngle, float unitUpDir)
        => angle;
}

// OnReqsFinished is a delegate used in pathfinding/quest code
public delegate void OnReqsFinished();

// ProtoTypeId holds integer constants for colony/entity prototype IDs.
// The game source defines this in a file not present in the repo.
public static class ProtoTypeId
{
    public const int ASSEMBLY_CORE = 1;
    public const int PPCoal = 2;
    public const int STORAGE = 3;
    public const int REPAIR_MACHINE = 4;
    public const int DWELLING_BED = 5;
    public const int FARM = 6;
    public const int ENHANCE_MACHINE = 7;
    public const int RECYCLE_MACHINE = 8;
    public const int FACTORY_REPLICATOR = 9;
    public const int PROCESSING = 10;
    public const int TRADE_POST = 11;
    public const int TRAINING_CENTER = 12;
    public const int MEDICAL_CHECK = 13;
    public const int MEDICAL_TREAT = 14;
    public const int MEDICAL_TENT = 15;
    public const int PPFusion = 16;
    public const int HERBAL_JUICE = 101;
    public const int NUTS = 102;
    public const int COMFORT_INJECTION_1 = 103;
    public const int ARROW = 104;
    public const int BULLET = 105;
    public const int BATTERY = 106;
    public const int HEAT_PACK = 107;
    public const int CHARCOAL = 108;
    public const int TORCH = 109;
    public const int FLOUR = 110;
    public const int WATER = 111;
    public const int INSECTICIDE = 112;
}

// ProcessingConst holds processing-related constants used in RandomItemObj
public static class ProcessingConst
{
    public const int NONE = 0;
    public const int NEED_HUMAN = 1;
    public const int TASK_NUM = 5;
    public const int OBJ_MAX = 10;
    public const int WORKER_AMOUNT_MAX = 10;
    public const int NotHaveCollect = -1;
    public const int INFORM_FINISH_TO_STORAGE = 0;
    public const int INFORM_FINISH_TO_RANDOMITEM = 1;
    public const string RESULT_MODEL_PATH = "Prefabs/RandomItem/result_model";
}

// FecesModelPath holds path constants for animal feces models
public static class FecesModelPath
{
    public const string PATH_01 = "Prefabs/Feces/Feces_01";
    public const string PATH_02 = "Prefabs/Feces/Feces_02";
    public const string PATH_03 = "Prefabs/Feces/Feces_03";
}

/// IsoTags: isotope/item tag string constants used as search tags in workshop/dungeon systems.
// Not present in repo source; referenced without namespace qualifier.
public static class IsoTags
{
    public const string Creation = "Creation";
    public const string Equipment = "Equipment";
    public const string Sword = "Sword";
    public const string Axe = "Axe";
    public const string Bow = "Bow";
    public const string Gun = "Gun";
    public const string Shield = "Shield";
    public const string Armor = "Armor";
    public const string Helmet = "Helmet";
    public const string Shoes = "Shoes";
    public const string Gloves = "Gloves";
    public const string Accessories = "Accessories";
    public const string Food = "Food";
    public const string Medicine = "Medicine";
    public const string Material = "Material";
    public const string Bullet = "Bullet";
    public const string Arrow = "Arrow";
    public const string Special = "Special";
    public const string Aircraft = "Aircraft";
    public const string AITurret = "AITurret";
    public const string ArmAndLeg = "ArmAndLeg";
    public const string Body = "Body";
    public const string Carrier = "Carrier";
    public const string Decoration = "Decoration";
    public const string Head = "Head";
    public const string HeadAndFoot = "HeadAndFoot";
    public const string ObjectItem = "ObjectItem";
    public const string Robot = "Robot";
    public const string Ship = "Ship";
    public const string Vehicle = "Vehicle";
}

// ColonyNameID: colony building prototype name ID constants.
public static class ColonyNameID
{
    public const int ASSEMBLY_CORE = 1;
    public const int ASSEMBLY = 1;
    public const int PROCESSING = 10;
    public const int PROCESSING_FACILITY = 10;
    public const int TRADE_POST = 11;
    public const int TRAINING_CENTER = 12;
    public const int MEDICAL_CHECK = 13;
    public const int MEDICAL_DETECTOR = 13;
    public const int MEDICAL_TREAT = 14;
    public const int MEDICAL_LAB = 14;
    public const int MEDICAL_TENT = 15;
    public const int QUARANTINE_TENT = 15;
    public const int FACTORY_REPLICATOR = 9;
    public const int FACTORY = 9;
    public const int STORAGE = 3;
    public const int FARM = 6;
    public const int DWELLING_BED = 5;
    public const int DWELLING = 5;
    public const int ENHANCE_MACHINE = 7;
    public const int ENHANCE = 7;
    public const int RECYCLE_MACHINE = 8;
    public const int RECYCLE = 8;
    public const int REPAIR_MACHINE = 4;
    public const int REPAIR = 4;
    public const int PPCOAL = 2;
    public const int PPFUSION = 16;
    public const int ENGINEER = 17;
}

// ColonyConst: colony system balance constants.
public static class ColonyConst
{
    public const float TRADE_POST_CHARGE_RATE = 0.1f;
    public const int MAX_COLONY_NPC = 50;
    public const int MAX_COLONY_STORAGE = 100;
    public const float COLONY_UPDATE_INTERVAL = 1.0f;
    public const int START_MONEY = 1000;
    public const int FACTORY_COMPOUND_GRID_COUNT = 9;
}

// ColonyMessage: colony event message IDs.
public static class ColonyMessage
{
    public const int ADD_NPC = 1;
    public const int REMOVE_NPC = 2;
    public const int ADD_BUILDING = 3;
    public const int REMOVE_BUILDING = 4;
    public const int RESOURCE_CHANGE = 5;
    public const int CANNOT_WORK_None = 0;
    public const int CANNOT_WORK_HpLow = 6;
    public const int CANNOT_WORK_IsHunger = 7;
    public const int CANNOT_WORK_IsInSleepTime = 8;
    public const int CANNOT_WORK_IsInDinnerTime = 9;
    public const int CANNOT_WORK_IsNeedMedicine = 10;
    public const int CANNOT_WORK_IsUncomfortable = 11;
    public const int CANNOT_WORK_HasAnyQuest = 12;
    public const int CANNOT_ENHANCE_ITEM = 13;
    public const int CANNOT_ENHANCE_MORE = 14;
    public const int OCCUPATION_NPC_TOO_MANY = 15;
}

// ColonyNoMgrMachine: colony machine types without dedicated manager.
public static class ColonyNoMgrMachine
{
    public const int NONE = 0;
    public const int LIGHTING = 1;
    public const int DECORATION = 2;
    public const int DOODAD_ID_REPAIR = 100;
    public const int DOODAD_ID_SOLARPOWER = 101;
}

// ColonyStatusWarning / ColonyErrorMsg: colony status warning/error IDs.
public static class ColonyStatusWarning
{
    public const int NONE = 0;
    public const int LOW_POWER = 1;
    public const int LOW_FOOD = 2;
    public const int DURABILITY_LOW = 3;
    public const int PPCOAL_RUNNING_LOW = 4;
    public const int PPFUSION_RUNNING_LOW = 5;
}
public static class ColonyErrorMsg
{
    public const int NONE = 0;
    public const int NO_POWER = 1;
    public const int NO_FOOD = 2;
    public const int TOO_CLOSE_TO_NATIVE_CAMP0 = 3;
    public const int TOO_CLOSE_TO_NATIVE_CAMP1 = 4;
    public const int TOO_CLOSE_TO_NATIVE_CAMP2 = 5;
}

// NGUIMath: NGUI math utility class.
public static class NGUIMath
{
    public static UnityEngine.Bounds CalculateRelativeWidgetBounds(UnityEngine.Transform trans) => default;
    public static UnityEngine.Bounds CalculateRelativeWidgetBounds(UnityEngine.Transform relativeTo, UnityEngine.Transform content) => default;
    public static UnityEngine.Vector3 ApplyHalfPixelOffset(UnityEngine.Vector3 pos, UnityEngine.Vector3 scale) => pos;
    public static UnityEngine.Vector2 ScreenToPixels(UnityEngine.Vector2 pos, UnityEngine.Transform relativeTo) => pos;
    public static float WrapAngle(float angle) => angle;
    public static float SpringDamp(float from, float to, ref float velocity, float deltaTime) => to;
    public static float SpringLerp(float from, float to, float strength, float deltaTime) => to;
    public static int ColorToInt(UnityEngine.Color color) => 0;
    public static string DecimalToHex(int value) => value.ToString("X");
}

// AstarMath: A* math utilities.
public static class AstarMath
{
    public static float Magnitude(Pathfinding.Int3 a, Pathfinding.Int3 b) => 0f;
    public static float MagnitudeXZ(Pathfinding.Int3 a, Pathfinding.Int3 b) => 0f;
    public static float MapValue(float value, float fromMin, float fromMax, float toMin, float toMax) => toMin;
    public static float Clamp01(float value) => UnityEngine.Mathf.Clamp01(value);
    public static float NearestPointFactor(UnityEngine.Vector3 lineStart, UnityEngine.Vector3 lineEnd, UnityEngine.Vector3 point) => 0f;
}

// AnimationOrTween: NGUI animation/tween trigger state.
public static class AnimationOrTween
{
    public enum Trigger { OnActivate, OnClick, OnHover, OnPress, OnDragOver, OnDragOut, OnDoubleClick, OnActivateTrue, OnActivateFalse, OnHoverTrue, OnHoverFalse, OnPressTrue, OnPressFalse }
    public enum Direction { Reverse = -1, Toggle = 0, Forward = 1 }
    public enum EnableCondition { DoNothing, EnableThenPlay, IgnoreIfActive }
    public enum DisableCondition { DoNotDisable, DisableAfterForward, DisableAfterReverse }
}

// UICursor: NGUI cursor display helper.
public static class UICursor
{
    public static void Set(UnityEngine.Texture2D tex) { }
    // Set(atlas, spriteName): NGUI atlas-based cursor sprite.
    public static void Set(UIAtlas atlas, string spriteName) { }
    public static void Clear() { }
}

// SleepTime: constants for time-of-day sleeping/rest.
public static class SleepTime
{
    public const float SLEEP_START = 22.0f;
    public const float SLEEP_END = 6.0f;
    public const float AWAKE_TIRED = 18.0f;
    public const float MinHours = 6.0f;
    public const float MaxHours = 22.0f;
    public const float NormalHours = 8.0f;
}

// PlantConst: plant growth/farming constants.
public static class PlantConst
{
    public const int MAX_GROW_LEVEL = 5;
    public const float GROW_INTERVAL = 3600.0f;
    public const int WATER_NEED = 1;
    public const int DIRTY_TYPE0 = 0;
    public const int DIRTY_TYPE1 = 1;
}

// RecycleConst: recycling machine constants.
public static class RecycleConst
{
    public const int MAX_INPUT = 10;
    public const float RECYCLE_RATE = 0.5f;
    public const int INFORM_FINISH_TO_STORAGE = 0;
    public const int INFORM_FINISH_TO_RANDOMITEM = 1;
    public const int INFORM_FINISH_TO_PACKAGE = 2;
}

// RandomItemType: categories for randomly-generated item drops.
public static class RandomItemType
{
    public const int NONE = 0;
    public const int WEAPON = 1;
    public const int ARMOR = 2;
    public const int MATERIAL = 3;
    public const int FOOD = 4;
    public const int TOOL = 5;
    public const int EQUIPMENT = 6;
    public const int CONSUMABLE = 7;
    public const int SCRIPT = 8;
}

// NovaEnvironmentProxy: minimal proxy for PeEnv.Nova (Nova Environment plugin).
public class NovaEnvironmentProxy
{
    // WetCoef: 0..1 float controlling wetness/rain intensity
    public float WetCoef { get; set; }
}

// PeEnv: global environment state constants and methods.
public static class PeEnv
{
    public const int NORMAL = 0;
    public const int RAIN = 1;
    public const int SNOW = 2;
    public const int STORM = 3;
    public const int THUNDER = 4;
    public const int FOG = 5;
    public static float globalWindStrength { get; set; }
    public static float globalWindDirX { get; set; }
    public static float globalWindDirZ { get; set; }
    public static bool isRain { get; set; }
    public static void CanRain(bool enable) { }
    // Nova Environment plugin proxy - provides WetCoef etc.
    public static NovaEnvironmentProxy Nova { get; set; }
    public static void Init() { }
    public static void Update() { }
    // SetControlRain takes a float intensity (0..1) per WeatherConfig usage
    public static void SetControlRain(float intensity) { }
}

// BoxMapTypeInt: integer type IDs for box/map categorisation.
public static class BoxMapTypeInt
{
    public const int NONE = 0;
    public const int COMMON = 1;
    public const int RARE = 2;
    public const int BOSS = 3;
    public const int GRASSLAND = 4;
    public const int DESERT = 5;
    public const int IN_CAVE = 6;
    public const int IN_WATER = 7;
    public const int NEAR_CAMP = 8;
    public const int NEAR_TOWN = 9;
    public const int REDSTONE = 10;
}

// TestPEEntityCamCtrl: test camera controller class (singleton).
public class TestPEEntityCamCtrl : UnityEngine.MonoBehaviour
{
    public static TestPEEntityCamCtrl Instance { get; private set; }
    public void SetTarget(UnityEngine.Transform target) { }
    public UnityEngine.Transform target { get; set; }
    public bool enabled { get; set; }
    public UnityEngine.Camera GetCam() => null;
    public void SetCamMode(int mode) { }
    public void SetCamMode(string modeName) { }
    // 3-arg overload: MainPlayerCmpt.cs calls SetCamMode(Transform, Transform, string).
    public void SetCamMode(UnityEngine.Transform camTarget, UnityEngine.Transform lookAt, string modeName) { }
}

// AutoCycleTips: UI auto-cycling tip display controller.
public class AutoCycleTips : UnityEngine.MonoBehaviour
{
    public const int STORAGE_FULL = 0;
    public const int MEDICINE_SUPPLY = 1;
    public const int PROCESS_FOR_STORAGE = 2;
    public const int PROCESS_FOR_RESOURCE = 3;
    public const int REPLICATE_FOR = 4;
    public const int FACTORY_TO_STORAGE = 5;
    public float cycleInterval { get; set; }
    public void SetTips(System.Collections.Generic.List<string> tips) { }
}

// AllyConstants: alliance system integer constants.
public static class AllyConstants
{
    public const int MAX_ALLIES = 5;
    public const int ALLY_FOLLOW_RANGE = 30;
    public const int ALLY_ATTACK_RANGE = 20;
    public const int EnemyNpcIdAddNum = 1000;
    public const int EnemyNpcCampId = 1;
    public const int EnemyNpcDamageId = 1;
    public const int PajaCampId = 2;
    public const int PajaDamageId = 2;
    public const int PujaCampId = 3;
    public const int PujaDamageId = 3;
}

// AdventureAlly: companion/ally AI controller.
public class AdventureAlly : UnityEngine.MonoBehaviour
{
    public const int DefaultPlayerId = 0;
    public const int PajaStartPlayerId = 100;
    public const int PujaStartPlayerId = 200;
    public const int EnemyNpcStartPlayerId = 300;
    public int allyId { get; set; }
    public bool isFollowing { get; set; }
    public void Follow(UnityEngine.Transform target) { }
    public void StopFollow() { }
    public void Attack(UnityEngine.Transform target) { }
}

// AllyColor: color constants for ally UI display.
public static class AllyColor
{
    public static UnityEngine.Color ALLY = UnityEngine.Color.green;
    public static UnityEngine.Color ENEMY = UnityEngine.Color.red;
    public static UnityEngine.Color NEUTRAL = UnityEngine.Color.yellow;
    public static UnityEngine.Color[] AllianceCols = new UnityEngine.Color[0];
}

// AllyIcon: icon constants for ally display.
// HummanIcon/PajaIcon/PujaIcon are string arrays — indexed in AllianceItem_N.cs.
public static class AllyIcon
{
    public const string FOLLOW = "icon_follow";
    public const string ATTACK = "icon_attack";
    public const string STAY = "icon_stay";
    public static readonly string[] HummanIcon = { "icon_human" };
    public static readonly string[] PajaIcon = { "icon_paja" };
    public static readonly string[] PujaIcon = { "icon_puja" };
}

// MissionMapLabelColor: color constants for mission map labels.
public static class MissionMapLabelColor
{
    public static UnityEngine.Color MAIN = UnityEngine.Color.yellow;
    public static UnityEngine.Color SIDE = UnityEngine.Color.white;
    public static UnityEngine.Color COMPLETE = UnityEngine.Color.grey;
    public static UnityEngine.Color ACTIVE = UnityEngine.Color.cyan;
    public static UnityEngine.Color INACTIVE = UnityEngine.Color.gray;
    public static UnityEngine.Color MissionTargetCol = UnityEngine.Color.cyan;
    public static UnityEngine.Color UnFinishedCol = UnityEngine.Color.white;
    public static UnityEngine.Color MainLineCol = UnityEngine.Color.yellow;
    public static UnityEngine.Color SideLineCol = UnityEngine.Color.white;
}

// VArtifactTownConstant / ArtifactTownConst: artifact town system constants.
public static class VArtifactTownConstant
{
    public const int MAX_LEVEL = 10;
    public const float UPGRADE_COST = 1000f;
    public const int NATIVE_TOWER_BUILDING_ID = 100;
}
public static class ArtifactTownConst
{
    public const int NONE = 0;
    public const int LEVEL_1 = 1;
    public const int DEPTH_IN_TERRAIN = 2;
}

// CSTrainMsgID: training center message IDs.
public static class CSTrainMsgID
{
    public const int START = 1;
    public const int STOP = 2;
    public const int FINISH = 3;
    public const int START_TRAINING = 1;
    public const int SAME_OR_MORE_SKILL = 10;
}

// DungeonConstants / DunItemId / DungenMessage / DungeonMonster: dungeon system constants.
public static class DungeonConstants
{
    public const int MAX_ROOMS = 20;
    public const int MAX_MONSTERS = 50;
    public const int MAX_ITEMS = 30;
    public const int TASK_LEVEL_START = 5;
}
public static class DunItemId
{
    public const int NONE = 0;
    public const int KEY = 1;
    public const int TREASURE = 2;
    public const int UNFINISHED_ISO = 3;
}
public static class DungenMessage
{
    public const int ENTER = 1;
    public const int EXIT = 2;
    public const int COMPLETE = 3;
    public const int ENTER_DUNGEN = 1;
    public const int EXIT_DUNGEN = 2;
    public const int TASK_ENTER_DUNGEN = 4;
}
public static class DungeonMonster
{
    public const int COMMON = 1;
    public const int ELITE = 2;
    public const int BOSS = 3;
    public const int CAMP_ID = 10;
    public const int DAMAGE_ID = 10;
}

// HumanSoundData: human character sound data holder.
public class HumanSoundData : UnityEngine.MonoBehaviour
{
    public string footstepPath { get; set; }
    public string hurtPath { get; set; }
    public string deathPath { get; set; }
    public static void LoadData() { }
    // GetSoundID returns int[] (array) — PEAbnormalEff.cs assigns to int[].
    public static int[] GetSoundID(string category, int type) => System.Array.Empty<int>();
    public static int[] GetSoundID(string category, string name) => System.Array.Empty<int>();
    // PEAbnormalEff.cs calls GetSoundID(int audioID, int sex).
    public static int[] GetSoundID(int audioID, int sex) => System.Array.Empty<int>();
}

// EffectId: visual effect ID constants.
public static class EffectId
{
    public const int NONE = 0;
    public const int HIT = 1;
    public const int BLOOD = 2;
    public const int EXPLOSION = 3;
    public const int FIRE = 4;
    public const int PICK_FECES = 50;
}

// ErrorMessage: error message string constants.
public static class ErrorMessage
{
    public const string NONE = "";
    public const string NETWORK_ERROR = "Network Error";
    public const string FILE_NOT_FOUND = "File Not Found";
    public const int NAME_HAS_EXISTED = 8001;
}

// ItemTabIndex: index constants for item UI tabs.
public static class ItemTabIndex
{
    public const int ALL = 0;
    public const int WEAPON = 1;
    public const int ARMOR = 2;
    public const int MATERIAL = 3;
    public const int FOOD = 4;
    public const int EQUIPMENT = 5;
    public const int RESOURCE = 6;
}

// TradeCampId: trading camp prototype ID constants.
public static class TradeCampId
{
    public const int NONE = 0;
    public const int CAMP_A = 1;
    public const int CAMP_B = 2;
    public const int MISSION_NATIVE = 10;
}

// IntroRunner: intro/cinematic sequence controller.
public class IntroRunner : UnityEngine.MonoBehaviour
{
    public void Play() { }
    public void Stop() { }
    public bool isPlaying => false;
    // movieEnd: static callback assigned a lambda by UIPlayerBuildCtrl.cs;
    // TutorialExit.cs tests it as a bool-like (null/non-null check).
    public static System.Action movieEnd { get; set; }
}

// DllCheck: DLL availability checker utility.
public static class DllCheck
{
    public static bool CheckAll() => true;
    public static bool CheckFMOD() => true;
    public static bool CheckSteam() => true;
    public static bool Test() => true;
}

// Packsize: package/bundle size constants.
public static class Packsize
{
    public const int SMALL = 1;
    public const int MEDIUM = 5;
    public const int LARGE = 10;
    public const int HUGE = 50;
    public static bool Test() => true;
}

// ForceConstant: physics force constants used in ragdoll/hit calculations.
public static class ForceConstant
{
    public const float LIGHT = 10f;
    public const float MEDIUM = 50f;
    public const float HEAVY = 200f;
    public const float EXPLOSION = 500f;
    public const float PLAYER = 100f;
}

// UnityUtil: Unity helper utility class.
public static class UnityUtil
{
    public static void SetActive(UnityEngine.GameObject go, bool active)
    {
        if (go != null) go.SetActive(active);
    }
    public static T GetOrAddComponent<T>(UnityEngine.GameObject go) where T : UnityEngine.Component
        => go != null ? go.GetComponent<T>() ?? go.AddComponent<T>() : null;
    public static void Destroy(UnityEngine.Object obj) { UnityEngine.Object.Destroy(obj); }
    public static void DestroyImmediate(UnityEngine.Object obj) { UnityEngine.Object.DestroyImmediate(obj); }
}

// LegController and LegInfo: proprietary spider/creature leg IK system.
// Used by LoadIKAnimData for 4-legged creature IK setup.
public class LegInfo
{
    public UnityEngine.Transform hip;
    public UnityEngine.Transform ankle;
    public UnityEngine.Transform toe;
    public float footLength;
    public float footWidth;
    public UnityEngine.Vector2 footOffset;
}

public class LegController : UnityEngine.MonoBehaviour
{
    public float groundPlaneHeight { get; set; }
    public UnityEngine.AnimationClip groundedPose { get; set; }
    public UnityEngine.Transform rootBone { get; set; }
    public LegInfo[] legs { get; set; }
}

// LegAnimator: procedural leg animation MonoBehaviour from the same plugin.
public class LegAnimator : UnityEngine.MonoBehaviour { }

// AlignmentTracker: utility MonoBehaviour for terrain alignment.
// velocitySmoothed: smoothed velocity used by AiNormalCharacterMotor and PEMotorNormal.
public class AlignmentTracker : UnityEngine.MonoBehaviour
{
    public UnityEngine.Vector3 velocitySmoothed { get; set; }
}
