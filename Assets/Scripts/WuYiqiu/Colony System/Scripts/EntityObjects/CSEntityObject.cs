using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class CSEntityObject : GLBehaviour
{
    public CSBuildingLogic csbl;

    [HideInInspector]
    public ColonyBase _ColonyObj;

    [HideInInspector]
    public int m_ObjectId;

    public float m_Power;

    public int m_ItemID;

    public Material m_EffectsMat;

    [HideInInspector]
    public CSCreator m_Creator;

    [HideInInspector]
    public CSEntity m_Entity;

    [HideInInspector]
    public int m_BoundState = 0;

    public CSConst.ObjectType m_Type;

	Animation[] workAnimation;
	AudioController workSound;

    // Work Transform
    public Transform[] m_WorkTrans;

    private bool m_Start = false;
    public bool HasStarted
    {
        get
        {
            return m_Start;
        }
    }
    // Simulator if need
    //	public CSSimulator m_Simulator;

    public virtual int Init(CSBuildingLogic csbl, CSCreator creator, bool bFight = true)
    {
        this.csbl = csbl;
        CSEntityAttr attr = new CSEntityAttr();
        attr.m_InstanceId = csbl.InstanceId;
        attr.m_protoId = csbl.protoId;
        attr.m_Type = (int)csbl.m_Type;
        attr.m_Pos = csbl.transform.position;
        attr.m_LogicObj = csbl.gameObject;
        attr.m_Obj = gameObject;
        attr.m_Bound = GetObjectBounds();
        attr.m_Bound.center = transform.TransformPoint(attr.m_Bound.center);
        attr.m_ColonyBase = _ColonyObj;

        int r;
        r = creator.CreateEntity(attr, out m_Entity);

        if (r != CSConst.rrtSucceed)
            return r;

        m_Creator = creator;
        m_ObjectId = csbl.InstanceId;

        // Add ColonyRunner
        if (bFight)
        {
			//--to do: Detectable.cs, if the entity can be attack
//            if (gameObject.GetComponent<ColonyRunner>() == null)
//            {
//                ColonyRunner cr = gameObject.AddComponent<ColonyRunner>();
//                cr.m_Entity = m_Entity;
//            }

        }
        return r;
    }
    public virtual int Init(int id, CSCreator creator, bool bFight = true)
    {
        CSEntityAttr attr = new CSEntityAttr();
        attr.m_InstanceId = id;
        attr.m_protoId = m_ItemID;
        attr.m_Type = (int)m_Type;
        attr.m_Pos = transform.position;
        attr.m_Obj = gameObject;
        attr.m_Bound = GetObjectBounds();
        attr.m_Bound.center = transform.TransformPoint(attr.m_Bound.center);
        attr.m_ColonyBase = _ColonyObj;

        int r;
        r = creator.CreateEntity(attr, out m_Entity);

        if (r != CSConst.rrtSucceed)
            return r;


        m_Creator = creator;
        m_ObjectId = id;

        // Add ColonyRunner
        if (bFight)
        {
            //--to do: Detectable.cs, if the entity can be attack
            //            if (gameObject.GetComponent<ColonyRunner>() == null)
            //            {
            //                ColonyRunner cr = gameObject.AddComponent<ColonyRunner>();
            //                cr.m_Entity = m_Entity;
            //            }

        }
        return r;
    }

    // Get Entity Object Bounds (for all sub-meshes)
    public Bounds GetObjectBounds()
    {
        MeshFilter[] mfs = gameObject.GetComponentsInChildren<MeshFilter>(true);
        Bounds final = new Bounds(Vector3.zero, Vector3.zero);

        foreach (MeshFilter mf in mfs)
        {
            if (mf != null && mf.gameObject.layer == CSMain.CSEntityLayerIndex)
            {
                Bounds mb = mf.mesh.bounds;
                ExtendBounds(mb, mf.transform, ref final);
            }
        }

        SkinnedMeshRenderer[] smrs = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);

        foreach (SkinnedMeshRenderer smr in smrs)
        {
            if (smr != null && smr.gameObject.layer == CSMain.CSEntityLayerIndex)
            {
                Bounds mb = smr.sharedMesh.bounds;
                ExtendBounds(mb, smr.transform, ref final);
            }
        }

        return final;
    }

    private void ExtendBounds(Bounds b_in, Transform trans, ref Bounds b_out)
    {
        Vector3 xdir = trans.right * trans.lossyScale.x;
        Vector3 ydir = trans.up * trans.lossyScale.y;
        Vector3 zdir = trans.forward * trans.lossyScale.z;
        xdir = transform.InverseTransformDirection(xdir);
        ydir = transform.InverseTransformDirection(ydir);
        zdir = transform.InverseTransformDirection(zdir);
        Vector3 cen = transform.InverseTransformPoint(trans.TransformPoint(b_in.center));

        Vector3[] vert = new Vector3[8];
        for (int i = 0; i < 8; i++)
        {
            vert[i] = cen;
            if ((i & 1) == 0)
                vert[i] -= b_in.extents.x * xdir;
            else
                vert[i] += b_in.extents.x * xdir;

            if ((i & 2) == 0)
                vert[i] -= b_in.extents.y * ydir;
            else
                vert[i] += b_in.extents.y * ydir;

            if ((i & 4) == 0)
                vert[i] -= b_in.extents.z * zdir;
            else
                vert[i] += b_in.extents.z * zdir;
        }
        if (Vector3.Equals(b_out.extents, Vector3.zero))
        {
            b_out.center = vert[0];
            for (int i = 1; i < 8; i++)
                b_out.Encapsulate(vert[i]);
        }
        else
        {
            for (int i = 0; i < 8; i++)
                b_out.Encapsulate(vert[i]);
        }
    }

    public void SetCollidersEnable(bool enable)
    {
        Collider[] colliders = gameObject.GetComponentsInChildren<Collider>(true);

        foreach (Collider cd in colliders)
            cd.enabled = enable;
    }

    // Effect
    //private List<List<Material>> m_OrginMats = null;
    private MeshRenderer[] m_MeshRenders = null;
    private SkinnedMeshRenderer[] m_SkinnerMeshRenders = null;
    private MeshFilter[] m_Meshfilters;

    private float m_StartTime = 0;

    protected void OnDestroy()
    {
        //		if (m_Simulator != null)
        //			m_Simulator.IsValid = false;
    }

    protected void Awake()
    {
        //m_OrginMats = new List<List<Material>>();
        //Mesh m;
    }

    // Use this for initialization
    protected void Start()
    {
        GlobalGLs.AddGL(this);

        //		if (m_EffectsMat != null)
        //		{
        //			m_MeshRenders = gameObject.GetComponentsInChildren<MeshRenderer>(true);
        //
        //			foreach (MeshRenderer mr in m_MeshRenders)
        //			{
        //				List<Material> mats = new List<Material>();
        //				Material[] newMats = new Material[mr.materials.Length];
        //				for (int i = 0; i < mr.materials.Length; ++i)
        //				{
        //					mats.Add(mr.materials[i]);
        //					newMats[i] = m_EffectsMat;
        //					//mr.materials[i] = m_EffectsMat; 
        //
        //				}
        //				mr.materials = newMats;
        //
        //
        //				m_OrginMats.Add(mats);
        //			}
        //
        //			m_SkinnerMeshRenders = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        //
        //			foreach (SkinnedMeshRenderer mr in m_SkinnerMeshRenders)
        //			{
        //				List<Material> mats = new List<Material>();
        //				Material[] newMats = new Material[mr.materials.Length];
        //				for (int i = 0; i < mr.materials.Length; ++i)
        //				{
        //					mats.Add(mr.materials[i]);
        //					newMats[i] = m_EffectsMat;
        //				}
        //				mr.materials = newMats;
        //
        //				m_OrginMats.Add(mats);
        //			}
        //
        //			StartCoroutine(DelayToRestoreMat()); 
        //		}

        // Get Meshes
        if (m_EffectsMat != null)
        {
            m_Meshfilters = gameObject.GetComponentsInChildren<MeshFilter>(true);

            m_MeshRenders = gameObject.GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer mr in m_MeshRenders)
                mr.enabled = false;

            m_SkinnerMeshRenders = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (SkinnedMeshRenderer mr in m_SkinnerMeshRenders)
                mr.enabled = false;

            StartCoroutine(DelayToEndEffect());
        }

        m_StartTime = Time.time;


        // Animation
        workAnimation = gameObject.GetComponentsInChildren<Animation>(true);
		foreach (Animation an in workAnimation){
			an.Stop();
            an.playAutomatically = false;
		}

        m_Start = false;

		//Sound,create
		if(m_Entity!=null&&m_Entity.m_Info.workSound>=0)
			workSound = AudioManager.instance.Create(gameObject.transform.position,m_Entity.m_Info.workSound,gameObject.transform,false,false);

    }

	public void StartWork(){
		if(workAnimation!=null){
			foreach (Animation an in workAnimation){
				if(!an.isPlaying){
					an.wrapMode = WrapMode.Loop;
					an.Play();
				}
			}
		}
		if(workSound!=null&&!workSound.isPlaying)
			workSound.PlayAudio(0.5f);
		m_Start = true;
	}

	public void StopWork(){
		if(workAnimation!=null)
			foreach (Animation an in workAnimation){
				if(an.isPlaying&&an.wrapMode!=WrapMode.Once){
					an.wrapMode = WrapMode.Once;
				}
			}
		if(workSound!=null&&workSound.isPlaying)
			workSound.StopAudio(0.5f);
		m_Start = false;
	}

    IEnumerator DelayToEndEffect()
    {
        while (true)
        {
            if (Time.time - m_StartTime > 0.5f)
            {
                foreach (MeshRenderer mr in m_MeshRenders)
                    mr.enabled = true;

                foreach (SkinnedMeshRenderer mr in m_SkinnerMeshRenders)
                    mr.enabled = true;

                break;
            }
            else
            {
                foreach (MeshFilter mf in m_Meshfilters)
                {
                    Graphics.DrawMesh(mf.mesh, mf.transform.position, mf.transform.rotation, m_EffectsMat, 0);
                }

                foreach (SkinnedMeshRenderer mr in m_SkinnerMeshRenders)
                    Graphics.DrawMesh(mr.sharedMesh, mr.transform.position, mr.transform.rotation, m_EffectsMat, 0);

                //foreach 
                yield return 0;
            }
        }
    }

    // Update is called once per frame
    protected void Update()
    {
        //		if (m_Simulator != null)
        //			m_Simulator.IsValid = true;
		if(m_Entity!=null&&m_Entity.IsDoingJobOn){
			if(!m_Start)
				StartWork();
		}
		else{
			if(m_Start)
				StopWork();
		}
    }

    private Material m_LineMat;

    // 0 Not Draw    1 Green     2 Red
    public override void OnGL()
    {
        if (m_BoundState != 1 && m_BoundState != 2)
            return;

        Bounds objBounds = GetObjectBounds();
        Vector3[] vert = new Vector3[8];

        for (int i = 0; i < 8; i++)
        {
            vert[i] = objBounds.center;

            if ((i & 1) == 0)
                vert[i] -= objBounds.extents.x * Vector3.right;
            else
                vert[i] += objBounds.extents.x * Vector3.right;

            if ((i & 2) == 0)
                vert[i] -= objBounds.extents.y * Vector3.up;
            else
                vert[i] += objBounds.extents.y * Vector3.up;

            if ((i & 4) == 0)
                vert[i] -= objBounds.extents.z * Vector3.forward;
            else
                vert[i] += objBounds.extents.z * Vector3.forward;

            vert[i] = transform.TransformPoint(vert[i]);
        }

        // Create material if not has
        if (m_LineMat == null)
        {
			m_LineMat = new Material(Shader.Find("Lines/Colored Blended"));

            m_LineMat.hideFlags = HideFlags.HideAndDontSave;
            m_LineMat.shader.hideFlags = HideFlags.HideAndDontSave;
        }

        // Save camera's matrix.
        GL.PushMatrix();

        // Set the current material
        m_LineMat.SetPass(0);

        // Draw Lines -- twelve edges
        GL.Begin(GL.LINES);
        GL.Color(m_BoundState == 1 ? Color.green : Color.red);
        GL.Vertex3(vert[0].x, vert[0].y, vert[0].z);
        GL.Vertex3(vert[1].x, vert[1].y, vert[1].z);
        GL.Vertex3(vert[2].x, vert[2].y, vert[2].z);
        GL.Vertex3(vert[3].x, vert[3].y, vert[3].z);
        GL.Vertex3(vert[4].x, vert[4].y, vert[4].z);
        GL.Vertex3(vert[5].x, vert[5].y, vert[5].z);
        GL.Vertex3(vert[6].x, vert[6].y, vert[6].z);
        GL.Vertex3(vert[7].x, vert[7].y, vert[7].z);
        GL.Vertex3(vert[0].x, vert[0].y, vert[0].z);
        GL.Vertex3(vert[4].x, vert[4].y, vert[4].z);
        GL.Vertex3(vert[1].x, vert[1].y, vert[1].z);
        GL.Vertex3(vert[5].x, vert[5].y, vert[5].z);
        GL.Vertex3(vert[2].x, vert[2].y, vert[2].z);
        GL.Vertex3(vert[6].x, vert[6].y, vert[6].z);
        GL.Vertex3(vert[3].x, vert[3].y, vert[3].z);
        GL.Vertex3(vert[7].x, vert[7].y, vert[7].z);
        GL.Vertex3(vert[0].x, vert[0].y, vert[0].z);
        GL.Vertex3(vert[2].x, vert[2].y, vert[2].z);
        GL.Vertex3(vert[1].x, vert[1].y, vert[1].z);
        GL.Vertex3(vert[3].x, vert[3].y, vert[3].z);
        GL.Vertex3(vert[4].x, vert[4].y, vert[4].z);
        GL.Vertex3(vert[6].x, vert[6].y, vert[6].z);
        GL.Vertex3(vert[5].x, vert[5].y, vert[5].z);
        GL.Vertex3(vert[7].x, vert[7].y, vert[7].z);
        GL.End();

        // Draw Quads -- six faces
        GL.Begin(GL.QUADS);
        GL.Color(m_BoundState == 1 ? new Color(0.0f, 0.1f, 0.0f, 0.15f) : new Color(0.1f, 0.0f, 0.0f, 0.15f));
        GL.Vertex3(vert[0].x, vert[0].y, vert[0].z);
        GL.Vertex3(vert[1].x, vert[1].y, vert[1].z);
        GL.Vertex3(vert[3].x, vert[3].y, vert[3].z);
        GL.Vertex3(vert[2].x, vert[2].y, vert[2].z);
        GL.Vertex3(vert[4].x, vert[4].y, vert[4].z);
        GL.Vertex3(vert[5].x, vert[5].y, vert[5].z);
        GL.Vertex3(vert[7].x, vert[7].y, vert[7].z);
        GL.Vertex3(vert[6].x, vert[6].y, vert[6].z);
        GL.Vertex3(vert[3].x, vert[3].y, vert[3].z);
        GL.Vertex3(vert[2].x, vert[2].y, vert[2].z);
        GL.Vertex3(vert[6].x, vert[6].y, vert[6].z);
        GL.Vertex3(vert[7].x, vert[7].y, vert[7].z);
        GL.Vertex3(vert[0].x, vert[0].y, vert[0].z);
        GL.Vertex3(vert[1].x, vert[1].y, vert[1].z);
        GL.Vertex3(vert[5].x, vert[5].y, vert[5].z);
        GL.Vertex3(vert[4].x, vert[4].y, vert[4].z);
        GL.Vertex3(vert[1].x, vert[1].y, vert[1].z);
        GL.Vertex3(vert[5].x, vert[5].y, vert[5].z);
        GL.Vertex3(vert[7].x, vert[7].y, vert[7].z);
        GL.Vertex3(vert[3].x, vert[3].y, vert[3].z);
        GL.Vertex3(vert[0].x, vert[0].y, vert[0].z);
        GL.Vertex3(vert[4].x, vert[4].y, vert[4].z);
        GL.Vertex3(vert[6].x, vert[6].y, vert[6].z);
        GL.Vertex3(vert[2].x, vert[2].y, vert[2].z);
        GL.End();

        // Restore camera's matrix.
        GL.PopMatrix();
    }
}
