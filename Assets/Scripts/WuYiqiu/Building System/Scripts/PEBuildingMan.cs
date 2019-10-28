//#define TEST_CODE


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ItemAsset;
using ItemAsset.PackageHelper;

public class PEBuildingMan : MonoBehaviour 
{
	private static PEBuildingMan _self = null;
	public static PEBuildingMan Self 	 { get { return _self;} }

	[SerializeField] BuildingMan _manipulator;

	// Main Manipulator
	public BuildingMan Manipulator { get{ return _manipulator;} }

	// For the point, box brush 

	private BSPattern m_Pattern;

	public BSPattern Pattern
	{
		get
		{
			return _manipulator.pattern;
		}

		set
		{
			if (_manipulator.BrushType == BuildingMan.EBrushType.Point || 
			    _manipulator.BrushType == BuildingMan.EBrushType.Box ||
			    _manipulator.BrushType == BuildingMan.EBrushType.B45Diagonal)
			{
				m_Pattern = value;
				_manipulator.pattern = value;
			}

		}
	}
	

	public bool selectVoxel = true;

	public bool IsGod = false; 

	public Bounds brushBound
	{
		get
		{
			//if (Manipulator.activeBrush as )
			if (Manipulator.activeBrush == null)
				return new Bounds();
			return Manipulator.activeBrush.brushBound;
		}
	}


	[SerializeField] PEIsoCapture _IsoCapturePrefab;
	private PEIsoCapture m_IsoCaputure;
	public PEIsoCapture  IsoCaputure { get { return m_IsoCaputure;} }


	public delegate void DVoxelModifyNotify (IntVector3[] indexes, BSVoxel[] voxels, BSVoxel[] oldvoxels, EBSBrushMode mode, IBSDataSource d);
	public event DVoxelModifyNotify onVoxelMotify;

	#if TEST_CODE

	public enum EDefaultPattern
	{
		V1,
		V2,
		V3,
		B1,
		B2,
		B3,
		UserDefine1,
		UserDefine1_1,
		UserDefine1_2,
		UserDefine1_3,
		UserDefine1_4,
		UserDefine1_5,
		UserDefine2,
		UserDefine2_1,
		UserDefine2_2,
		UserDefine2_3,
		UserDefine2_4,
		UserDefine2_5,
		UserDefine4,
		UserDefine4_1,
		UserDefine4_2,
		UserDefine4_3,
		UserDefine4_4,
		UserDefine4_5
	}

	// Default Brush Type
	public EDefaultPattern defaultPattern = EDefaultPattern.V1;
	#endif
	

//	public const int c_MinItemProtoID = 268;
//	public const int c_MaxItemProtoID = 282;

	/// <summary>
	/// Gets the item ID by material index.
	/// </summary>
	public static int GetBlockItemProtoID(byte matIndex)
	{
//		return matIndex + c_MinItemProtoID;
		if (BSBlockMatMap.s_MatToItem.ContainsKey((int)matIndex))
			return BSBlockMatMap.s_MatToItem[(int)matIndex];
		else
			return -1;
	}

	public static int GetBlockMaterialType(int proto_id)
	{
//		return proto_id - c_MinItemProtoID;
		if (BSBlockMatMap.s_ItemToMat.ContainsKey(proto_id))
			return BSBlockMatMap.s_ItemToMat[proto_id];
		else
			return -1;
	}

	public static int GetVoxelItemProtoID(byte matIndex)
	{
		return BSVoxelMatMap.GetItemID(matIndex);
	}

	/// <summary>
	/// Extracts the all the headers of building isos.
	/// </summary>
	/// <returns>The the headers.</returns>
	public BSIsoHeadData[] ExtractTheHeaders()
	{
		string FilePath = GameConfig.GetUserDataPath() + BuildingMan.s_IsoPath;
		if (!Directory.Exists(FilePath))
			Directory.CreateDirectory(FilePath);

		string[] fileNames = Directory.GetFiles(FilePath);

		List<BSIsoHeadData> headers = new List<BSIsoHeadData>();
		foreach (string fn in fileNames)
		{
			if(fn.Contains(".biso"))
			{
				BSIsoHeadData h;
				BSIsoData.ExtractHeader(fn, out h);
				int start = fn.LastIndexOf('/') + 1;
				int end = fn.LastIndexOf('.');
				h.Name = fn.Substring(start, end - start);
				headers.Add(h);
			}
		}

		return headers.ToArray();
	}

