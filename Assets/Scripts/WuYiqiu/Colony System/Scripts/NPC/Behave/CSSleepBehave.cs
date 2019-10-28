//using UnityEngine;
//using System.Collections;

//public class CSSleepBehave : CSNPCBehave 
//{
//    public override  Vector3 m_DestPos
//    {
//        get { return m_Param.m_DesPos; }
//        set {
//            m_Param.m_DesPos = value; 
//            m_bRestPos = true;
//        }
//    }

//    public bool m_bRestPos;

//    public override Quaternion m_DestRot {
//        get {
//            return m_Param.m_Rot;
//        }
//        set {
//            m_bRestPos = true;
//            m_Param.m_Rot = value;
//        }
//    }

//    private bool bDoFinished = false;
//    private CSSleepBParam m_Param;

//    #region BEHAVE_FUNC

//    public override void Init (EPriority priority, CSBehaveParam param, PersonnelBase npc)
//    {
//        m_Priority = priority;
//        m_Param = param as CSSleepBParam;
//        m_Npc = npc;
//        m_bRestPos = true;
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
//            Debug.Log("Can't find the behave npc.");
//            return;
//        }

//        m_Npc.m_Pos = m_Param.m_DesPos;
//        m_Npc.m_Rot = m_Param.m_Rot;

//        m_State = EState.Running;
//    }

//    public override void Update ()
//    {
//        // Reset Pos if need
//        if (m_bRestPos)
//        {
//            // Find the pos
//            SetTrans ();
//            // Meet the block
//            if ((m_Npc.m_Pos - m_DestPos).sqrMagnitude > 1f)
//            {
//                if (m_Param.onMeetBlock != null)
//                    m_Param.onMeetBlock(m_Npc);
				
//                m_State = EState.Finished;
//                m_bRestPos = false;
//                return;
//            }
//            m_bRestPos = false;
//        }

////		if (Time.frameCount % 16 == 0)
////		{
////			SetTrans ();
////		}

//        if (m_State == EState.Running)
//        {
//            m_Npc.Sleep(true);

//            if (m_Param.onBehaveFinished != null)
//                m_Param.onBehaveFinished(m_Npc);
//        }

//        m_State = EState.Holding;
//    }

//    public override void Break()
//    {
//        m_Npc.Sleep(false);
//    }

//    public override void Pause ()
//    {
//        m_State = EState.Pause;
//        m_Npc.Sleep(false);
//    }
	
//    public override void Continue ()
//    {
//        m_State = EState.Running;
//    }

//    public override void Over ()
//    {
//        m_Npc.Sleep(false);
//    }

//    #endregion

//    private void SetTrans () 
//    {
//        Vector3 pos = new Vector3(m_DestPos.x, m_DestPos.y + 0.5f, m_DestPos.z);
//        Vector3 prePos = Vector3.zero;
//        RaycastHit rch = new RaycastHit();
//        m_Npc.m_Pos = m_DestPos;
//        m_Npc.m_Rot = m_DestRot;
//        for (int i = 0; i < 10; i++)
//        {
//            if (Physics.Raycast(pos, Vector3.down, out rch, i+1, 1 << Pathea.Layer.VFVoxelTerrain | 1 << Pathea.Layer.Building))
//            {
//                m_Npc.m_Pos = rch.point;
//                prePos = pos;
//                pos += new Vector3(0.0f, 1f, 0.0f);
//                break;
//            }
//        }

//        if (rch.point == Vector3.zero)
//            m_Npc.m_Pos = m_DestPos;
//        else
//            m_Npc.m_Pos = rch.point;
//    }
//}
