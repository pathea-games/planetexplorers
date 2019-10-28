using UnityEngine;
using System.Collections;


public class GLGridPlane : GLBehaviour
{
	public int m_MajorGridInterval = 10;
	public int m_MinorGridInterval = 5;
	public ECoordPlane m_CoordPlane = ECoordPlane.XZ;
	public Vector3 m_CellSize = Vector3.one;
	public IntVector3 m_CellCount = IntVector3.One;
	public Color m_PlaneColor;
	public Color m_MajorLineColor;
	public Color m_MinorLineColor;
	public Color m_CellLineColor;
	public bool m_ShowGrid = true;
    public bool m_Fdisk = false;
	public GameObject m_DirGroup;
	public GameObject m_DirFront;
	public GameObject m_DirBack;
	public GameObject m_DirLeft;
	public GameObject m_DirRight;
	public Projector m_LaserGrid;

	// Use this for initialization
	void Start ()
	{
		if ( m_LaserGrid != null )
			m_LaserGrid.material = Material.Instantiate(m_LaserGrid.material) as Material;
	}
	
	// Update is called once per frame
	void Update ()
	{
		BoxCollider bc = GetComponent<BoxCollider>();
		if ( bc != null )
		{
			Vector3 editorsize = new Vector3( m_CellSize.x * m_CellCount.x, m_CellSize.y * m_CellCount.y, m_CellSize.z * m_CellCount.z );
			Vector3 center = editorsize * 0.5f;
			Vector3 size = editorsize;
			if ( m_CoordPlane == ECoordPlane.XY )
			{
				center.z = 0;
				size.z = m_CellSize.z * 0.1f;
			}
			else if ( m_CoordPlane == ECoordPlane.XZ )
			{
				center.y = 0;
				size.y = m_CellSize.y * 0.1f;
			}
			else if ( m_CoordPlane == ECoordPlane.ZY )
			{
				center.x = 0;
				size.x = m_CellSize.x * 0.1f;
			}
			bc.center = center;
			bc.size = size;
		}
		if ( m_LaserGrid != null )
		{
			m_LaserGrid.orthographicSize = Mathf.Max(m_CellCount.x, m_CellCount.z) * 0.5f * m_CellSize.x;
			m_LaserGrid.nearClipPlane = 0;
			m_LaserGrid.farClipPlane = m_CellCount.y * m_CellSize.y;
			m_LaserGrid.transform.position = new Vector3( m_CellSize.x * m_CellCount.x * 0.5f, m_CellSize.y * m_CellCount.y, m_CellSize.z * m_CellCount.z * 0.5f );
			m_LaserGrid.material.SetFloat("_MajorGrid", m_MajorGridInterval*m_CellSize.x);
			m_LaserGrid.material.SetFloat("_MinorGrid", m_MinorGridInterval*m_CellSize.x);
			m_LaserGrid.material.SetFloat("_CellSize", m_CellSize.x);
			m_LaserGrid.gameObject.SetActive(VCEditor.Instance.m_UI.m_ShowLaserGrid && VCEditor.Instance.m_UI.m_MaterialTab.isChecked);
		}
		UpdateDirGroup();
	}

	void OnEnable ()
	{
		UpdateDirGroup();
	}

	public void UpdateDirGroup ()
	{
		if ( m_DirGroup != null )
		{
			m_DirGroup.transform.localScale = m_CellSize;
			m_DirFront.transform.localPosition = new Vector3(m_CellCount.x * 0.5f, 0, m_CellCount.z + 3);
			m_DirBack.transform.localPosition = new Vector3(m_CellCount.x * 0.5f, 0, -3);
			m_DirLeft.transform.localPosition = new Vector3(-3, 0, m_CellCount.z * 0.5f);
			m_DirRight.transform.localPosition = new Vector3(m_CellCount.x +3, 0, m_CellCount.z * 0.5f);
			m_DirGroup.SetActive(!VCEditor.Instance.m_UI.m_ISOTab.isChecked);

		}
	}
	