	#region UNITY_INNER_FUNC

	void Awake()
	{
		
		if (_self != null)
			Debug.LogWarning ("There is alread a Building manipulator ");
		else
			_self = this;

		BSVoxelModify.onModifyCheck += OnCheckVoxelModify;

		_manipulator.onCreateBrush += OnCreateBrush;

		m_IsoCaputure = Instantiate(_IsoCapturePrefab) as PEIsoCapture;
		m_IsoCaputure.transform.parent = transform;
		m_IsoCaputure.transform.localPosition = Vector3.zero;
		m_IsoCaputure.transform.localRotation = Quaternion.identity;
	}

	void OnDestroy()
	{
		BSVoxelModify.onModifyCheck -= OnCheckVoxelModify;
		_manipulator.onCreateBrush -= OnCreateBrush;

	}

	void Update ()
	{

		 if (_manipulator.BrushType == BuildingMan.EBrushType.Select)
		{
			// Select mode for voxels
			if (selectVoxel)
			{
				_manipulator.pattern = BSPattern.DefaultV1;
			}
			// Select mode for block
			else
				_manipulator.pattern = BSPattern.DefaultB1;
		}
		else if (_manipulator.BrushType == BuildingMan.EBrushType.Point || 
		         _manipulator.BrushType == BuildingMan.EBrushType.Box ||
		         _manipulator.BrushType == BuildingMan.EBrushType.B45Diagonal)
		{
			#if TEST_CODE
			UpdateTest();
			#endif

			if (m_Pattern != null)
				_manipulator.pattern = m_Pattern;
		}
		else
		{
				_manipulator.pattern = null;
		}

		if(_manipulator.BrushType == BuildingMan.EBrushType.Box)
		{
			BSBoxBrush brush = _manipulator.activeBrush as BSBoxBrush;
			if (brush != null)
			{
				Vector3 size = brush.Size;
                int count = Mathf.RoundToInt(size.x) * Mathf.RoundToInt(size.y) * Mathf.RoundToInt(size.z) * brush.pattern.size;

                if (Pathea.PeCreature.Instance.mainPlayer == null)
                    return;
				Pathea.PackageCmpt pkg = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PackageCmpt>();
				if (brush.dataSource == BuildingMan.Blocks)
				{
                   
                    int itemId = GetBlockItemProtoID((byte)(brush.materialType));
                    if (pkg.GetItemCount(itemId) >= ( Mathf.CeilToInt((float)count / 4)))
					{
						brush.forceShowRemoveColor = false;	
					}
					else
						brush.forceShowRemoveColor = true;
				}
				else if (brush.dataSource == BuildingMan.Voxels)
				{
                    int itemId = GetVoxelItemProtoID(brush.materialType);
					if (pkg.GetItemCount(itemId) >= count)
					{
						brush.forceShowRemoveColor = false;	
					}
					else
						brush.forceShowRemoveColor = true;
				}
			}
		}
	}

	// For  Test
	#region For_Test
	public bool GUITest = false;

	//List<BSVoxel> _voxels = new List<BSVoxel>();
	Dictionary<int, int> _costsItems = new Dictionary<int, int>();
	Dictionary<int, int> _playerItems = new Dictionary<int, int>();

