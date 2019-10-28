using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using WhiteCat.UnityExtension;

public class CreationMeshLoader : MonoBehaviour
{
	private int m_CreationID = 0;
	private CreationData m_CreationData;
	private VCMCComputer m_Computer;
	private VCMeshMgr m_MeshMgr;
	private WhiteCat.CreationController creationController;

	public string m_DebugCommand = "";
	private bool m_lastVisible = false;
	private bool m_Visible = false;
	private bool m_Loading = false;
	public bool m_ShowNow = false;
	public bool m_HideNow = false;

    public bool m_Meshdagger = false;
   

	public delegate void DNotify ();
	public event DNotify OnLoadMeshComplete;
	public event DNotify OnFreeMesh;

	int CreationID
	{
		set
		{
			if ( m_Computer != null )
				m_Computer.Destroy();

			CreationData crd = CreationMgr.GetCreation(value);
			if ( crd != null )
			{
                m_CreationID = value;
                m_CreationData = crd;

                m_MeshMgr = GetComponent<VCMeshMgr>();
                m_Computer = new VCMCComputer();
                m_Computer.Init(new IntVector3(crd.m_IsoData.m_HeadInfo.xSize,
                                                 crd.m_IsoData.m_HeadInfo.ySize,
                                                 crd.m_IsoData.m_HeadInfo.zSize),
                                 m_MeshMgr, false);

                // [VCCase] - Create box collider
                if (crd.m_Attribute.m_Type == ECreation.Vehicle ||
                     crd.m_Attribute.m_Type == ECreation.Aircraft ||
                     crd.m_Attribute.m_Type == ECreation.Boat ||
                     crd.m_Attribute.m_Type == ECreation.SimpleObject ||
                     crd.m_Attribute.m_Type == ECreation.AITurret)
                    m_Computer.m_CreateBoxCollider = true;
			}
			else
			{
				m_CreationID = 0;
				m_CreationData = null;
				m_Computer = null;
				m_MeshMgr = null;
			}
		}
	}
	
    void  InitCreationID(int creationId)
    {
        if (m_Computer != null)
            m_Computer.Destroy();

        CreationData crd = CreationMgr.GetCreation(creationId);
        if (crd != null)
        {
            m_CreationID = creationId;
            m_CreationData = crd;

            m_MeshMgr = GetComponent<VCMeshMgr>();
            m_Computer = new VCMCComputer();
            m_Computer.Init(new IntVector3(crd.m_IsoData.m_HeadInfo.xSize,
                                             crd.m_IsoData.m_HeadInfo.ySize,
                                             crd.m_IsoData.m_HeadInfo.zSize),
                             m_MeshMgr, false);

            // [VCCase] - Create box collider
            if (crd.m_Attribute.m_Type == ECreation.Vehicle ||
                 crd.m_Attribute.m_Type == ECreation.Aircraft ||
                 crd.m_Attribute.m_Type == ECreation.Boat ||
                 crd.m_Attribute.m_Type == ECreation.SimpleObject ||
                 crd.m_Attribute.m_Type == ECreation.AITurret)
                m_Computer.m_CreateBoxCollider = true;
        }
        else
        {
            m_CreationID = 0;
            m_CreationData = null;
            m_Computer = null;
            m_MeshMgr = null;
        }

    }

    void InitCreationIDClone(int creationId, VFCreationDataSource dataSource, VCMeshMgr mesh_mgr)
    {
        if (m_Computer != null)
            m_Computer.Destroy();

        CreationData crd = CreationMgr.GetCreation(creationId);
        if (crd != null)
        {
            m_CreationID = creationId;
            m_CreationData = crd;

            m_MeshMgr = mesh_mgr;
            m_MeshMgr.m_LeftSidePos = !mesh_mgr.m_LeftSidePos;
            m_Computer = new VCMCComputer();
            m_Computer.InitClone(dataSource, m_MeshMgr,false);
           
            if (crd.m_Attribute.m_Type == ECreation.Vehicle ||
                 crd.m_Attribute.m_Type == ECreation.Aircraft ||
                 crd.m_Attribute.m_Type == ECreation.Boat ||
                 crd.m_Attribute.m_Type == ECreation.SimpleObject ||
                 crd.m_Attribute.m_Type == ECreation.AITurret)
                m_Computer.m_CreateBoxCollider = true;
        }
        else
        {
            m_CreationID = 0;
            m_CreationData = null;
            m_Computer = null;
            m_MeshMgr = null;
        }

    }

	public bool Valid
	{
		get
		{
			if ( m_CreationID == 0 )
				return false;
			if ( m_CreationData == null )
				return false;
			if ( m_Computer == null )
				return false;
			if ( m_MeshMgr == null )
				return false;
			return true;
		}
	}
	
