using UnityEngine;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TameMonsterConfig : ScriptableObject
{
    [Serializable]
    public struct FloatRange
    {
        public float Min;
        public float Max;

        public FloatRange(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }

    [Serializable]
    public struct DropIfRangeInfo
    {
        [Header("角度半径")]
        public float AngleRadius;
        [Header("UI显示颜色")]
        public Color32 Color;
        [Header("允许停留时间")]
        public float AllowStayTime;
        [Header("超出停留时间掉落机率(最大为1)")]
        public float DropOutRandomPercent;
    }

    [Serializable]
    public struct SingleActionSeedInfo
    {
        [Header("行为")]
        public AnalogMonsterStruggle.MonsterAction Action;
        [Header("行为的机率百分比")]
        public float RandomPercent;
        [Header("行为的随机时间范围")]
        public FloatRange TimeRandomRange;
        [Header("走机率百分比")]
        public float WalkRandomPercent;
        [Header("跑机率百分比")]
        public float RunRandomPercent;
        [Header("冲刺机率百分比")]
        public float SprintRandomPercent;
    }

    static TameMonsterConfig _instance;
    public static TameMonsterConfig instance
    {
        get
        {
            if (!_instance)
            {
                _instance = Resources.Load<TameMonsterConfig>("TameMonsterConfig");
            }
            return _instance;
        }
    }

    [Header("__________玩家控制相关参数__________")]
    [Header("最大旋转")]
    public float MaxRotate = 40f;
    [Header("最大回力气比")]
    public float PositiveFactor = 0.25f;
    [Header("调整重心速度")]
    public float CtrlRotateSpeed = 1f;

    [Header("__________怪物相关参数__________")]
    [Header("怪物移动影响玩家的系数")]
    public float MonsterMoveFactor = 1f;
    [Header("怪物旋转影响玩家的系数")]
    public float MonsterRotateFactor = 0.9f;
    [Header("怪物挣扎行为总数随机范围")]
    public FloatRange MonsterStruggleActionSizeRange;
    [Header("怪物体积范围范围")]
    public FloatRange MonsterBulkRange;
    [Header("怪物行为列表")]
    public List<SingleActionSeedInfo> ActionList;

    [Header("__________驯服检测相关__________")]
    [Header("玩家击落的各范围及概率")]
    public List<DropIfRangeInfo> IfRangeList;

    [Header("__________IK相关__________")]
    [Header("使用IK后骑点的坐标校正偏移")]
    public Vector3 IkRideOffset = new Vector3(0f, 0.45f, 0f);
    [Header("坐骑IK运算迭代次数")]
    public int IKIterationSize = 3;


#if UNITY_EDITOR

    [MenuItem("Assets/Create/TameMonsterConfig")]
    static void CreateConfig()
    {
        AssetDatabase.CreateAsset(CreateInstance<TameMonsterConfig>(), "Assets/Scripts/MountsMonster/Resources/TameMonsterConfig.asset");
    }
#endif
}


