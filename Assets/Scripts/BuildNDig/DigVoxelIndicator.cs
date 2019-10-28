using UnityEngine;
using System.Collections;

public class DigVoxelIndicator : MonoBehaviour 
{
	public static IntVector3 InvalidPos { get { return new IntVector3 (-100,-100,-100); } }
	public IntVector3 m_Center = InvalidPos;
	public float m_Radius = 0.3f;
	public int m_AddHeight = 0;
	public Gradient m_ActiveColors;
	public Gradient m_UnActiveColors;
	
	//Material _LineMaterial = null;
	
	public bool show = false;
	public bool disEnable{ get; set; }
	
	bool m_FindVoxel;
	public Vector3 digPos{ get { return m_DTar.snapto; } }
	public bool active{ get{ return disEnable && m_FindVoxel; } }
	
	int minVol = 1;//BSMath.MC_ISO_VALUE;
	public int volAdder = 10;
	public float m_Contrast = 0.75f;

	BSMath.DrawTarget m_DTar;

	float shrink = 0.2f;

	ViewBounds mViewBounds;

	public Bounds bounds { get { Vector3 size = 2 * (m_Radius + shrink) * Vector3.one; size.y = 1; return new Bounds(m_Center, size); } }
//	void Start ()
//	{
//		GlobalGLs.AddGL(this);
//	}

	void OnDestroy ()
	{
		if(null != mViewBounds)
			ViewBoundsMgr.Instance.Recycle (mViewBounds);
//		GlobalGLs.RemoveGL(this);
	}

	void GetCenterVoxel()
	{
		for(int vol = minVol; vol < BSMath.MC_ISO_VALUE + volAdder; vol += volAdder)
		{
			if(BSMath.RayCastDrawTarget(PeCamera.mouseRay, BuildingMan.Voxels,  out m_DTar,  vol))
			{
				m_Center = new IntVector3(m_DTar.snapto);
				if(CheckVoxel(m_Center))
				{
					m_FindVoxel = true;
					return;
				}
			}
		}
		
		//checkOriginNear

		if(CheckVoxel(PeCamera.mouseRay.origin - BuildingMan.Voxels.Offset))
		{
			m_Center = new IntVector3(PeCamera.mouseRay.origin - BuildingMan.Voxels.Offset);
			m_FindVoxel = true;
			m_DTar = new BSMath.DrawTarget();
			m_DTar.snapto = m_Center;
			return;
		}

		m_FindVoxel = false;
		m_Center = InvalidPos;
	}

	bool CheckVoxel(IntVector3 pos)
	{		
		VFVoxel voxel = VFVoxelTerrain.self.Voxels.SafeRead(pos.x, pos.y, pos.z, 0);
		if(voxel.Volume < BSMath.MC_ISO_VALUE)
		{
			for(int axis = 0; axis < 3; axis++)
			{
				for(int adder = -1; adder <= 1; adder += 2)
				{
					IntVector3 borderPos = pos + new IntVector3(adder * ((axis==0)?1:0), adder * ((axis==1)?1:0), adder * ((axis==2)?1:0));
					VFVoxel borderVoxel = VFVoxelTerrain.self.Voxels.SafeRead(borderPos.x, borderPos.y, borderPos.z, 0);
					if(borderVoxel.Volume >= BSMath.MC_ISO_VALUE)
					{
						return true;
					}
				}
			}
		}
		else
		{
			return true;
		}

		return false;
	}
	
	void Update ()
	{	
		if (show && enabled && !BSInput.s_MouseOnUI) 
		{
			GetCenterVoxel ();
			UpdateViewBounds ();
		}
		else
		{
			if(null != mViewBounds)
			{
				ViewBoundsMgr.Instance.Recycle(mViewBounds);
				mViewBounds = null;
			}
		}
	}

