using UnityEngine;
using System.Collections;
using System.Collections.Generic;
 
public abstract class CSCommon : CSEntity 
{
	private CSAssembly	m_Assembly; 
	public CSAssembly	Assembly 
	{ 
		get 
		{
			return m_Assembly; 
		}  

		set 
		{
			if (m_Assembly != value)
			{
				m_Assembly = value;
				ExcuteEvent(CSConst.eetCommon_ChangeAssembly, value);
			}
			else
				m_Assembly = value;
		}
	}
	
	public float	m_Power;

	#region WORKER_ABOUT
	// Workers
	protected CSPersonnel[]	m_Workers;
	
	protected PersonnelSpace[] m_WorkSpaces;
	public PersonnelSpace[] WorkSpaces		{ get { return m_WorkSpaces; } }
    public Pathea.Operate.PEMachine WorkPoints;

	public const float MAXDistance  =  2f;
	public const float SqrMaxDistance = MAXDistance * MAXDistance;
	public int WorkerCount	
	{
		get {
			int count = 0;
			if(m_Workers!=null){
				foreach (CSPersonnel pl in m_Workers)
				{
					if (pl != null)
						count ++;
				}
			}
			return count;
		}
	}
	public int WorkerMaxCount 		
	{
		get {
			if (m_Workers == null)
				return 0;
			return m_Workers.Length; 
		} 
	}

	public CSPersonnel Worker(int index)
	{
		return m_Workers[index];
	}

    public virtual float GetWorkerParam()
    {
        return GetWorkingCount();
    }

	public int GetWorkingCount()
	{
		int workingCnt = 0;
		foreach (CSPersonnel person in m_Workers)
		{

            if (person != null)
                workingCnt++;
		}
		return workingCnt;
	}

	public virtual bool AddWorker (CSPersonnel npc)
	{
		if (m_Workers == null)
		{
			Debug.LogWarning("There are not workers in this common entity.");
			return false;
		}
        foreach (CSPersonnel pl in m_Workers)
        {
            if (pl != null && pl == npc)
                return true;
        }
		for (int i = 0; i <m_Workers.Length; i++)
		{
			if (m_Workers[i] == null)
			{
				m_Workers[i] = npc;

				return true;
			}
		}
		
		return false;
	}

    public virtual void RemoveWorker(CSPersonnel npc)
	{
		if (m_Workers == null)
		{
			Debug.LogWarning("There are not workers in this common entity.");
			return;
		}
		
		for (int i = 0; i < m_Workers.Length; i++)
		{
			if (m_Workers[i] == npc)
			{
                for (int j = 0; j < m_WorkSpaces.Length; j++)
                {
                    if (m_WorkSpaces[j].m_Person == m_Workers[i])
                    {
                        m_WorkSpaces[j].m_Person = null;
                        break;
                    }
                }

				m_Workers[i] = null;
				 break;
			}
		}
	}
	

//	public void ClearAllWorkSpace()
//	{
//		for (int i = 0; i < m_WorkSpaces.Length; i++)
//		{
//			if (m_WorkSpaces[i].m_Person != null)
//				m_WorkSpaces[i].m_Person = null;
//		}
//	}
	

	public void ClearWorkers()
	{
		if (m_Workers == null)
		{
			Debug.LogWarning("There are not workers in this common entity.");
			return;
		}

		for (int i = 0; i < m_Workers.Length; i++)
		{
			if (m_Workers[i] != null)
			{
				m_Workers[i].WorkRoom=null;
			}
		}
	}


	public void AutoSettleWorkers()
	{
		if (m_Creator == null)
		{
			Debug.LogError("The creator is null.");
			return;
		}
		CSPersonnel[] personnels = m_Creator.GetNpcs();
		if (personnels == null)
			return;

		foreach (CSPersonnel csp in personnels)
		{
			int restCnt = WorkerMaxCount - WorkerCount;
			if (restCnt > 0)
			{
				if (csp.WorkRoom == null)
				{
					csp.WorkRoom=this;
				}
			}
			else
				break;
		}
	}

	public virtual bool NeedWorkers ()
	{
		return false;
	}

	#endregion

	public override void ChangeState ()
	{
		bool oldState = m_IsRunning;
		if (m_Assembly != null && m_Assembly.IsRunning)
			m_IsRunning = true;
		else
			m_IsRunning = false;
		if (oldState && !m_IsRunning){
			DestroySomeData();
			//UpdateDataToUI();
		}
		else if (!oldState && m_IsRunning)
			UpdateDataToUI();
	}
	
	
	public override void DestroySelf ()
	{
		base.DestroySelf ();
		
		if (m_Assembly != null)
			m_Assembly.DetachCommonEntity(this);

		// send away Worker
		if (m_Workers != null)
		{
			for (int i = 0; i < m_Workers.Length; i++)
			{
				if (m_Workers[i] != null)
					m_Workers[i].ClearWorkRoom();
			}
		}
	}

	public override void Update ()
	{
		base.Update ();

        //if (gameObject != null)
        //{
        //    if (m_WorkSpaces != null && Time.frameCount % 20 == 0)
        //    {
        //        for (int i = 0; i < m_WorkSpaces.Length; i++)
        //        {
        //            if (m_WorkSpaces[i] == null)
        //                continue;

        //            Vector3 pos = CSUtils.FindAffixedPos(m_WorkSpaces[i].Pos, PersonnelBase.s_BoundPos, MAXDistance);
        //            if ((pos - m_WorkSpaces[i].Pos).sqrMagnitude < SqrMaxDistance)
        //            {
        //                m_WorkSpaces[i].m_CanUse = true;
        //            }
        //            else
        //                m_WorkSpaces[i].m_CanUse = false;
        //            m_WorkSpaces[i].FinalPos = pos;

        //        }
        //    }
        //}
        //else
        //{
        //    if (m_WorkSpaces != null && Time.frameCount % 20 == 0)
        //    {
        //        for (int i = 0; i < m_WorkSpaces.Length; i++)
        //        {
        //            if (m_WorkSpaces[i] == null)
        //                continue;

        //            m_WorkSpaces[i].m_CanUse = true;
        //            m_WorkSpaces[i].FinalPos = m_WorkSpaces[i].Pos;
        //        }
        //    }
        //}
	}

