//using UnityEngine;
//using System.Collections;

//public abstract class CSNPCBehave 
//{
//    public virtual Vector3 m_DestPos
//    {
//        get { return Vector3.zero; }
//        set { }
//    }

//    public virtual Quaternion m_DestRot
//    {
//        get { return Quaternion.identity; }
//        set { }
//    }
	

//    public enum EState
//    {
//        Preparing,
//        Pause,
//        Running,
//        Holding,
//        Finished
//    }

//    protected EState m_State;
//    public EState  State 	 { get { return m_State; } }	

//    public enum EPriority
//    {
//        P_1,
//        p_2,
//        p_3,
//        p_4,
//        p_5
//    }

//    protected EPriority m_Priority;
//    public EPriority Pritority	{ get {return m_Priority; } }

//    //protected AiNpcObject m_Npc;
//    protected PersonnelBase m_Npc;

//    public CSNPCBehave()
//    {
//        m_Priority = EPriority.P_1;
//    }

//    public bool IsRunning()
//    {
//        return (m_State == EState.Running) || (m_State == EState.Holding);
//    }

//    public abstract void Init (EPriority priority, CSBehaveParam param, PersonnelBase npc);

//    // Function
//    public abstract void Start(int npc_id);

//    public abstract void Update();

//    public abstract void Break();

//    public abstract void Pause();

//    public abstract void Continue();

//    public abstract void Over();

//    public virtual void OnAffectByPrevBehave(CSNPCBehave behave)
//    {
//        m_DestPos = behave.m_DestPos;
//    }
//}
