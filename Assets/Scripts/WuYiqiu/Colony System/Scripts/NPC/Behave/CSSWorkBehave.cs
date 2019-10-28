//using UnityEngine;
//using System.Collections;

//public class CSSWorkBehave  : CSNPCBehave 
//{
////	public override  Vector3 m_DestPos
////	{
////		get { return m_Param.m_DesPos; }
////		set {
////			m_Param.m_DesPos = value; 
////			m_bResetPos = true;
////		}
////	}
////
//    public bool m_bResetPos;
////
////	public override Quaternion m_DestRot {
////		get {
////			return m_Param.m_Rot;
////		}
////		set {
////			m_Param.m_Rot = value;
////			m_bResetPos = true;
////		}
////	}

////	public PersonnelSpace[] m_WorkSpace
////	{
////		get{
////			return m_Param.m_WorkSpace;
////		}
////
////		set
////		{
////			m_Param.m_WorkSpace = value;
////			m_bRestPos = true;
////		}
////	}

//    private bool bDoFinished = false;
//    private CSWorkBParam m_Param; 

//    private Vector3 m_DestPos;
//    private Quaternion m_DestRot;

//    public const float MAXDistance  =  2f;
//    public const float SqrMaxDistance = MAXDistance * MAXDistance;
	
//    #region BEHAVE_FUNC

//    public override void Init (EPriority priority, CSBehaveParam param, PersonnelBase npc)
//    {
//        m_Priority = priority;
//        m_Param = param as CSWorkBParam;
//        m_Npc = npc;
//        m_bResetPos = true;
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

//        Block45Man.self.AttachEvents(OnBlock45ColliderCreated, OnBlock45ColliderDestroy);
//    }

//    public override void Update ()
//    {
////        // Reset Pos if need
////        if (m_bResetPos)
////        {
////            PersonnelSpace outPs = null;

////            for (int i = 0; i < m_Param.m_WorkSpace.Length; i++)
////            {
////                if (m_Param.m_WorkSpace[i].m_Person == m_Npc)
////                    m_Param.m_WorkSpace[i].m_Person = null;
////            } 
			
////            for (int i = 0; i < m_Param.m_WorkSpace.Length; i++)
////            {
////                if (m_Param.m_WorkSpace[i].m_Person != null && m_Param.m_WorkSpace[i].m_Person != m_Npc)
////                    continue;
////                // Find the pos
////                SetTrans (m_Param.m_WorkSpace[i].Pos, m_Param.m_WorkSpace[i].m_Rot);

//////				if ((m_Npc.m_Pos - m_Param.m_WorkSpace[i].m_Pos).sqrMagnitude < SqrMaxDistance)
//////				{
//////					Vector3 _pos = new Vector3(m_Npc.m_Pos.x, m_Npc.m_Pos.y, m_Npc.m_Pos.z);
//////					RaycastHit rch;
//////					if (!Physics.Linecast(_pos, _pos + new Vector3(0, 2, 0), out rch, ~(1 << Pathea.Layer.VFVoxelTerrain))
//////						  || m_Npc.m_Go == rch.collider.gameObject)
//////					{
//////						outPs = m_Param.m_WorkSpace[i];
//////						break; 
//////					}
//////				}

////                outPs = m_Param.m_WorkSpace[i];
////            }

////            // Meet the block
////            if (outPs == null)
////            {
////                if (m_Param.onMeetBlock != null)
////                    m_Param.onMeetBlock(m_Npc);
					
////                m_State = EState.Finished;
////                m_bResetPos = false;
////                return;
////            }
////            else
////            {
////                outPs.m_Person = m_Npc;
////                m_DestPos = m_Npc.m_Pos; 
////                m_DestRot = m_Npc.m_Rot;
////            }

////            m_bResetPos = false;
////        }
////        else if (Time.frameCount % 32 == 0)
////        {
////        }

////        if (m_State == EState.Running)
////        {
////            DoWorkAction(true);
////            if (m_Param.onBehaveFinished != null)
////                m_Param.onBehaveFinished(m_Npc);

////            m_State = EState.Holding;
////        }


//    }

//    public override void Break()
//    {
//        DoWorkAction(false);
//        Block45Man.self.DetachEvents(OnBlock45ColliderCreated, OnBlock45ColliderDestroy);
//    }

//    public override void Pause ()
//    {
//        m_State = EState.Pause;
//        DoWorkAction(false);
//    }
	
//    public override void Continue ()
//    {
//        m_State = EState.Running;
//    }

//    public override void Over ()
//    {
//        DoWorkAction(false);
//        Block45Man.self.DetachEvents(OnBlock45ColliderCreated, OnBlock45ColliderDestroy);
//    }
//    #endregion
	 
