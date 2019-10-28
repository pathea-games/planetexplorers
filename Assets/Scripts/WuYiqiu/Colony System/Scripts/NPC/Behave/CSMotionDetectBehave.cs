//using UnityEngine;
//using System.Collections;

//public class CSMotionDetectBehave : CSNPCBehave 
//{
//    private CSBehaveParam m_Param;

//    private CSPersonnel m_Person;

//    const float c_MaxStagnantTime = 3;

//    #region BEHAVE_FUNC

//    // Use this for initialization
//    public override void Init (EPriority priority, CSBehaveParam param, PersonnelBase npc)
//    {
//        m_Priority = priority;
//        m_Param = param;
//        m_Npc = npc;
//    }

//    public override void Start (int npc_id)
//    {
//        //if (NpcManager.Instance == null)
//        //{
//        //    Debug.LogError("There is no NpcManager in the scene.");
//        //    return;
//        //}
		
//        if (m_Param == null)
//        {
//            Debug.LogError("You must initialize this bebave before it start.");
//            return;
//        }

//        if (m_State != EState.Preparing)
//        {
//            Debug.LogError("This behave is already start!");
//            return;
//        }

//        if (m_Npc == null)
//        {
//            Debug.LogError("Can't find the behave npc.");
//            return;
//        }

//        m_Person = m_Npc as CSPersonnel;
//        if (m_Person == null)
//        {
//            Debug.LogError("only CSPerson can use this behave!");
//            return;
//        }

//        m_State = EState.Running;
//    }

//    private float m_CurTime = 0f;
//    private Vector2 prePos;

//    // Update is called once per frame
//    public override void Update () 
//    {
//        //--to do: wait
//        //if (m_Person.m_Npc.dead || m_Person.m_Npc.isAttacking)
//        //    return;

//        //// Record previous position when m_CurTime is Zero
//        //if (((int)m_CurTime) == 0)
//        //    prePos = new Vector2 (m_Person.m_Pos.x, m_Person.m_Pos.z);

//        //// Current position every tick
//        //Vector2 curPos = new Vector2(m_Person.m_Pos.x, m_Person.m_Pos.z);

//        //// Check if this man is Stagnant? 
//        //if (((int)m_CurTime) == c_MaxStagnantTime)
//        //{
//        //    Vector3 newPos = m_Person.m_Pos + Vector3.Normalize( m_Person.m_Npc.forward) * 0.5f + Vector3.up * 100f;
//        //    RaycastHit rch;
//        //    if ( Physics.Raycast(newPos, Vector3.down, out rch, 100, 1 << Pathea.Layer.VFVoxelTerrain ) )
//        //    {
//        //        m_Person.m_Pos = rch.point;
//        //    }
//        //    m_CurTime = 0;
//        //}

//        //m_CurTime += Time.deltaTime; 
//    }

//    public override void Break()
//    {

//    }

//    public override void Pause ()
//    {
//        m_State = EState.Pause;
//    }

//    public override void Continue ()
//    {
//        m_State = EState.Running;
//    }

//    public override void Over ()
//    {

//    }

//    #endregion
//}
