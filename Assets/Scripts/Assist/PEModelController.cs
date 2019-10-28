using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;

public class PEModelController : MonoBehaviour 
{
    public GameObject deathModel;
    public int deathEffectID;

    AnimatorCmpt m_AnimCmpt;
	[SerializeField]
    Rigidbody m_Rigidbody;
	[SerializeField]
	Collider[] m_Colliders;

    Bounds m_ColliderBounds;
    public Bounds ColliderBounds { get { return m_ColliderBounds; } }

    public Collider[] colliders 
	{ 
		get
		{
			if(null == m_Colliders || 0 == m_Colliders.Length)
				m_Colliders = PETools.PEUtil.GetCmpts<Collider>(transform);
			return m_Colliders; 
		}

	}
	[SerializeField]
    Renderer[] m_Renderers;
	Renderer[] _renderers
	{
		get
		{
			if(null == m_Renderers || 0 == m_Renderers.Length)
				InitRenderers();
			return m_Renderers;
		}
	}
	[SerializeField]
	StandardAlphaAnimator m_Alpha = null;
//	Dictionary<string, Transform> m_Childs = new Dictionary<string, Transform>();

	static bool IsDamageCollider(Collider col){
		return col.isTrigger;
	}

	public void ResetModelInfo()
	{
		m_Rigidbody = GetComponent<Rigidbody>();
		m_Colliders = PETools.PEUtil.GetCmpts<Collider>(transform);
		InitRenderers();
//		ActivateDamageCollider(false);
	}

    public Material[] GetMaterials()
    {
        if (m_Renderers != null && m_Renderers.Length > 0)
        {
            //for (int i = 0; i < m_Renderers.Length; i++)
            //{
                return m_Renderers[0].materials;
            //}
        }

        return null;
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

    public void ActivateDeathEffect()
    {
        if (deathEffectID > 0)
            Pathea.Effect.EffectBuilder.Instance.Register(deathEffectID, null, transform.position, Quaternion.identity);
    }

    public void ActivateDeathMode(bool isDeath)
    {
        if (deathModel != null)
        {
            deathModel.transform.position = transform.position;
            deathModel.transform.rotation = transform.rotation;

            gameObject.SetActive(!isDeath);
            deathModel.SetActive(isDeath);
        }
    }

    void CalculateColliderBounds()
    {
        m_ColliderBounds = new Bounds();
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider col = colliders[i];
            if (col != null && !IsDamageCollider(col))
            {
                if (m_ColliderBounds.center != Vector3.zero)
                    m_ColliderBounds.Encapsulate(col.bounds);
                else
                {
                    m_ColliderBounds.center = col.bounds.center;
                    m_ColliderBounds.size = col.bounds.size;
                }
            }
        }

    }

//    void Awake()
//    {
//		m_Rigidbody = GetComponent<Rigidbody>();
//		InitAll ();
//		ActivateDamageCollider(false);
//    }

    void Start()
	{
		m_AnimCmpt = GetComponentInParent<AnimatorCmpt>();
	}

    void Update()
    {
        CalculateColliderBounds();

        if (deathModel != null)
        {
            deathModel.transform.position = transform.position;
            deathModel.transform.rotation = transform.rotation;
        }
    }

    void AnimatorEvent(string para)
    {
        if (null != m_AnimCmpt)
            m_AnimCmpt.AnimEvent(para);
    }

//	void InitAll()
//	{
//		m_renderers = GetComponentsInChildren<Renderer>();	// not put this in loop because unreasonable memory allocated
//		for (int i = 0; i < m_renderers.Length; i++) {
//			if(m_renderers[i] is SkinnedMeshRenderer){
//				m_Alpha = m_renderers[i].GetComponent<StandardAlphaAnimator>();
//				if(m_Alpha != null)
//					break;
//			}
//		}
//		m_Colliders.Clear ();
//		GetComponentsInChildren<Collider> (m_Colliders);
//	}

	void InitRenderers()
	{
		m_Renderers = PETools.PEUtil.GetCmpts<Renderer>(transform);
		for (int i = 0; i < m_Renderers.Length; i++) 
		{
			if(m_Renderers[i] is SkinnedMeshRenderer)
			{
				m_Alpha = m_Renderers[i].GetComponent<StandardAlphaAnimator>();
				if(m_Alpha != null)
					break;
			}
		}
	}

    public Rigidbody Rigid
    {
        get { return m_Rigidbody; }
    }

    public void Remodel()
    {
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
        if(m_Alpha != null)
        {
            m_Alpha.SetAlpha(0.0f);
            m_Alpha.FadeOut(time);
        }
    }

    public void ActivatePhysics(bool value)
    {
        if(m_Rigidbody != null)
        {
            m_Rigidbody.isKinematic = !value;
        }
    }

    public void ActivateRenderer(bool value)
    {
        for (int i = 0; i < _renderers.Length; i++) {
			Renderer renderer = _renderers [i];
			if (renderer != null)
				renderer.enabled = value;
		}
    }

    public void ActivateColliders(bool value)
	{
        for (int i = 0; i < colliders.Length; i++) {
			Collider col = colliders [i];
			if (col != null && !col.isTrigger)
				col.enabled = value;
		}
    }

//    public void ActivateDamageCollider(bool value)
//    {
//		if(!CheckColliders())
//			return;
//		for (int i = 0; i < colliders.Length; i++) {
//			Collider col = colliders [i];
//			if (col != null && IsDamageCollider(col))
//				col.enabled = value;
//		}
//    }
}