//    private void SetTrans (Vector3 destPos, Quaternion dsetRot)
//    {
//        RaycastHit rch;
//        Vector3 _pos = destPos + new Vector3(0, 3, 0);
//        if (Physics.Raycast(_pos, Vector3.down, out rch, 4, 1 << Pathea.Layer.VFVoxelTerrain))
//        {
//            destPos = rch.point;
//        }

//        m_Npc.m_Rot = dsetRot;

//        Vector3 rPos = destPos;
//        // Block
//        if (Block45Man.self != null)
//        {
//            int blockCnt = 0;
			
//            foreach (Vector3 vec in PersonnelBase.s_BoundPos)
//            {
//                Vector3 pos = (destPos + vec) * Block45Constants._scaleInverted;
//                B45Block block = Block45Man.self.DataSource.SafeRead((int)pos.x, (int)pos.y, (int)pos.z);
//                if (block.blockType != 0 || block.materialType != 0)
//                    blockCnt++;
//            }

//            // search up
//            if (blockCnt > 0)
//            {
//                int lg = 2;
//                Vector3 orign = destPos +  PersonnelBase.s_BoundPos[PersonnelBase.s_BoundPos.Length - 1];
				
//                while (true)
//                {
//                    Vector3 pos = orign;
//                    int _bCnt = 0;
//                    for (int i = 0; i < lg; i++)
//                    {
//                        Vector3 blockPos = pos * Block45Constants._scaleInverted;
//                        B45Block block = Block45Man.self.DataSource.SafeRead((int)blockPos.x, (int)blockPos.y, (int)blockPos.z);
//                        if (block.blockType != 0 || block.materialType != 0)
//                            _bCnt++;
						
//                        pos += new Vector3(0, Block45Constants._scale, 0);
//                    }
					
//                    if (_bCnt == 0)
//                    {
//                        rPos = orign;
//                        break;
//                    }
					
//                    orign += new Vector3(0, lg * Block45Constants._scale, 0);
//                }
//            }
//            // search down
//            else 
//            {
//                Vector3 orign = destPos +  PersonnelBase.s_BoundPos[0];

//                int count = Mathf.CeilToInt( MAXDistance ) * Block45Constants._scaleInverted  + 1;
//                for (int i = count; i > 0; i--)
//                {
//                    Vector3 blockPos = orign * Block45Constants._scaleInverted;
//                    B45Block block = Block45Man.self.DataSource.SafeRead((int)blockPos.x, (int)blockPos.y, (int)blockPos.z);
//                    if (block.blockType != 0 || block.materialType != 0)
//                    {
//                        orign += new Vector3(0, Block45Constants._scale, 0);
//                        break;
//                    }
//                    else
//                        orign -= new Vector3(0, Block45Constants._scale, 0); 
//                }

//                rPos = orign;
//            }
//        }

//        // Voxel
//        if (VFVoxelTerrain.self != null)
//        {
//            Vector3 orgin = rPos;

//            while(true)
//            {
//                Vector3 pos = orgin;
//                VFVoxel voxel = VFVoxelTerrain.self.Voxels.SafeRead((int)pos.x, (int)pos.y, (int)pos.z);
//                if (voxel.Volume < 128)
//                    break;

//                orgin += new Vector3(0, 1, 0);
//            }

//            rPos = orgin;
//        }

//        m_Npc.m_Pos = rPos;
	
//    }
	
//    private void DoWorkAction(bool isDo)
//    {
//        switch (m_Param.m_WorkMode)
//        {
//        case CSWorkBParam.EWorkType.Enhance:
//            m_Npc.PlayAnimation(PersonnelBase.EAnimateType.WorkOnEnhanceMachine, isDo);
//            break;
//        case CSWorkBParam.EWorkType.Repair:
//            m_Npc.PlayAnimation(PersonnelBase.EAnimateType.WorkOnRepairMachine, isDo);
//            break;
//        case CSWorkBParam.EWorkType.Recycle:
//            m_Npc.PlayAnimation(PersonnelBase.EAnimateType.WorkOnRecycleMachine, isDo);
//            break;
//        default:
//            break;
//        }
//    }
	
//    void OnBlock45ColliderCreated(Block45ChunkGo b45Chunk)
//    {
//        if (m_DestPos == Vector3.zero)
//            return;

//        Bounds bounds = b45Chunk._mc.bounds;
//        Vector3 max = bounds.max + Vector3.up * 5;
//        Vector3 min = bounds.min + Vector3.down * 5;

//        Bounds tmpBounds = new Bounds();
//        tmpBounds.SetMinMax(min, max);
//        if (tmpBounds.Contains(m_DestPos)) 
//        {
//            SetTrans(m_DestPos, m_DestRot); 
//        }
//    }

//    void OnBlock45ColliderDestroy(Block45ChunkGo b45Chunk)
//    {

//    }
//}