	public void FreeMesh ()
	{
		StopAllCoroutines();
		RevertMat();
		m_Loading = false;
		if ( Valid )
		{
			MeshFilter[] mfs = GetComponentsInChildren<MeshFilter>(true);
			foreach ( MeshFilter mf in mfs )
			{
				if ( mf.mesh != null )
				{
					Mesh.Destroy(mf.mesh);
					mf.mesh = null;
				}
			}
			//m_MeshMgr.FreeGameObjects();
			m_Computer.Clear();
		}
		if ( OnFreeMesh != null )
			OnFreeMesh();
	}
	
	public void LoadMesh ()
	{
		if ( m_Loading )
			FreeMesh();
		gameObject.layer = VCConfig.s_ProductLayer;
		//StartCoroutine(LoadMesh_coroutine());
		DoLoadMesh();
    }
	
	void DoLoadMesh ()
	{
		m_Loading = true;
		SetLoadingMat();
		if ( Valid )
		{
			// This way can avoid 'out of sync' bugs
			int vcnt = m_CreationData.m_IsoData.m_Voxels.Count;
			int[] vkeys = new int[vcnt];
			m_CreationData.m_IsoData.m_Voxels.Keys.CopyTo(vkeys, 0);
			Dictionary<int, VCVoxel> voxels = m_CreationData.m_IsoData.m_Voxels;
			if ( m_Computer != null )
			{
				//int step = WhiteCat.PEVCConfig.instance.vcountEveryFrame;
				//int step_1 = step - 1;

				for ( int i = 0; i < vcnt; ++i )
				{
					int key = vkeys[i];
                    if (m_Meshdagger && m_Computer != null && m_Computer.m_MeshMgr != null)
                    {
                        if (voxels != null && voxels.ContainsKey(key) && m_Computer.InSide(key, m_CreationData.m_IsoData.m_HeadInfo.xSize, m_Computer.m_MeshMgr.m_LeftSidePos))
                            m_Computer.AlterVoxel(key, voxels[key]);
                    }
                    else
                    {
                        if (m_Computer != null && voxels != null && voxels.ContainsKey(key))
                            m_Computer.AlterVoxel(key, voxels[key]);
                    }
					

					//if ( i % step == step_1)
					//{
					//	if ( m_Computer != null ) m_Computer.ReqMesh();
					//	yield return 0;
					//}
				}
				if ( m_Computer != null ) m_Computer.ReqMesh();
			}

			creationController.AddBuildFinishedListener(RevertMat);
        }
		m_Loading = false;
	}

	public void Init (WhiteCat.CreationController controller)
	{
		m_lastVisible = false;
		m_Visible = false;
        
        InitCreationID(controller.creationData.m_ObjectID);
		//CreationID = controller.creationData.m_ObjectID;

		this.creationController = controller;
        controller.onUpdate += UpdateMeshLoader;
    }

    public void InitClone(WhiteCat.CreationController controller, VFCreationDataSource dataSource, VCMeshMgr mesh)//)
    {
        m_lastVisible = false;
        m_Visible = false;

        InitCreationIDClone(controller.creationData.m_ObjectID, dataSource,mesh);
       // InitCreationID(controller.creationData.m_ObjectID);

        this.creationController = controller;
        controller.onUpdate += UpdateMeshLoader;
    }

	void OnDestroy ()
	{
		StopAllCoroutines();
		m_Loading = false;
		if ( m_Computer != null )
		{
			m_Computer.Destroy();
			m_Computer = null;
		}
	}
	
	void UpdateMeshLoader ()
	{
		if ( Valid )
		{
			if ( m_ShowNow )
			{
				m_ShowNow = false;
				m_DebugCommand = "show";
			}
			if ( m_HideNow )
			{
				m_HideNow = false;
				m_DebugCommand = "hide";
			}
			if ( m_DebugCommand.ToLower().Trim() == "show" )
			{
				m_DebugCommand = "";
				LoadMesh();
			}
			else if ( m_DebugCommand.ToLower().Trim() == "hide" )
			{
				m_DebugCommand = "";
				FreeMesh();
			}
			m_Visible = VCGameMediator.MeshVisible(this);
			if ( m_lastVisible != m_Visible )
			{
				m_lastVisible = m_Visible;
				if ( m_Visible )
					LoadMesh();
				else
					FreeMesh(); 
			}
		}
	}
	
	void SetLoadingMat ()
	{
		if ( Valid && m_Loading )
			m_MeshMgr.m_MeshMat = VCEditor.Instance.m_HolographicMat;
	}

	void RevertMat ()
	{
		if ( Valid && !m_Loading )
		{
			m_MeshMgr.m_MeshMat = m_CreationData.m_MeshMgr.m_MeshMat;
			m_MeshMgr.SetMeshMat(m_MeshMgr.m_MeshMat);
			OutlineObject oo = VCUtils.GetComponentOrOnParent<OutlineObject>(this.gameObject);
			if ( oo != null )
				oo.ReplaceInCache(VCEditor.Instance.m_HolographicMat, m_MeshMgr.m_MeshMat);
			if ( OnLoadMeshComplete != null )
				OnLoadMeshComplete();
		}
	}
}