	void OnGUI ()
	{
		if (!GUITest)
			return;
		int colLen = 20;
		int rowLen = 250;
		GUI.BeginGroup(new Rect(Screen.width/2 - 250, 50, rowLen, 500));
		int col = 0;
		GUI.Label(new Rect(0, col * colLen, rowLen, colLen),  "Items :");
		col ++;
		foreach (KeyValuePair<int, int> kvp in _costsItems)
		{
			GUI.Label(new Rect(0, col * colLen, 50, colLen),  "Item ID:");
			GUI.Label(new Rect(50, col * colLen, 50, colLen),  kvp.Key.ToString());
			GUI.Label(new Rect(100, col * colLen, 50, colLen),  " Count:");
			GUI.Label(new Rect(150, col * colLen, 50, colLen),  kvp.Value.ToString());
			col++;
		}

		GUI.Label(new Rect(0, col * colLen, 50, colLen),  "Player Count:");
		col ++;
		foreach (KeyValuePair<int, int> kvp in _playerItems)
		{
			GUI.Label(new Rect(0, col * colLen, 50, colLen),  "Item ID:");
			GUI.Label(new Rect(50, col * colLen, 50, colLen),  kvp.Key.ToString());
			GUI.Label(new Rect(100, col * colLen, 50, colLen),  " Count:");
			GUI.Label(new Rect(150, col * colLen, 50, colLen),  kvp.Value.ToString());
			col++;
		}
		GUI.EndGroup();       

	}
	#endregion