    //public PersonnelSpace FindEmptySpace ()
    //{
    //    if (gameObject != null)
    //    {
    //        if (m_WorkSpaces != null )
    //        {
    //            for (int i = 0; i < m_WorkSpaces.Length; i++)
    //            {
    //                if (m_WorkSpaces[i] == null || m_WorkSpaces[i].m_Person != null)
    //                    continue;

    //                Vector3 pos = CSUtils.FindAffixedPos(m_WorkSpaces[i].Pos, PersonnelBase.s_BoundPos, MAXDistance);

    //                m_WorkSpaces[i].FinalPos = pos;
    //                if ((pos - m_WorkSpaces[i].Pos).sqrMagnitude < SqrMaxDistance)
    //                {
    //                    m_WorkSpaces[i].m_CanUse = true;
    //                    return m_WorkSpaces[i];
    //                }
    //                else
    //                    m_WorkSpaces[i].m_CanUse = false;
    //            }
    //        }
    //    }
    //    else
    //    {
    //        if (m_WorkSpaces != null )
    //        {
    //            for (int i = 0; i < m_WorkSpaces.Length; i++)
    //            {
    //                if (m_WorkSpaces[i] == null || m_WorkSpaces[i].m_Person != null)
    //                    continue;

    //                m_WorkSpaces[i].m_CanUse = true;
    //                return m_WorkSpaces[i];
    //            }
    //        }
    //    }

    //    return null;
    //}

    public PersonnelSpace FindEmptySpace(PersonnelBase person)
    {
        for (int i = 0; i < m_WorkSpaces.Length; i++)
        {
            if (m_WorkSpaces[i].m_Person == null||m_WorkSpaces[i].m_Person==person)
            {
                return m_WorkSpaces[i];
            }
        }
        return null;
    }


//	Vector3 _findValidPos (Vector3 destPos)
//	{
//		RaycastHit rch;
//		Vector3 _pos = destPos + new Vector3(0, 3, 0);
//		if (Physics.Raycast(_pos, Vector3.down, out rch, 4, 1 << Pathea.Layer.VFVoxelTerrain))
//		{
//			destPos = rch.point;
//		}
//
//		Vector3 rPos = destPos;
//
//		// Block
//		if (Block45Man.self != null)
//		{
//			int blockCnt = 0;
//			
//			foreach (Vector3 vec in PersonnelBase.s_BoundPos)
//			{
//				Vector3 pos = (destPos + vec) * Block45Constants._scaleInverted;
	//				B45Block block = Block45Man.self.DataSource.SafeRead((int)pos.x, (int)pos.y, (int)pos.z);
//				if (block.blockType != 0 || block.materialType != 0)
//					blockCnt++;
//			}
//
//			// search up
//			if (blockCnt > 0)
//			{
//				int lg = 2;
//				Vector3 orign = destPos +  PersonnelBase.s_BoundPos[PersonnelBase.s_BoundPos.Length - 1];
//				
//				while (true)
//				{
//					Vector3 pos = orign;
//					int _bCnt = 0;
//					for (int i = 0; i < lg; i++)
//					{
//						Vector3 blockPos = pos * Block45Constants._scaleInverted;
	//						B45Block block = Block45Man.self.DataSource.SafeRead((int)blockPos.x, (int)blockPos.y, (int)blockPos.z);
//						if (block.blockType != 0 || block.materialType != 0)
//							_bCnt++;
//						
//						pos += new Vector3(0, Block45Constants._scale, 0);
//					}
//					
//					if (_bCnt == 0)
//					{
//						rPos = orign;
//						break;
//					}
//					
//					orign += new Vector3(0, lg * Block45Constants._scale, 0);
//				}
//			}
//			// search down
//			else 
//			{
//				Vector3 orign = destPos +  PersonnelBase.s_BoundPos[0];
//				
//				int count = Mathf.CeilToInt( MAXDistance ) * Block45Constants._scaleInverted  + 1;
//				for (int i = count; i > 0; i--)
//				{
//					Vector3 blockPos = orign * Block45Constants._scaleInverted;
	//					B45Block block = Block45Man.self.DataSource.SafeRead((int)blockPos.x, (int)blockPos.y, (int)blockPos.z);
//					if (block.blockType != 0 || block.materialType != 0)
//					{
//						orign += new Vector3(0, Block45Constants._scale, 0);
//						break;
//					}
//					else
//						orign -= new Vector3(0, Block45Constants._scale, 0); 
//				}
//				
//				rPos = orign;
//			}
//		}
//
//		// Voxel
//		if (VFVoxelTerrain.self != null)
//		{
//			Vector3 orgin = rPos;
//			
//			while(true)
//			{
//				Vector3 pos = orgin;
//				VFVoxel voxel = VFVoxelTerrain.self.Voxels.SafeRead((int)pos.x, (int)pos.y, (int)pos.z);
//				if (voxel.Volume < 128)
//					break;
//				
//				orgin += new Vector3(0, 1, 0);
//			}
//			
//			rPos = orgin;
//		}
//
//		return rPos;
//	}
	
}