	public override void OnGL ()
	{
		Vector3 grid_size = new Vector3(m_CellSize.x * m_CellCount.x, m_CellSize.y * m_CellCount.y, m_CellSize.z * m_CellCount.z);
		Vector3 grid_begin = transform.position;
		Vector3 grid_end = grid_begin + grid_size;

        if (m_Fdisk)
        {
            float fvalue = m_CellSize.x *5;
            float fd = 0.5f;
            GL.Begin(GL.QUADS);
            GL.Color(m_PlaneColor);
            switch (m_CoordPlane)
            {
                case ECoordPlane.XZ:
                    {
                        GL.Vertex3(grid_begin.x, grid_begin.y, grid_begin.z);
                        GL.Vertex3(fd * grid_end.x -fvalue, grid_begin.y, grid_begin.z);
                        GL.Vertex3(fd * grid_end.x - fvalue, grid_begin.y, grid_end.z);
                        GL.Vertex3(grid_begin.x, grid_begin.y, grid_end.z);
                        break;
                    }
            }
            GL.End();

            GL.Begin(GL.QUADS);
            GL.Color(m_PlaneColor);
            switch (m_CoordPlane)
            {
                case ECoordPlane.XZ:
                    {
                        GL.Vertex3(fd * grid_end.x + fvalue, grid_begin.y, grid_begin.z);
                        GL.Vertex3(grid_end.x, grid_begin.y, grid_begin.z);
                        GL.Vertex3(grid_end.x, grid_begin.y, grid_end.z);
                        GL.Vertex3(fd * grid_end.x + fvalue, grid_begin.y, grid_end.z);
                        break;
                    }
            }
            GL.End();

        }
        else
        {
            GL.Begin(GL.QUADS);
            GL.Color(m_PlaneColor);
            switch (m_CoordPlane)
            {
                case ECoordPlane.XZ:
                    {
                        GL.Vertex3(grid_begin.x, grid_begin.y, grid_begin.z);
                        GL.Vertex3(grid_end.x, grid_begin.y, grid_begin.z);
                        GL.Vertex3(grid_end.x, grid_begin.y, grid_end.z);
                        GL.Vertex3(grid_begin.x, grid_begin.y, grid_end.z);
                        break;
                    }
                case ECoordPlane.XY:
                    {
                        GL.Vertex3(grid_begin.x, grid_begin.y, grid_begin.z);
                        GL.Vertex3(grid_end.x, grid_begin.y, grid_begin.z);
                        GL.Vertex3(grid_end.x, grid_end.y, grid_begin.z);
                        GL.Vertex3(grid_begin.x, grid_end.y, grid_begin.z);
                        break;
                    }
                case ECoordPlane.ZY:
                    {
                        GL.Vertex3(grid_begin.x, grid_begin.y, grid_begin.z);
                        GL.Vertex3(grid_begin.x, grid_begin.y, grid_end.z);
                        GL.Vertex3(grid_begin.x, grid_end.y, grid_end.z);
                        GL.Vertex3(grid_begin.x, grid_end.y, grid_begin.z);
                        break;
                    }
            }
            GL.End();
        }
		
		
		if ( m_ShowGrid )
		{
			GL.Begin(GL.LINES);
			switch (m_CoordPlane)
			{
			case ECoordPlane.XZ:
			{
                if(m_Fdisk)
                {
                    float fvalue = m_CellSize.x * 5;
                    float fd = 0.5f;

                    //x
                    for (int i = 0; i <= Mathf.CeilToInt(m_CellCount.x * 0.5f - 500 * m_CellSize.x); ++i)
                    {
                        if (i % m_MajorGridInterval == 0)
                            GL.Color(m_MajorLineColor);
                        else if (i % m_MinorGridInterval == 0)
                            GL.Color(m_MinorLineColor);
                        else if (Mathf.CeilToInt(m_CellCount.x * 0.5f - 500 * m_CellSize.x) == i)
                            GL.Color(m_MajorLineColor);
                        else
                            GL.Color(m_CellLineColor);

                        float pos = grid_begin.x + i * m_CellSize.x;
                        GL.Vertex3(pos, grid_begin.y, grid_begin.z);
                        GL.Vertex3(pos, grid_begin.y, grid_end.z);
                    }

                    for (int i = Mathf.CeilToInt(m_CellCount.x * 0.5f + 500 * m_CellSize.x); i <= m_CellCount.x; ++i)
                    {
                        if ((i - Mathf.CeilToInt(m_CellCount.x * 0.5f + 500 * m_CellSize.x)) % m_MajorGridInterval == 0)
                            GL.Color(m_MajorLineColor);
                        else if ((i - Mathf.CeilToInt(m_CellCount.x * 0.5f + 500 * m_CellSize.x)) % m_MinorGridInterval == 0)
                            GL.Color(m_MinorLineColor);
                        else
                            GL.Color(m_CellLineColor);

                        float pos = grid_begin.x + i * m_CellSize.x;
                        GL.Vertex3(pos, grid_begin.y, grid_begin.z);
                        GL.Vertex3(pos, grid_begin.y, grid_end.z);
                    }

                    //z
                    for (int i = 0; i <= m_CellCount.z; ++i)
                    {
                        if (i % m_MajorGridInterval == 0)
                            GL.Color(m_MajorLineColor);
                        else if (i % m_MinorGridInterval == 0)
                            GL.Color(m_MinorLineColor);
                        else
                            GL.Color(m_CellLineColor);

                        float pos = grid_begin.z + i * m_CellSize.z;
                        GL.Vertex3(grid_begin.x, grid_begin.y, pos);
                        GL.Vertex3(fd * grid_end.x - fvalue, grid_begin.y, pos);

                        GL.Vertex3(fd * grid_end.x + fvalue, grid_begin.y, pos);
                        GL.Vertex3(grid_end.x, grid_begin.y, pos);
                    }
                    break;

                }
                else
                {
                    for (int i = 0; i <= m_CellCount.x; ++i)
                    {
                        if (i % m_MajorGridInterval == 0)
                            GL.Color(m_MajorLineColor);
                        else if (i % m_MinorGridInterval == 0)
                            GL.Color(m_MinorLineColor);
                        else
                            GL.Color(m_CellLineColor);

                        float pos = grid_begin.x + i * m_CellSize.x;
                        GL.Vertex3(pos, grid_begin.y, grid_begin.z);
                        GL.Vertex3(pos, grid_begin.y, grid_end.z);
                    }
                }
				
				for ( int i = 0; i <= m_CellCount.z; ++i )
				{
					if ( i % m_MajorGridInterval == 0 )
						GL.Color(m_MajorLineColor);
					else if ( i % m_MinorGridInterval == 0 )
						GL.Color(m_MinorLineColor);
					else
						GL.Color(m_CellLineColor);
					
					float pos = grid_begin.z + i*m_CellSize.z;
					GL.Vertex3(grid_begin.x, grid_begin.y, pos);
					GL.Vertex3(grid_end.x, grid_begin.y, pos);
				}
				break;
			}
			case ECoordPlane.XY:
			{
				for ( int i = 0; i <= m_CellCount.x; ++i )
				{
					if ( i % m_MajorGridInterval == 0 )
						GL.Color(m_MajorLineColor);
					else if ( i % m_MinorGridInterval == 0 )
						GL.Color(m_MinorLineColor);
					else
						GL.Color(m_CellLineColor);
					
					float pos = grid_begin.x + i*m_CellSize.x;
					GL.Vertex3(pos, grid_begin.y, grid_begin.z);
					GL.Vertex3(pos, grid_end.y, grid_begin.z);
				}
				for ( int i = 0; i <= m_CellCount.y; ++i )
				{
					if ( i % m_MajorGridInterval == 0 )
						GL.Color(m_MajorLineColor);
					else if ( i % m_MinorGridInterval == 0 )
						GL.Color(m_MinorLineColor);
					else
						GL.Color(m_CellLineColor);
					
					float pos = grid_begin.y + i*m_CellSize.y;
					GL.Vertex3(grid_begin.x, pos, grid_begin.z);
					GL.Vertex3(grid_end.x, pos, grid_begin.z);
				}
				break;
			}
			case ECoordPlane.ZY:
			{
				for ( int i = 0; i <= m_CellCount.z; ++i )
				{
					if ( i % m_MajorGridInterval == 0 )
						GL.Color(m_MajorLineColor);
					else if ( i % m_MinorGridInterval == 0 )
						GL.Color(m_MinorLineColor);
					else
						GL.Color(m_CellLineColor);
					
					float pos = grid_begin.z + i*m_CellSize.z;
					GL.Vertex3(grid_begin.x, grid_begin.y, pos);
					GL.Vertex3(grid_begin.x, grid_end.y, pos);
				}
				for ( int i = 0; i <= m_CellCount.y; ++i )
				{
					if ( i % m_MajorGridInterval == 0 )
						GL.Color(m_MajorLineColor);
					else if ( i % m_MinorGridInterval == 0 )
						GL.Color(m_MinorLineColor);
					else
						GL.Color(m_CellLineColor);
					
					float pos = grid_begin.y + i*m_CellSize.y;
					GL.Vertex3(grid_begin.x, pos, grid_begin.z);
					GL.Vertex3(grid_begin.x, pos, grid_end.z);
				}
				break;
			}
			}		
			GL.End();
		}
	}
}
