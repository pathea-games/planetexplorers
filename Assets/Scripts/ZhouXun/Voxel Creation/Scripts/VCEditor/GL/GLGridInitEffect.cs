using UnityEngine;
using System.Collections;

public class GLGridInitEffect : GLBehaviour
{
	private float m_Time;
	public float m_AnimationLife = 2f;
	public Vector3 m_CellSize = Vector3.one;
	public IntVector3 m_CellCount = IntVector3.One;
	public Color m_NormalColor;
	public Color m_FlowColor;
	public Color m_GlowColor;
	
	// Use this for initialization
	void Start ()
	{
		m_Time = 0;
	}
	
	void OnEnable ()
	{
		m_Time = 0;
	}
	
	// Update is called once per frame
	void Update ()
	{
		m_Time += Time.deltaTime;
	}
	
	public override void OnGL ()
	{
		for ( int x = 0; x < m_CellCount.x; ++x )
		{
			for ( int z = 0; z < m_CellCount.z; ++z )
			{
				DrawCell(x,z);
			}
		}
	}
	
	void DrawCell(int x, int z)
	{
		Vector3 begin, end;
		begin = new Vector3((x+0.02f) * m_CellSize.x, m_CellSize.y * 0.02f, (z+0.02f) * m_CellSize.z);
		end = new Vector3((x+0.98f) * m_CellSize.x, m_CellSize.y * 0.02f, (z+0.98f) * m_CellSize.z);
		float r = (m_Time/m_AnimationLife) * (new Vector2(m_CellCount.x, m_CellCount.z).magnitude);
		float dist = new Vector2(x - m_CellCount.x * 0.5f, z - m_CellCount.z * 0.5f).magnitude + (Mathf.Sin(x)+Mathf.Cos(z))*3;
		float brightdist = 5;
		float brightness = Mathf.Clamp01((brightdist - Mathf.Abs(dist-r))/brightdist);
		GL.Begin(GL.QUADS);
		GL.Color(QuadColor(brightness, dist < r));
		GL.Vertex3(begin.x, begin.y + brightness*m_CellSize.y, begin.z);
		GL.Vertex3(begin.x, begin.y, end.z);
		GL.Vertex3(end.x, begin.y + brightness*m_CellSize.y, end.z);
		GL.Vertex3(end.x, begin.y, begin.z);
		GL.End();
	}
	
	Color QuadColor(float brightness, bool inner)
	{
		if ( brightness > 0.5f )
		{
			return Color.Lerp(m_FlowColor, m_GlowColor, (brightness - 0.5f) * 2);
		}
		else
		{
			return Color.Lerp(inner ? m_NormalColor : Color.clear, m_FlowColor, brightness * 2);
		}
	}
}
