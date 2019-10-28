//#define TEST_CODE


using UnityEngine;
using System.Collections;

/// <summary>
/// Building System manipulator
/// </summary>

public class BuildingMan : MonoBehaviour 
{
	public enum EBrushType 
	{
		None,
		Point,
		Box,
		Select,
		Iso,
		B45Diagonal,
		IsoSelectBrush
	}

	private EBrushType m_BrushType = EBrushType.None;
	public EBrushType BrushType { get { return m_BrushType;} }

	public delegate void DCreateBrushNotify(BSBrush brush, EBrushType type);
	public event  DCreateBrushNotify onCreateBrush;

	private static BuildingMan _self = null;
	public static BuildingMan Self 	 { get { return _self;} } 

	public static string s_IsoPath = "/PlanetExplorers/BuildingIso/";
	
	public static string s_IsoExt = ".biso";

	private IBSDataSource m_ActiveData = null;
	public IBSDataSource ActiveData { get { return m_ActiveData;} }

	#region Data_Source
	public static BSVoxelData 		Voxels   { get { return Datas[0] as BSVoxelData; }}
	public static BSBlock45Data		Blocks   { get { return Datas[1] as BSBlock45Data; }}

	public static IBSDataSource[]  Datas  = new IBSDataSource[2]{ 
		new BSVoxelData(), new BSBlock45Data()
	}; 
	#endregion

	// Material Type 
	public byte MaterialType = 2;

	// Indcators 
	public BSGLNearVoxelIndicator voxelIndicator;
	public BSGLNearBlockIndicator blockIndicator;

	// Brush will stay this Group
	public GameObject brushGroup;

	// Global UI Skin
	public GUISkin guiSkin;
	
	private BSBrush m_ActiveBrush = null;
	public BSBrush activeBrush { get { return m_ActiveBrush;}}	

	#region Brush_Prefab

	[System.Serializable]
	public class CBrushRes
	{
		public GameObject pointBrush;
		public GameObject boxBrush;
		public GameObject SelectBrush;
		public GameObject IsoBrush;
		public GameObject diagonalBrush;
		public GameObject IsoSelectBrush;
	}

	[SerializeField] CBrushRes brushPrefabs;
	

	#endregion

	public BSPattern pattern;

	public Material patternMeshMat = null;

#if TEST_CODE

	[SerializeField] bool _genPointBrush = false;
	[SerializeField] bool _genBoxBrush = false;
	[SerializeField] bool _genSelectBrush = false;
	[SerializeField] bool _genIsoBrush = false;
	[SerializeField] bool _genDiagonalBrush = false;
	[SerializeField] bool _clear = false;
	
#endif


	/// <summary>
	/// Creates the specific brush.
	/// </summary>
	public static BSBrush CreateBrush (EBrushType type)
	{
		if (Self == null)
		{
			Debug.LogError("Building manipulator");
			return null;
		}

		if (type == Self.m_BrushType)
			return Self.m_ActiveBrush;

		if (Self.m_ActiveBrush != null )
		{
			Destroy(Self.m_ActiveBrush.gameObject);
		}

		BSBrush brush = null;

		switch (type)
		{
		case EBrushType.Point:
			brush = BSBrush.Create<BSPointBrush>(Self.brushPrefabs.pointBrush, Self.brushGroup.transform);
			break;
		case EBrushType.Box:
			brush = BSBrush.Create<BSBoxBrush>(Self.brushPrefabs.boxBrush, Self.brushGroup.transform);
			break;
		case EBrushType.Select:
			brush = BSBrush.Create<BSMiscBrush>(Self.brushPrefabs.SelectBrush, Self.brushGroup.transform);
			break;
		case EBrushType.Iso:
			brush = BSBrush.Create<BSIsoBrush>(Self.brushPrefabs.IsoBrush, Self.brushGroup.transform);
			break;
		case EBrushType.B45Diagonal:
			brush = BSBrush.Create<BSB45DiagonalBrush>(Self.brushPrefabs.diagonalBrush, Self.brushGroup.transform);
			break;
		case EBrushType.IsoSelectBrush:
			brush = BSBrush.Create<BSIsoSelectBrush>(Self.brushPrefabs.IsoSelectBrush, Self.brushGroup.transform);
			break;
		default:
			break;
		}
		Self.m_BrushType = type;

		if (brush == null)
			return null;

		Self.m_ActiveBrush = brush;

		brush.pattern = Self.pattern;
		brush.dataSource = Self.m_ActiveData;
		Self.voxelIndicator.minVol = brush.minvol;

		if (Self.onCreateBrush != null)
			Self.onCreateBrush (brush, type);

		return brush;
	}


