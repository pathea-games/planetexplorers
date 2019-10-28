using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Projector))]

public class MapProjectorAnyAxis : MonoBehaviour 
{
	#region USER_ATTRIBUTE
	
	public float Size = 50;
	public int ColorIndex = 0;
	public float Brightness = 1.0f;
	public float AlphaCoef = 1.0f;
	public Texture2D MapTex = null;
	public float Depth = 100; 

	[SerializeField] private Color[] ColorPreset;
	
	#endregion

	private Projector m_Projector;
	
	public Projector projector { get { return m_Projector; } }
	private Transform m_Trans;
	private Material m_Material;

	public float NearClip	
	{
		get { return m_Projector.nearClipPlane;}
		set { m_Projector.nearClipPlane = value; }
	}


	void Awake ()
	{
		m_Projector = gameObject.GetComponent<Projector>();
		m_Material = Material.Instantiate(projector.material) as Material;
		projector.material = m_Material;
		m_Trans = transform;

	}

	void OnDestroy ()
	{
		if (m_Material != null)
		{
			Material.Destroy(m_Material);
		}
	}

	// Use this for initialization
	void Start () 
	{

	}
	
	// Update is called once per frame
	void Update () 
	{
		if (m_Material == null)
			return;

		m_Projector.orthographicSize = Size;
		m_Projector.orthographic = true;
		
		Vector3 pos = m_Trans.position;
		Vector3 direction = m_Trans.forward;
		;
		
		m_Material.SetTexture("_MainTex", MapTex);
		m_Material.SetVector("_CenterAndSize", new Vector4(pos.x, pos.y, pos.z, Size));
		m_Material.SetVector("_Direction", new Vector4(direction.x, direction.y, direction.z));
		m_Material.SetColor("_TintColor", ColorPreset[ColorIndex % ColorPreset.Length]);
		m_Material.SetFloat("_Brightness", Brightness);
		m_Material.SetFloat("_AlphaCoef", AlphaCoef);
		m_Material.SetFloat("_Depth", Depth);
	}
}
