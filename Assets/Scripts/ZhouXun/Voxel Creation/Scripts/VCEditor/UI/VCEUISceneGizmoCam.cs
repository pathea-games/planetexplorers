using UnityEngine;
using System.Collections;

public class VCEUISceneGizmoCam : MonoBehaviour
{
	public GUISkin m_GUISkin = null;
	public Material m_FocusMaterial = null;
	public Material m_OriginMaterial = null;
	public Material m_XAxisMaterial = null;
	public Material m_YAxisMaterial = null;
	public Material m_ZAxisMaterial = null;
	public Material m_NXAxisMaterial = null;
	public Material m_NYAxisMaterial = null;
	public Material m_NZAxisMaterial = null;
	public float m_TextOffset = 2.9f;
	public Renderer m_mrXAxis;
	public Renderer m_mrYAxis;
	public Renderer m_mrZAxis;
	public Renderer m_mrNXAxis;
	public Renderer m_mrNYAxis;
	public Renderer m_mrNZAxis;
	public Renderer m_mrOrigin;
	
	public Rect m_RenderRect = new Rect (0,0,1,1);
	
	private Ray m_MouseRay;
	private float m_alpha_pos_x = 1.0f;
	private float m_alpha_pos_y = 1.0f;
	private float m_alpha_pos_z = 1.0f;
	private float m_alpha_neg_x = 1.0f;
	private float m_alpha_neg_y = 1.0f;
	private float m_alpha_neg_z = 1.0f;
	private float m_alpha_origin = 1.0f;
	private float m_alpha_threshold = 0.2f;
	
	private VCECamera m_MainCameraBehaviour;
	
	// x,y,z Text
	bool xShow, yShow, zShow;
	Vector3 xpoint, ypoint, zpoint;

	public static float AxisAlpha( float angle )
	{
		float retval = angle / 25.0f;
		if ( retval > 1 )
			retval = 1;
		retval = retval * retval;
		return retval;
	}
	
	void Start ()
	{
		m_MainCameraBehaviour = VCEditor.Instance.m_MainCamera.GetComponent<VCECamera>();
		m_FocusMaterial = Material.Instantiate(m_FocusMaterial) as Material;
		m_OriginMaterial = Material.Instantiate(m_OriginMaterial) as Material;
		m_XAxisMaterial = Material.Instantiate(m_XAxisMaterial) as Material;
		m_YAxisMaterial = Material.Instantiate(m_YAxisMaterial) as Material;
		m_ZAxisMaterial = Material.Instantiate(m_ZAxisMaterial) as Material;
		m_NXAxisMaterial = Material.Instantiate(m_NXAxisMaterial) as Material;
		m_NYAxisMaterial = Material.Instantiate(m_NYAxisMaterial) as Material;
		m_NZAxisMaterial = Material.Instantiate(m_NZAxisMaterial) as Material;
	}
	
