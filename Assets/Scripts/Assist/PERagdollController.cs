using UnityEngine;
using System.Collections;
using AnimFollow;
using PETools;

public class RagdollHitInfo
{
    public Transform hitTransform;
    public Vector3 hitPoint;
    public Vector3 hitForce;
    public Vector3 hitNormal;
}

public interface IRagdollHandler
{
    void OnRagdollBuild(GameObject obj);

    void OnDetachJoint(GameObject boneRagdoll);
    void OnReattachJoint(GameObject boneRagdoll);
    void OnAttachJoint(GameObject boneRagdoll);

    void OnFallBegin(GameObject ragdoll);
    void OnFallFinished(GameObject ragdoll);
    void OnGetupBegin(GameObject ragdoll);
    void OnGetupFinished(GameObject ragdoll);

    void ActivateRagdollRenderer(bool isActive);
}

public class PERagdollController : RagdollControl_AF 
{
    bool m_GetupReady;
    bool m_RagdollActive;
    IRagdollHandler m_Handler;

    [SerializeField]
	Renderer[] m_Renderers;
    [SerializeField]
	StandardAlphaAnimator m_Alpha;
    [SerializeField]
    PERagdollEffect m_RagdollEffect;
	[SerializeField]
	Collider[]	m_Colliders;
	public Collider[] colliders
	{
		get
		{
			if(null == m_Colliders || 0 == m_Colliders.Length)
				m_Colliders = PEUtil.GetCmpts<Collider>(transform);
			return m_Colliders;
		}
	}

    public IRagdollHandler Handler {
        get { return m_Handler; }
    }

	public void ResetRagdoll()
	{
        m_Alpha = PEUtil.GetCmpt<StandardAlphaAnimator>(transform);
        m_Renderers = PEUtil.GetCmpts<Renderer>(transform);
		m_Colliders = PEUtil.GetCmpts<Collider>(transform);
		slaveRigidBodies = PEUtil.GetCmpts<Rigidbody>(transform);
		if(0 < slaveRigidBodies.Length)
			ragdollRootBone = slaveRigidBodies[0].transform;

		animFollow = GetComponent<AnimFollow_AF>();
		if(null != animFollow)
			master = animFollow.master;
		if(null != master)
		{
			masterRootBone = PETools.PEUtil.GetChild(master.transform, ragdollRootBone.name);
			anim = PEUtil.GetCmpt<Animator>(master.transform);
		}
        if (m_RagdollEffect == null)
            m_RagdollEffect = gameObject.AddComponent<PERagdollEffect>();

        if(m_RagdollEffect != null)
            m_RagdollEffect.ResetRagdoll();
    }

  //  new public void Awake()
  //  {
  //      base.Awake();
		
		//InitRenderer ();

  //      if (ragdollRootBone != null)
  //      {
  //          m_RagdollEffect = ragdollRootBone.gameObject.AddComponent<PERagdollEffect>();
  //          m_RagdollEffect.SetRigidbody(slaveRigidBodies);
  //      }
  //  }

    public void SetHandler(IRagdollHandler handler)
    {
        m_Handler = handler;
		m_Handler.OnRagdollBuild(gameObject);
    }

    public void SetMaterials(Material[] materials)
    {
        if (materials != null && materials.Length > 0)
        {
            for (int i = 0; i < m_Renderers.Length; i++)
            {
                m_Renderers[i].materials = materials;
            }
        }
    }

    public bool IsRagdoll()
    {
        return m_RagdollActive;
    }

    public void FadeIn(float time = 2.0f)
    {
        if (m_Alpha != null)
            m_Alpha.FadeIn(time);
    }

    public void FadeOut(float time = 2.0f)
    {
        if (m_Alpha != null)
            m_Alpha.FadeOut(time);
    }

    public void HideView(float time)
    {
        if (m_Alpha != null)
        {
            m_Alpha.SetAlpha(0.0f);
            m_Alpha.FadeOut(time);
        }
    }

    public void ActivateRenderer(bool value)
    {
		ResetRagdoll ();

		foreach (Renderer renderer in m_Renderers)
        {
			if(null != renderer)
	            renderer.enabled = value;
        }
    }

    public void ActivatePhysics(bool value)
    {
        try
        {
            for (int i = 0; i < slaveRigidBodies.Length; i++)
            {
                if(slaveRigidBodies[i] != null)
                {
                    if (animFollow.active && value)
                        slaveRigidBodies[i].isKinematic = false;
                    else
                        slaveRigidBodies[i].isKinematic = true;
                }
            }
        }
        catch
        {
            throw new System.NullReferenceException(name);
        }
    
    }

    public void Activate(RagdollHitInfo hitInfo, bool isGetupReady)
    {
        //if (animFollow.active)
        //    return;

        shotByBullet = true;
        animFollow.active = true;
        m_GetupReady = isGetupReady;

        StartCoroutine(AddForce(hitInfo));
    }

    public void Deactivate(bool immediately = false)
    {
        //if (!animFollow.active)
        //    return;

        if (immediately)
        {
            falling = false;
            gettingUp = false;
            animFollow.active = false;

            OnGetupFinished();
        }
        else
        {
            m_GetupReady = true;
        }
    }

    public void SmrBuild(SkinnedMeshRenderer renderer)
    {
        if (renderer != null)
		{
			m_Renderers = new Renderer[1]{renderer};
            renderer.rootBone = ragdollRootBone;
            renderer.enabled = animFollow.active;
        }
    }

