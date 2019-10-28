//using UnityEngine;
//using System.Collections;
//using Pathea;
//public class CSPatrolBehave : CSNPCBehave 
//{
//    private CSPatrolParam m_Param;

//    private enum PatrolState
//    {
//        None,
//        Prepare,
//        Stay,
//        Walk
//    }

//    private PatrolState m_PatrolState;

//    #region BEHAVE_FUNC

//    public override void Init (EPriority priority, CSBehaveParam param, PersonnelBase npc)
//    {
//        m_Priority = priority;
//        m_Param = param as CSPatrolParam;
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

//        m_State = EState.Running;
//    }

//    private float m_CurWalkDuring = 0f;
//    private Vector2 prePos;
//    private float m_StaySec;
//    private float m_CurStayTime = 0;
//    public override void Update ()
//    {
//        if (!m_Npc.CanBehave()) 
//            return;

//        if (m_State != EState.Running)
//            return;

//        if (m_PatrolState == PatrolState.None)
//        {
//            Vector3 pos = GetRandomPos();

//            // Find the Y
//            RaycastHit hit;
//            if (Physics.Raycast(pos + Vector3.up * 1000f, Vector3.down, out hit, 1000, 1 << Pathea.Layer.VFVoxelTerrain))
//            {
//                pos.y = hit.point.y + 0.1f;
//                m_Npc.MoveTo(pos, SpeedState.Walk);
//                m_PatrolState = PatrolState.Walk;
//                m_CurWalkDuring = 0;
//                m_Param.m_DesPos = pos;
//            }

//            prePos = new Vector2 (m_Npc.m_Pos.x, m_Npc.m_Pos.z);
//        }
//        else if (m_PatrolState == PatrolState.Walk)
//        {
//            Vector2 curPos = new Vector2(m_Npc.m_Pos.x, m_Npc.m_Pos.z);
//            if ((int)( m_CurWalkDuring) == 5)
//            {
//                if ((prePos - curPos).sqrMagnitude < 4)
//                {
////					m_Npc.Stay();
//                    m_PatrolState = PatrolState.Stay;
//                    m_StaySec = Random.Range(m_Param.m_MinStaySec, m_Param.m_MaxStaySec);
//                    m_CurStayTime = 0;
//                }

//                m_CurWalkDuring = 0;
//                prePos = new Vector2 (m_Npc.m_Pos.x, m_Npc.m_Pos.z);
//            }

//            // Is Close to the dest
//            Vector2 desPos = new Vector2(m_Param.m_DesPos.x, m_Param.m_DesPos.z);
//            if ((desPos - curPos).sqrMagnitude < 3.0f)
//            {
//                m_PatrolState = PatrolState.Stay;
//                m_StaySec = Random.Range(m_Param.m_MinStaySec, m_Param.m_MaxStaySec);
//                m_CurStayTime = 0;
//            }


//            m_CurWalkDuring += Time.deltaTime; 
//        }
//        else if (m_PatrolState == PatrolState.Stay)
//        {
//            if (m_CurStayTime >= m_StaySec)
//            {
//                Vector3 pos = GetRandomPos();

//                // Find the Y
//                RaycastHit hit;
//                if (Physics.Raycast(pos + Vector3.up * 600f, Vector3.down, out hit, 1000, 1 << Pathea.Layer.VFVoxelTerrain))
//                {
//                    pos.y = hit.point.y + 0.1f;
//                    m_Npc.MoveTo(pos, SpeedState.Walk);
//                    m_PatrolState = PatrolState.Walk;
//                    m_CurWalkDuring = 0;
//                    m_Param.m_DesPos = pos;
//                }
//            }

//            m_CurStayTime += Time.deltaTime;
//            prePos = new Vector2 (m_Npc.m_Pos.x, m_Npc.m_Pos.z);
//        }
//    }

//    public override void Break()
//    {
//        m_Npc.MoveTo(Vector3.zero, SpeedState.None);
//    }

//    public override void Pause ()
//    {
//        m_Npc.MoveTo(Vector3.zero, SpeedState.None);
//        m_PatrolState = PatrolState.None;
//        m_State = EState.Pause;
//    }

//    public override void Continue ()
//    {
//        m_State = EState.Running;
//    }

//    public override void Over ()
//    {
//        m_Npc.MoveTo(Vector3.zero, SpeedState.None);
//    }

//    #endregion


//    Vector3 GetRandomPos ()
//    {
//        Vector2 randomFactor = Random.insideUnitCircle ;
//        //randomFactor = 2 * randomFactor - Vector2.one;
//        Vector3 pos = m_Param.m_Center 	+ (m_Param.m_Radius - 25) * new Vector3(randomFactor.x, 0,  randomFactor.y);
//        return pos;
//    }

//    private void OnArriveDest()
//    {

//    }

//}