	// Update is called once per frame
	void LateUpdate ()
	{
		if ( !VCEditor.DocumentOpen() )
			return;
		
		GetComponent<Camera>().pixelRect = m_RenderRect;
		
		// Update view matrix
		float AxisCamDist = 7.5f;
		GetComponent<Camera>().transform.localPosition = Vector3.zero;
		GetComponent<Camera>().transform.localRotation = m_MainCameraBehaviour.transform.localRotation;
		GetComponent<Camera>().transform.localPosition -= GetComponent<Camera>().transform.forward * AxisCamDist;
		
		// Set axis transparent
		Vector3 sight = GetComponent<Camera>().transform.forward;
		
		m_alpha_pos_x = AxisAlpha( Vector3.Angle(sight, -Vector3.right) );
		m_alpha_pos_y = AxisAlpha( Vector3.Angle(sight, -Vector3.up) );
		m_alpha_pos_z = AxisAlpha( Vector3.Angle(sight, -Vector3.forward ) );
		m_alpha_neg_x = AxisAlpha( Vector3.Angle(sight, Vector3.right) );
		m_alpha_neg_y = AxisAlpha( Vector3.Angle(sight, Vector3.up) );
		m_alpha_neg_z = AxisAlpha( Vector3.Angle(sight, Vector3.forward) );
		m_alpha_origin = 1.0f;
				
		m_XAxisMaterial.color  = new Color (m_XAxisMaterial.color.r,  m_XAxisMaterial.color.g,  m_XAxisMaterial.color.b,  m_alpha_pos_x);
		m_YAxisMaterial.color  = new Color (m_YAxisMaterial.color.r,  m_YAxisMaterial.color.g,  m_YAxisMaterial.color.b,  m_alpha_pos_y);
		m_ZAxisMaterial.color  = new Color (m_ZAxisMaterial.color.r,  m_ZAxisMaterial.color.g,  m_ZAxisMaterial.color.b,  m_alpha_pos_z);
		m_NXAxisMaterial.color = new Color (m_NXAxisMaterial.color.r, m_NXAxisMaterial.color.g, m_NXAxisMaterial.color.b, m_alpha_neg_x);
		m_NYAxisMaterial.color = new Color (m_NYAxisMaterial.color.r, m_NYAxisMaterial.color.g, m_NYAxisMaterial.color.b, m_alpha_neg_y);
		m_NZAxisMaterial.color = new Color (m_NZAxisMaterial.color.r, m_NZAxisMaterial.color.g, m_NZAxisMaterial.color.b, m_alpha_neg_z);
		m_OriginMaterial.color = new Color (m_OriginMaterial.color.r, m_OriginMaterial.color.g, m_OriginMaterial.color.b, m_alpha_origin);
		
		// Pick axis
		m_MouseRay = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
		Renderer focus = null;
		float MinDist = 1000.0f;
		float DestYaw = 0.0f;
		float DestPitch = 0.0f;
		
		m_mrXAxis.material = m_XAxisMaterial;
		m_mrYAxis.material = m_YAxisMaterial;
		m_mrZAxis.material = m_ZAxisMaterial;
		m_mrNXAxis.material = m_NXAxisMaterial;
		m_mrNYAxis.material = m_NYAxisMaterial;
		m_mrNZAxis.material = m_NZAxisMaterial;
		m_mrOrigin.material = m_OriginMaterial;
		RaycastHit rchit;
		if ( m_alpha_pos_x > m_alpha_threshold && m_mrXAxis.GetComponent<Collider>().Raycast( m_MouseRay, out rchit, 100 ) )
		{
			if ( rchit.distance < MinDist )
			{
				MinDist = rchit.distance;
				focus = m_mrXAxis;
				DestYaw = 0.0f;
				DestPitch = 0.0f;
				VCEStatusBar.ShowText("Right view".ToLocalizationString(), 2);
			}
		}
		if ( m_alpha_pos_y > m_alpha_threshold && m_mrYAxis.GetComponent<Collider>().Raycast( m_MouseRay, out rchit, 100 ) )
		{
			if ( rchit.distance < MinDist )
			{
				MinDist = rchit.distance;
				focus = m_mrYAxis;
				DestYaw = 90.0f;
				DestPitch = 90.0f;
				VCEStatusBar.ShowText("Top view".ToLocalizationString(), 2);
			}
		}
		if ( m_alpha_pos_z > m_alpha_threshold && m_mrZAxis.GetComponent<Collider>().Raycast( m_MouseRay, out rchit, 100 ) )
		{
			if ( rchit.distance < MinDist )
			{
				MinDist = rchit.distance;
				focus = m_mrZAxis;
				DestYaw = 90.0f;
				DestPitch = 0.0f;
				VCEStatusBar.ShowText("Front view".ToLocalizationString(), 2);
			}
		}
		if ( m_alpha_neg_x > m_alpha_threshold && m_mrNXAxis.GetComponent<Collider>().Raycast( m_MouseRay, out rchit, 100 ) )
		{
			if ( rchit.distance < MinDist )
			{
				MinDist = rchit.distance;
				focus = m_mrNXAxis;
				DestYaw = 180.0f;
				DestPitch = 0.0f;
				VCEStatusBar.ShowText("Left view".ToLocalizationString(), 2);
			}
		}
		if ( m_alpha_neg_y > m_alpha_threshold && m_mrNYAxis.GetComponent<Collider>().Raycast( m_MouseRay, out rchit, 100 ) )
		{
			if ( rchit.distance < MinDist )
			{
				MinDist = rchit.distance;
				focus = m_mrNYAxis;
				DestYaw = 90.0f;
				DestPitch = -90.0f;
				VCEStatusBar.ShowText("Bottom view".ToLocalizationString(), 2);
			}
		}
		if ( m_alpha_neg_z > m_alpha_threshold && m_mrNZAxis.GetComponent<Collider>().Raycast( m_MouseRay, out rchit, 100 ) )
		{
			if ( rchit.distance < MinDist )
			{
				MinDist = rchit.distance;
				focus = m_mrNZAxis;
				DestYaw = -90.0f;
				DestPitch = 0.0f;
				VCEStatusBar.ShowText("Back view".ToLocalizationString(), 2);
			}
		}
		if ( m_mrOrigin.GetComponent<Collider>().Raycast( m_MouseRay, out rchit, 100 ) )
		{
			if ( rchit.distance < MinDist )
			{
				MinDist = rchit.distance;
				focus = m_mrOrigin;
				DestYaw = m_MainCameraBehaviour.BeginYaw;
				DestPitch = m_MainCameraBehaviour.BeginPitch;
				VCEStatusBar.ShowText("Reset camera".ToLocalizationString(), 2);
			}
		}
		
		// Click
		if ( MinDist < 999 )
		{
			focus.material = m_FocusMaterial;
			if ( Input.GetMouseButtonDown(0) )
			{
				while ( DestYaw < m_MainCameraBehaviour.Yaw - 180.0f )
					DestYaw += 360.0f;
				while ( DestYaw > m_MainCameraBehaviour.Yaw + 180.0f )
					DestYaw -= 360.0f;
				while ( DestPitch < m_MainCameraBehaviour.Pitch - 180.0f )
					DestPitch += 360.0f;
				while ( DestPitch > m_MainCameraBehaviour.Pitch + 180.0f )
					DestPitch -= 360.0f;
				m_MainCameraBehaviour.SetYaw( DestYaw );
				m_MainCameraBehaviour.SetPitch( DestPitch );
			}
		}
		
		// x,y,z Text calculate
		xpoint = GetComponent<Camera>().WorldToScreenPoint( Vector3.right*m_TextOffset + transform.parent.position );
		ypoint = GetComponent<Camera>().WorldToScreenPoint( Vector3.up*m_TextOffset + transform.parent.position );
		zpoint = GetComponent<Camera>().WorldToScreenPoint( Vector3.forward*m_TextOffset + transform.parent.position );
		
		Ray xRay = GetComponent<Camera>().ScreenPointToRay(xpoint);
		Ray yRay = GetComponent<Camera>().ScreenPointToRay(ypoint);
		Ray zRay = GetComponent<Camera>().ScreenPointToRay(zpoint);
		
		xShow = true;
		yShow = true;
		zShow = true;
		if ( m_mrXAxis.GetComponent<Collider>().Raycast( xRay, out rchit, 100 ) )
			xShow = false;
		if ( m_mrYAxis.GetComponent<Collider>().Raycast( xRay, out rchit, 100 ) )
			xShow = false;
		if ( m_mrZAxis.GetComponent<Collider>().Raycast( xRay, out rchit, 100 ) )
			xShow = false;
		if ( m_mrNXAxis.GetComponent<Collider>().Raycast( xRay, out rchit, 100 ) )
			xShow = false;
		if ( m_mrNYAxis.GetComponent<Collider>().Raycast( xRay, out rchit, 100 ) )
			xShow = false;
		if ( m_mrNZAxis.GetComponent<Collider>().Raycast( xRay, out rchit, 100 ) )
			xShow = false;
		if ( m_mrOrigin.GetComponent<Collider>().Raycast( xRay, out rchit, 100 ) )
			xShow = false;
		
		if ( m_mrXAxis.GetComponent<Collider>().Raycast( yRay, out rchit, 100 ) )
			yShow = false;
		if ( m_mrYAxis.GetComponent<Collider>().Raycast( yRay, out rchit, 100 ) )
			yShow = false;
		if ( m_mrZAxis.GetComponent<Collider>().Raycast( yRay, out rchit, 100 ) )
			yShow = false;
		if ( m_mrNXAxis.GetComponent<Collider>().Raycast( yRay, out rchit, 100 ) )
			yShow = false;
		if ( m_mrNYAxis.GetComponent<Collider>().Raycast( yRay, out rchit, 100 ) )
			yShow = false;
		if ( m_mrNZAxis.GetComponent<Collider>().Raycast( yRay, out rchit, 100 ) )
			yShow = false;
		if ( m_mrOrigin.GetComponent<Collider>().Raycast( yRay, out rchit, 100 ) )
			yShow = false;
		
		if ( m_mrXAxis.GetComponent<Collider>().Raycast( zRay, out rchit, 100 ) )
			zShow = false;
		if ( m_mrYAxis.GetComponent<Collider>().Raycast( zRay, out rchit, 100 ) )
			zShow = false;
		if ( m_mrZAxis.GetComponent<Collider>().Raycast( zRay, out rchit, 100 ) )
			zShow = false;
		if ( m_mrNXAxis.GetComponent<Collider>().Raycast( zRay, out rchit, 100 ) )
			zShow = false;
		if ( m_mrNYAxis.GetComponent<Collider>().Raycast( zRay, out rchit, 100 ) )
			zShow = false;
		if ( m_mrNZAxis.GetComponent<Collider>().Raycast( zRay, out rchit, 100 ) )
			zShow = false;
		if ( m_mrOrigin.GetComponent<Collider>().Raycast( zRay, out rchit, 100 ) )
			zShow = false;
	}
	
	void OnGUI()
	{
		if ( !VCEditor.DocumentOpen() )
			return;
		
		GUI.skin = m_GUISkin;
		if ( m_alpha_pos_x > m_alpha_threshold && xShow )
			GUI.Label( new Rect(xpoint.x - 10 ,Screen.height - xpoint.y - 15, 20,20), "x", "AxisText" );
		if ( m_alpha_pos_y > m_alpha_threshold && yShow )
			GUI.Label( new Rect(ypoint.x - 10 ,Screen.height - ypoint.y - 15, 20,20), "y", "AxisText" );
		if ( m_alpha_pos_z > m_alpha_threshold && zShow )
			GUI.Label( new Rect(zpoint.x - 10 ,Screen.height - zpoint.y - 15, 20,20), "z", "AxisText" );
	}
}