	void UpdateViewBounds()
	{
		if (null == mViewBounds)
			mViewBounds = ViewBoundsMgr.Instance.Get ();
		
		float minBorder = Mathf.RoundToInt(m_Radius);
		float maxBorder = -minBorder + Mathf.FloorToInt(m_Radius * 2f);
		Vector3 min = new Vector3(m_Center.x - minBorder - 0.5f, m_Center.y - 0.5f, m_Center.z - minBorder - 0.5f);
		Vector3 max = new Vector3(m_Center.x + maxBorder + 0.5f, m_Center.y + m_AddHeight + 0.5f, m_Center.z + maxBorder + 0.5f);

		min -= shrink * Vector3.one;
		max += shrink * Vector3.one;
		mViewBounds.SetSize (max - min);
		mViewBounds.SetPos (min);
		mViewBounds.SetColor (active ? m_ActiveColors.Evaluate(1f) : m_UnActiveColors.Evaluate(1f));
	}
	
//	void OnDisable()
//	{
//		m_Center = InvalidPos;
//	}
//	
//	public override void OnGL ()
//	{
//		if(!show)
//			return;
//
//		if (!enabled || !gameObject.activeInHierarchy)
//			return;
//		
//		if (BSInput.s_MouseOnUI)
//			return;
//		
//		if (_LineMaterial == null)
//		{
//			_LineMaterial = new Material(Shader.Find("Lines/Colored Blended"));
//			_LineMaterial.hideFlags = HideFlags.HideAndDontSave;
//			_LineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
//		}
//		
//		GL.PushMatrix();
//		
//		_LineMaterial.SetPass(0);
//
//		Bounds box = new Bounds (Vector3.zero, Vector3.zero);
//		float minBorder = Mathf.RoundToInt(m_Radius);
//		float maxBorder = -minBorder + Mathf.FloorToInt(m_Radius * 2f);
//		Vector3 min = new Vector3(m_Center.x - minBorder - 0.5f, m_Center.y - 0.5f, m_Center.z - minBorder - 0.5f);
//		Vector3 max = new Vector3(m_Center.x + maxBorder + 0.5f, m_Center.y + m_AddHeight + 0.5f, m_Center.z + maxBorder + 0.5f);
//
//		for(int i = 0; i < 2; i++)
//		{
//			box.SetMinMax(min - i * Vector3.one, max + i * Vector3.one);
//			Color lc = active ? m_ActiveColors.Evaluate(1f - i) : m_UnActiveColors.Evaluate(1f - i);
//			lc.a *= ((float)GameTime.Timer.CycleInDay + 1f) * 0.5f * 0.45f + 0.55f;
//			Color bc = lc;
//			lc.a *= 10f;
//
//			float shrink = 0.04f;
//			if (Camera.main != null)
//			{
//				float dist_min = (Camera.main.transform.position - box.min).magnitude;
//				float dist_max = (Camera.main.transform.position - box.max).magnitude;
//				
//				dist_max = dist_max > dist_min ? dist_max : dist_min;
//				
//				shrink = Mathf.Clamp(dist_max * 0.001f, 0.04f, 0.2f);
//			}
//			
//			box.min -= new Vector3(shrink, shrink, shrink);
//			box.max += new Vector3(shrink, shrink, shrink);
//			
//			// Edge
//			GL.Begin(GL.LINES);
//			GL.Color(lc);
//			GL.Vertex3( box.min.x, box.min.y, box.min.z );
//			GL.Vertex3( box.max.x, box.min.y, box.min.z );
//			GL.Vertex3( box.min.x, box.min.y, box.max.z );
//			GL.Vertex3( box.max.x, box.min.y, box.max.z );
//			GL.Vertex3( box.min.x, box.max.y, box.min.z );
//			GL.Vertex3( box.max.x, box.max.y, box.min.z );
//			GL.Vertex3( box.min.x, box.max.y, box.max.z );
//			GL.Vertex3( box.max.x, box.max.y, box.max.z );
//			
//			GL.Vertex3( box.min.x, box.min.y, box.min.z );
//			GL.Vertex3( box.min.x, box.max.y, box.min.z );
//			GL.Vertex3( box.min.x, box.min.y, box.max.z );
//			GL.Vertex3( box.min.x, box.max.y, box.max.z );
//			GL.Vertex3( box.max.x, box.min.y, box.min.z );
//			GL.Vertex3( box.max.x, box.max.y, box.min.z );
//			GL.Vertex3( box.max.x, box.min.y, box.max.z );
//			GL.Vertex3( box.max.x, box.max.y, box.max.z );
//			
//			GL.Vertex3( box.min.x, box.min.y, box.min.z );
//			GL.Vertex3( box.min.x, box.min.y, box.max.z );
//			GL.Vertex3( box.min.x, box.max.y, box.min.z );
//			GL.Vertex3( box.min.x, box.max.y, box.max.z );
//			GL.Vertex3( box.max.x, box.min.y, box.min.z );
//			GL.Vertex3( box.max.x, box.min.y, box.max.z );
//			GL.Vertex3( box.max.x, box.max.y, box.min.z );
//			GL.Vertex3( box.max.x, box.max.y, box.max.z );
//			GL.End();
//			
//			// Face
//			GL.Begin(GL.QUADS);
//			GL.Color(bc);
//			GL.Vertex3( box.min.x, box.min.y, box.min.z );
//			GL.Vertex3( box.min.x, box.min.y, box.max.z );
//			GL.Vertex3( box.min.x, box.max.y, box.max.z );
//			GL.Vertex3( box.min.x, box.max.y, box.min.z );
//			
//			GL.Vertex3( box.max.x, box.min.y, box.min.z );
//			GL.Vertex3( box.max.x, box.min.y, box.max.z );
//			GL.Vertex3( box.max.x, box.max.y, box.max.z );
//			GL.Vertex3( box.max.x, box.max.y, box.min.z );
//			
//			GL.Vertex3( box.min.x, box.min.y, box.min.z );
//			GL.Vertex3( box.min.x, box.min.y, box.max.z );
//			GL.Vertex3( box.max.x, box.min.y, box.max.z );
//			GL.Vertex3( box.max.x, box.min.y, box.min.z );
//			
//			GL.Vertex3( box.min.x, box.max.y, box.min.z );
//			GL.Vertex3( box.min.x, box.max.y, box.max.z );
//			GL.Vertex3( box.max.x, box.max.y, box.max.z );
//			GL.Vertex3( box.max.x, box.max.y, box.min.z );
//			
//			GL.Vertex3( box.min.x, box.min.y, box.min.z );
//			GL.Vertex3( box.min.x, box.max.y, box.min.z );
//			GL.Vertex3( box.max.x, box.max.y, box.min.z );
//			GL.Vertex3( box.max.x, box.min.y, box.min.z );
//			
//			GL.Vertex3( box.min.x, box.min.y, box.max.z );
//			GL.Vertex3( box.min.x, box.max.y, box.max.z );
//			GL.Vertex3( box.max.x, box.max.y, box.max.z );
//			GL.Vertex3( box.max.x, box.min.y, box.max.z );
//			GL.End();
//		}
//		
//		GL.PopMatrix();
//	}
}
