//using UnityEngine;
//using System.Collections;


//public class CSBehaveParam 
//{
//    public Vector3 m_DesPos;
//    public Quaternion m_Rot;
//    public float  m_DesRadius;

//    // Delegate
//    public delegate void NpcParmDel(PersonnelBase npc);
//    public NpcParmDel onBehaveFinished;
//    public NpcParmDel onMeetBlock;
//}

//public class CSSimpleMoveBParam : CSBehaveParam 
//{
//    public float m_DelayTime;
//    public float m_Speed;
//    public bool  m_ForceFixPos = true;
//}

//public class CSPatrolParam : CSBehaveParam
//{
//    public Vector3 m_Center;
//    public float m_Radius;
//    public float m_MinStaySec;
//    public float m_MaxStaySec;
//}

//public class CSSleepBParam : CSBehaveParam
//{

//}

//public class CSWorkBParam : CSBehaveParam
//{
//    public enum EWorkType
//    {
//        Storage,
//        Enhance,
//        Repair,
//        Recycle,
//        Farm_Watering,
//        Farm_Weeding,
//        Factory
//    }

//    public EWorkType m_WorkMode;

//    public float m_DeplayTime = 0;

//    public Vector3 m_LookAtPos;

//    public PersonnelSpace[] m_WorkSpace;

//    public PersonnelSpace m_CurSpace;
	
//}

//public class CSFarmWorkParm : CSBehaveParam
//{
//    public enum EWorkType
//    {
//        None,
//        Watering
//    }

//    public EWorkType m_WorkMode;

//    public float m_DeplayTime = 0;

//    public float m_FinishedDelayTime = 0;
//}