    #region attach joint
//    public void AttachJoint(string boneName, Transform boneJoint)
//    {
//        Transform boneChild = PETools.PEUtil.GetChild(transform, boneJoint.name);
//
//        if (boneChild != null)
//            return;
//
//        GameObject joint = GameObject.Instantiate(boneJoint.gameObject) as GameObject;
//        joint.name = PETools.PEUtil.ToPrefabName(joint.name);
//
//        Transform[] transforms = joint.GetComponentsInChildren<Transform>();
//        foreach (Transform tr in transforms)
//        {
//            tr.gameObject.layer = gameObject.layer;
//
//            if (tr.GetComponent<Collider>() != null)
//                GameObject.Destroy(tr.GetComponent<Collider>());
//
//            if (tr.GetComponent<Rigidbody>() != null)
//                GameObject.Destroy(tr.GetComponent<Rigidbody>());
//
//            if (tr.GetComponent<Light>() != null)
//                GameObject.Destroy(tr.GetComponent<Light>());
//
//            if (tr.GetComponent<ParticleSystem>() != null)
//                GameObject.Destroy(tr.GetComponent<ParticleSystem>());
//
//            if (tr.GetComponent<AudioSource>() != null)
//                GameObject.Destroy(tr.GetComponent<AudioSource>());
//        }
//
//        if (master != null && master.GetComponent<Collider>() != null)
//        {
//            Collider[] crs = joint.GetComponentsInChildren<Collider>();
//            foreach (Collider collider in crs)
//            {
//                if(collider.enabled)
//                    Physics.IgnoreCollision(master.GetComponent<Collider>(), collider);
//            }
//        }
//
//        ConfigurableJoint[] joints = boneJoint.GetComponentsInChildren<ConfigurableJoint>();
//        foreach (ConfigurableJoint conf in joints)
//        {
//            GameObject.Destroy(conf);
//
//            Rigidbody rigid = conf.gameObject.GetComponent<Rigidbody>();
//            if (rigid != null)
//                GameObject.Destroy(rigid);
//
//            Collider c = conf.gameObject.GetComponent<Collider>();
//            if (c != null)
//                GameObject.Destroy(c);
//        }
//
//        StartCoroutine(WaitDestroyComponent(boneName, joint));
//    }
//
//    IEnumerator WaitDestroyComponent(string boneName, GameObject joint)
//    {
//        yield return null;
//
//        animFollow.AttachJoint(boneName, joint.transform);
//        m_Handler.OnAttachJoint(joint);
//    }
//
//    public void Reattach(string boneName, Transform boneJoint)
//    {
//        Transform joint = PETools.PEUtil.GetChild(transform, boneJoint.name);
//        if (joint != null)
//        {
//            //animFollow.DetachJoint(joint);
//            //animFollow.AttachJoint(boneName, joint);
//
//            animFollow.Reattach(boneName, boneJoint);
//
//            m_Handler.OnReattachJoint(joint.gameObject);
//        }
//    }
//
//    public void DetachJoint(Transform boneJoint)
//    {
//        Transform boneRagdoll = PETools.PEUtil.GetChild(transform, boneJoint.name);
//        if (null == boneRagdoll)
//        {
//            Debug.LogError("can't find bone of model:" + boneJoint.name);
//            return;
//        }
//
//        animFollow.DetachJoint(boneRagdoll);
//
//        m_Handler.OnDetachJoint(boneRagdoll.gameObject);
//
//        GameObject.Destroy(boneRagdoll.gameObject);
//    }
    #endregion

    #region private func
    IEnumerator AddForce(RagdollHitInfo hitInfo)
    {
        if (hitInfo == null || hitInfo.hitTransform == null)
            yield break;

        yield return new WaitForFixedUpdate();

        Rigidbody r = hitInfo.hitTransform.GetComponent<Rigidbody>();
        if (r != null)
        {
            r.AddForceAtPosition(hitInfo.hitForce * 100, hitInfo.hitPoint);
        }

        //foreach (Rigidbody rigid in slaveRigidBodies)
        //{
        //    rigid.AddForceAtPosition(hitInfo.hitForce * rigid.mass, rigid.position);
        //}

        //if (ragdollRootBone.GetComponent<Rigidbody>() != null)
        //    ragdollRootBone.GetComponent<Rigidbody>().AddExplosionForce(hitInfo.hitForce.magnitude * 100, hitInfo.hitPoint, 5.0f);
    }
    #endregion

    #region inherit
    public bool IsReadyGetUp()
    {
        return falling && ragdollRootBone.GetComponent<Rigidbody>().velocity.magnitude < settledSpeed;
    }

    protected override bool IsGetupReady()
    {
        return m_GetupReady;
    }

    protected override void OnFallBegin()
    {
        if (m_RagdollEffect != null)
            m_RagdollEffect.IsActive = true;

        m_RagdollActive = true;
        m_Handler.OnFallBegin(gameObject);
    }

    protected override void OnFallFinished()
    {
        m_Handler.OnFallFinished(gameObject);
    }

    protected override void OnGetupBegin()
    {
        m_Handler.OnGetupBegin(gameObject);
    }

    protected override void OnGetupFinished()
    {
        if (m_RagdollEffect != null)
            m_RagdollEffect.IsActive = false;

        m_RagdollActive = false;
        m_Handler.OnGetupFinished(gameObject);
    }
    #endregion
}