	#endregion
	/// <summary>
	/// voxel modify check event before really undo and redo. Call by BSVoxelModify
	/// </summary>
	bool OnCheckVoxelModify (int opType, IntVector3[] indexes, BSVoxel[] voxels, BSVoxel[] oldvoxels, EBSBrushMode mode, IBSDataSource ds)
	{
		if (IsGod)
			return true;

		if (Pathea.PeCreature.Instance.mainPlayer == null)
			return false;

		bool result = true;


		if (mode == EBSBrushMode.Add)
		{

			Dictionary<int, int> items = new Dictionary<int, int>();
			// Calculate the needed items;
			int adder = 1;
			foreach (BSVoxel voxel in voxels)
			{
				
				int id = 0;
				if (ds == BuildingMan.Blocks)
				{
					if (voxel.IsExtendable())
					{
						if (!voxel.IsExtendableRoot())
						{
							id = GetBlockItemProtoID((byte)(voxel.materialType >> 2));
							adder = 1;
						}
						else
							adder = 0;
					}
					else
						id = GetBlockItemProtoID(voxel.materialType);
					

				}
				else if (ds == BuildingMan.Voxels)
					id = GetVoxelItemProtoID(voxel.materialType);
				
				if (id <= 0)
					continue;

				if (id != 0)
				{
					if (items.ContainsKey(id))
						items[id] += adder;
					else
						items.Add(id, adder);
				}
			} 
			_costsItems = items;

			float divisor = 1.0f;

			if (ds == BuildingMan.Blocks)
				divisor = (float)( 1 << BSBlock45Data.s_ScaleInverted);

			// Has player enough items ?
			Pathea.PackageCmpt pkg = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PackageCmpt>();

			_playerItems.Clear();
			foreach (KeyValuePair<int, int> kvp in items)
			{
				_playerItems.Add(kvp.Key, pkg.GetItemCount(kvp.Key));
				if (pkg.GetItemCount(kvp.Key) < Mathf.CeilToInt( kvp.Value / divisor))
				{
					result = false;
				}
			}
			
			// now delete
			if (result)
			{
				if (GameConfig.IsMultiMode)
				{
					if (null == PlayerNetwork.mainPlayer)
						return false;

					if (!Pathea.PeGameMgr.IsMultiCoop && VArtifactUtil.IsInTownBallArea(PlayerNetwork.mainPlayer._pos))
					{
						new PeTipMsg(PELocalization.GetString(8000864), PeTipMsg.EMsgLevel.Warning);
						return false;
					}

					//if (!PlayerNetwork.OnLimitBoundsCheck(brushBound))
					//{
					//	new PeTipMsg(PELocalization.GetString(8000864), PeTipMsg.EMsgLevel.Warning);
					//	return false;
					//}

					PlayerNetwork.mainPlayer.RequestRedo(opType, indexes, oldvoxels, voxels, mode, ds.DataType, ds.Scale);

					DigTerrainManager.BlockClearGrass(ds, indexes);

					return true;
				}
				else
				{
					string debug_log = "";

					foreach (KeyValuePair<int, int> kvp in items)
					{
						if (pkg.Destory(kvp.Key, Mathf.CeilToInt(kvp.Value / divisor)))
						{
							debug_log += "\r\n Rmove Item from player package ID[" + kvp.Key.ToString() + "]" + " count - " + kvp.Value.ToString();
						}
					}

                    if (ds == BuildingMan.Blocks)
                    {
                        for (int i = 0; i < indexes.Length; i++)
                        {
                            Vector3 pos = new Vector3(indexes[i].x * ds.Scale, indexes[i].y * ds.Scale, indexes[i].z * ds.Scale) - ds.Offset;
                            PeGrassSystem.DeleteAtPos(pos);

                            PeGrassSystem.DeleteAtPos(new Vector3(pos.x, pos.y - 1, pos.z));

                            //PeGrassSystem.DeleteAtPos(new Vector3(pos.x, pos.y + 1, pos.z));
                        }
                    }
                    else if (ds == BuildingMan.Voxels)
                    {
                        for (int i = 0; i <indexes.Length; i++)
                        {
                            Vector3 pos = new Vector3(indexes[i].x, indexes[i].y, indexes[i].z);
                            PeGrassSystem.DeleteAtPos(pos);

                            PeGrassSystem.DeleteAtPos(new Vector3(pos.x, pos.y - 1, pos.z));

                            //PeGrassSystem.DeleteAtPos(new Vector3(pos.x, pos.y + 1, pos.z));
                        }
                    }

					//Debug.LogWarning(debug_log);
				}
				
			}
            else
            {
                new PeTipMsg(PELocalization.GetString(821000001), PeTipMsg.EMsgLevel.Warning);
            }
		}
		else if (mode == EBSBrushMode.Subtract)
		{
			Dictionary<int, int> items = new Dictionary<int, int>();
			// Calculate the needed items;
			int adder = 1;
			foreach (BSVoxel voxel in oldvoxels)
			{
				
				int id = 0;
                if (ds == BuildingMan.Blocks)
                {
                    if (voxel.IsExtendable())
                    {
                        if (!voxel.IsExtendableRoot())
                        {
                            id = GetBlockItemProtoID((byte)(voxel.materialType >> 2));
                            adder = 1;
                        }
                        else
                            adder = 0;
                    }
                    else
                    {
                        if (!BuildingMan.Blocks.VoxelIsZero(voxel, 0))
                            id = GetBlockItemProtoID((byte)(voxel.materialType));
                    }
                }
                else if (ds == BuildingMan.Voxels)
                {
                    if (!BuildingMan.Voxels.VoxelIsZero(voxel, 1))
                        id = GetVoxelItemProtoID(voxel.materialType);
                }
				
				if (id <= 0)
					continue;
				
				if (items.ContainsKey(id))
					items[id] += adder;
				else
					items.Add(id, adder);
			} 


			float divisor = 1.0f;
			
			if (ds == BuildingMan.Blocks)
				divisor = (float)( 1 << BSBlock45Data.s_ScaleInverted);

			// Has player enough package ?
			Pathea.PlayerPackageCmpt pkg = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PlayerPackageCmpt>();

			MaterialItem[] array = new MaterialItem[items.Count];

			int i = 0;
			foreach (KeyValuePair<int, int> kvp in items)
			{
				array[i] = new MaterialItem()
				{
					protoId = kvp.Key,
					count = Mathf.FloorToInt( kvp.Value / divisor)
				};
				i++;
			}

			result = pkg.package.CanAdd(array);

			// Really add
			if (result)
			{
				if (GameConfig.IsMultiMode)
				{
					if (null == PlayerNetwork.mainPlayer)
						return false;

					//if (!PlayerNetwork.OnLimitBoundsCheck(brushBound))
					//{
					//	new PeTipMsg(PELocalization.GetString(8000864), PeTipMsg.EMsgLevel.Warning);
					//	return false;
					//}

					PlayerNetwork.mainPlayer.RequestRedo(opType, indexes, oldvoxels, voxels, mode, ds.DataType,ds.Scale);
					return true;
				}
				else
				{
					string debug_log = "";
					foreach (MaterialItem mi in array)
					{
						if (mi.count != 0)
							pkg.Add(mi.protoId, mi.count);
						debug_log += "Add Item from player package ID[" + mi.protoId.ToString() + "]" + " count - " + mi.count.ToString() + "\r\n";
					}

					Debug.LogWarning(debug_log);
				}
			}


		}

		if (result)
		{
			if (onVoxelMotify != null)
				onVoxelMotify(indexes, voxels, oldvoxels, mode, ds);
		}

		return result;
	}
	