	public static void Clear ()
	{
		if (Self.m_ActiveBrush != null)
		{
			Destroy(Self.m_ActiveBrush.gameObject);
		}

		BSHistory.Clear();
	}

	#region Unity_Inner_Func

	void OnGUI ()
	{
//		if (GUI.Button(new Rect(200,200, 100, 35), "Brush"))
//		{
//			CreateBrush(EBrushType.IsoSelectBrush);
//		}
	}

	void Awake()
	{

		if (_self != null)
			Debug.LogWarning ("There is alread a Building manipulator ");
		else
			_self = this;

		patternMeshMat.renderQueue = 3000;
	}

	void OnDestroy()
	{
	}
	
	void Update()
	{

		#if TEST_CODE
		UpdateTest();
		#endif

		if (BSInput.s_Undo)
		{
			BSHistory.Undo();
		}
		else if (BSInput.s_Redo)
		{
			BSHistory.Redo();
		}

		// if null get the defualt pattern
		if (pattern == null)
		{
			pattern = BSPattern.DefaultB1;
		}

		IBSDataSource data = null;
		if (pattern.type == EBSVoxelType.Block)
		{
			data = Blocks;

			if ( Block45Man.self._b45Materials.Length > MaterialType)
				pattern.MeshMat = Block45Man.self._b45Materials[MaterialType];
			else
				pattern.MeshMat = Block45Man.self._b45Materials[0];
		}
		else if (pattern.type == EBSVoxelType.Voxel)
			data = Voxels;
		else
			data = null;
		
//		pattern.SetMaterial(MaterialType);
//		int expand = Mathf.Clamp(2 + (pattern.size - 2), 2, 3);
//		blockIndicator.m_Expand = expand;
//		voxelIndicator.m_Expand = expand;

		if (m_ActiveBrush != null)
		{
			voxelIndicator.minVol = m_ActiveBrush.minvol;

			m_ActiveBrush.dataSource = data;
			m_ActiveBrush.pattern = pattern;
			m_ActiveBrush.materialType = MaterialType;

			if (m_ActiveBrush.pattern.type == EBSVoxelType.Block)
			{
				blockIndicator.enabled = true;
				voxelIndicator.enabled = false;
			}
			else if (m_ActiveBrush.pattern.type == EBSVoxelType.Voxel)
			{
				blockIndicator.enabled = false;
				voxelIndicator.enabled = true;
			}
		}
		else
		{
			blockIndicator.enabled = false;
			voxelIndicator.enabled = false;
		}
		 
	}

	#endregion

#if TEST_CODE
	private void UpdateTest ()
	{	
		// Create Special Brush
		
		if (Input.GetKeyDown(KeyCode.F5))
			_genPointBrush = true;
		else if (Input.GetKeyDown(KeyCode.F6))
			_genBoxBrush = true;
		else if (Input.GetKeyDown(KeyCode.F7))
			_genSelectBrush = true;
		else if (Input.GetKeyDown(KeyCode.F4))
			_genIsoBrush = true;
		else if (Input.GetKeyDown(KeyCode.F3))
			_clear = true;
		else if (Input.GetKeyDown(KeyCode.F9))
			_genDiagonalBrush = true;
		
		if (_genPointBrush)
		{
			CreateBrush(EBrushType.Point);
			_genPointBrush = false;
		}
		
		if (_genBoxBrush)
		{
			CreateBrush(EBrushType.Box);
			_genBoxBrush = false;
		}
		
		if (_genSelectBrush)
		{
			CreateBrush(EBrushType.Select);
			_genSelectBrush = false;
		}
		
		if (_genIsoBrush)
		{
			CreateBrush(EBrushType.Iso);
			_genIsoBrush = false;

		}

		if (_genDiagonalBrush)
		{
			CreateBrush(EBrushType.B45Diagonal);
			_genDiagonalBrush = false;
		}

		if (_clear)
		{
			Clear();
			_clear = false;
		}
	}
#endif

}
