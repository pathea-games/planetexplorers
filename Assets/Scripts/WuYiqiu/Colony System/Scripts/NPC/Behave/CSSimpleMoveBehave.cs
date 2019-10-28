//using UnityEngine;
//using System.Collections;
//using Pathea;
//public class CSSimpleMoveBehave : CSNPCBehave 
//{
//    public override  Vector3 m_DestPos
//    {
//        get { return m_Param.m_DesPos; }
//        set { m_Param.m_DesPos = value; }
//    }
	
//    public override Quaternion m_DestRot {
//        get {
//            return m_Param.m_Rot;
//        }
//        set {
//            m_Param.m_Rot = value;
//        }
//    }

//    private CSSimpleMoveBParam m_Param;
//    private float m_CurDelayTime = 0;

//    private bool bArriveDest = false;

//    private float m_Speed;

//    const float c_RunDistance = 50.0f; 
	
//    #region BEHAVE_FUNC

//    public override void Init (EPriority priority, CSBehaveParam param, PersonnelBase npc)
//    {
//        m_Priority = priority;
//        m_Param = param as CSSimpleMoveBParam;
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
//            Debug.Log("Can't find the behave npc.");
//            return;
//        }

//        if (m_CurDelayTime < m_Param.m_DelayTime)
//        {
//            m_CurDelayTime += Time.deltaTime;
//            return;
//        }


//        m_State = EState.Running;
//    }
	
//    private float m_CurTime = 0f;
//    private Vector2 prePos;

//    public override void Update ()
//    {
//        if (!m_Npc.CanBehave())
//            return;

//        MoveTo(m_Param.m_DesPos);

//        Vector3 curDestPos = m_Param.m_DesPos;

//        // Record previous position when m_CurTime is Zero
//        if (((int)m_CurTime) == 0)
//            prePos = new Vector2 (m_Npc.m_Pos.x, m_Npc.m_Pos.z);

//        // Current position every tick
//        Vector2 curPos = new Vector2(m_Npc.m_Pos.x, m_Npc.m_Pos.z);

//        // Check if this man is Stagnant? 
//        if (!bArriveDest && ((int)m_CurTime) == 5)
//        {
//            if ((prePos - curPos).sqrMagnitude < 2)
//            {
//                // Fix Y?
//                m_Npc.MoveToImmediately(curDestPos);


////				if (Physics.Raycast(new Vector3(curDestPos.x, curDestPos.y + 10, curDestPos.z), Vector3.down, out rch, 200, 1 << Pathea.Layer.VFVoxelTerrain))
////				{
////
////				}
////				else
////					m_Npc.MoveToImmediately(curDestPos);
				   
//            }

//            m_CurTime = 0;
//        }

//        m_CurTime += Time.deltaTime; 

//        // Is Close to the dest
//        Vector2 desPos = new Vector2(curDestPos.x, curDestPos.z);
//        if ((desPos - curPos).sqrMagnitude < 3.0f)
//        {
//            if (m_Param.m_ForceFixPos)
//                m_Npc.m_Pos = m_Param.m_DesPos;
//            OnArriveDest();
//        }
//    }

//    public override void Break()
//    {
//        // Stop Move
//        m_Npc.MoveTo(Vector3.zero, SpeedState.None);
//    }

//    public override void Pause ()
//    {
//        // Stop Move
//        //--to do: new
//        //m_Npc.MoveTo(Vector3.zero, 0.0f);

//        m_State = EState.Pause;
//    }

//    public override void Continue ()
//    {
//        //--to do: new
//        //m_Npc.MoveTo(m_Param.m_DesPos, m_Speed);

//        m_State = EState.Running;
//    }

//    public override void Over ()
//    {

//    }
//    #endregion

//    private void MoveTo (Vector3 destPos)
//    {
//        if (m_Param.m_Speed < 0.0001f)
//        {
//            float dist = (m_Param.m_DesPos - m_Npc.m_Pos).magnitude;
//            m_Speed = Mathf.Clamp(dist / c_RunDistance * m_Npc.RunSpeed, m_Npc.WalkSpeed , m_Npc.RunSpeed);
//            //m_Speed =  m_Npc.WalkSpeed + Random.value * Mathf.Abs( m_Npc.RunSpeed - m_Npc.WalkSpeed);;
//        }
//        else
//            m_Speed = m_Param.m_Speed;
//        //--to do: new
//        //m_Npc.MoveTo(m_Param.m_DesPos, m_Speed);
//    }

//    private void OnArriveDest()
//    {
//        bArriveDest = true;

//        if (m_Param.onBehaveFinished != null)
//            m_Param.onBehaveFinished(m_Npc);

//        m_State = EState.Finished; 

////		m_Npc.MoveTo(Vector3.zero, 0.0f);

//    }

//    public void RestDestination (Vector3 desPos)
//    {
//        if (m_State == EState.Running)
//        {
//            m_Param.m_DesPos = desPos;
//            //--to do: new
//            //m_Npc.MoveTo(desPos, m_Speed);
//        }
//    }

	
//}