	void OnCreateBrush(BSBrush brush, BuildingMan.EBrushType type)
	{
//		if (type == BuildingMan.EBrushType.Select)
//		{
//			_updatePattern = false;
//			// Select mode for voxels
//			if (selectVoxel)
//				_manipulator.pattern = BSPattern.DefaultV1;
//			// Select mode for block
//			else
//				_manipulator.pattern = BSPattern.DefaultB1;
//		}
//		else if (type == BuildingMan.EBrushType.Iso)
//		{
//			_updatePattern = false;
//
//			_manipulator.pattern = BSPattern.DefaultB1;
//		}
//		else 
//		{
//			_updatePattern = true;
//		}
	}

#if TEST_CODE
	void UpdateTest()
	{
		if (defaultPattern == EDefaultPattern.V1)
			Pattern = BSPattern.DefaultV1;
		else if (defaultPattern == EDefaultPattern.V2)
			Pattern = BSPattern.DefaultV2;
		else if (defaultPattern == EDefaultPattern.V3)
			Pattern = BSPattern.DefaultV3;
		else if (defaultPattern == EDefaultPattern.B1)
			Pattern = BSPattern.DefaultB1;
		else if (defaultPattern == EDefaultPattern.B2 )
			Pattern = BSPattern.DefaultB2;
		else if (defaultPattern == EDefaultPattern.B3)
			Pattern = BSPattern.DefaultB3;
		else if (defaultPattern == EDefaultPattern.UserDefine1)
			Pattern = BSPattern.s_tblPatterns[13];
		else if (defaultPattern == EDefaultPattern.UserDefine1_1)
			Pattern = BSPattern.s_tblPatterns[1];
		else if (defaultPattern == EDefaultPattern.UserDefine1_2)
			Pattern = BSPattern.s_tblPatterns[2];
		else if (defaultPattern == EDefaultPattern.UserDefine1_3)
			Pattern = BSPattern.s_tblPatterns[3];
		else if (defaultPattern == EDefaultPattern.UserDefine1_4)
			Pattern = BSPattern.s_tblPatterns[4];
		else if (defaultPattern == EDefaultPattern.UserDefine1_5)
			Pattern = BSPattern.s_tblPatterns[5];
		else if (defaultPattern == EDefaultPattern.UserDefine2)
			Pattern = BSPattern.s_tblPatterns[27];
		else if (defaultPattern == EDefaultPattern.UserDefine2_1)
			Pattern = BSPattern.s_tblPatterns[24];
		else if (defaultPattern == EDefaultPattern.UserDefine2_2)
			Pattern = BSPattern.s_tblPatterns[25];
		else if (defaultPattern == EDefaultPattern.UserDefine2_3)
			Pattern = BSPattern.s_tblPatterns[26];
		else if (defaultPattern == EDefaultPattern.UserDefine2_4)
			Pattern = BSPattern.s_tblPatterns[27];
		else if (defaultPattern == EDefaultPattern.UserDefine2_5)
			Pattern = BSPattern.s_tblPatterns[28];
		else if (defaultPattern == EDefaultPattern.UserDefine4)
			Pattern = BSPattern.s_tblPatterns[34];
		else if (defaultPattern == EDefaultPattern.UserDefine4_1)
			Pattern = BSPattern.s_tblPatterns[32];
		else if (defaultPattern == EDefaultPattern.UserDefine4_2)
			Pattern = BSPattern.s_tblPatterns[33];
		else if (defaultPattern == EDefaultPattern.UserDefine4_3)
			Pattern = BSPattern.s_tblPatterns[37];
		else if (defaultPattern == EDefaultPattern.UserDefine4_4)
			Pattern = BSPattern.s_tblPatterns[35];
		else if (defaultPattern == EDefaultPattern.UserDefine4_5)
			Pattern = BSPattern.s_tblPatterns[36];
	}
#endif
}
