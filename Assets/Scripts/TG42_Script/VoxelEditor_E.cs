#if UNITY_EDITOR
#define DefineVoxelEditor
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.InteropServices;

public partial class VoxelEditor : MonoBehaviour
{
#if DefineVoxelEditor
	
	public enum EEditMode
	{
		eEditNormSpec,
		eEditVoxel,
		eEditVegetables,
	};
	
    public enum EBrushType
    {
        eBoxBrush,
        eSphereBrush,
		eTerStyleBrush,
		eBrushMax,
    };

    public enum EBuildType
    {
        eBuildVoxel,
		eFlatten,
        eSmoothVoxel,
		eGrowNoise,
		ePlantFilter,
    };
	public enum EVoxelTerrainSet
    {
        _1_Terrain1 = 1,
        _2_Terrain2 = 17,
		_3_Terrain3 = 33,
		_4_Terrain4 = 49,
		_5_Terrain5 = 65,
    };
	
	public enum EVoxelTerrainSubSet
    {
        _1_Type = 0,
        _2_Type,
        _3_Type,
		_4_Type,
		_5_Type,
        _6_Type,
        _7_Type,
		_8_Type,
		_9_Type,
        _10_Type,
        _11_Type,
		_12_Type,
		_13_Type,
        _14_Type,
        _15_Type,
		_16_Type,
    };

	Vector3 m_rayDir;
    Vector3 m_raycastHit; //用于brush点击测试之用
	RaycastHit m_hitInfo; //用于brush点击测试之用
	public EVoxelTerrainSet m_eVoxelTerrainSet = EVoxelTerrainSet._1_Terrain1;
	public EVoxelTerrainSubSet m_eVoxelTerrainSubSet = EVoxelTerrainSubSet._1_Type;
	public const int constVoxelType = -1;//(int)VFVoxel.EType.WATER;
    public bool m_bPaintVoxelTypeMode;
	public float m_NoiseStrength;
    bool m_rayHit = false;
		
    public bool m_drawGizmo = false; //是否显示brush
    public GameObject m_boxBrushPrefab; //boxBrush依赖的prefab
    public GameObject m_sphereBrushPrefab; //sphereBrush依赖的prefab
    public GameObject m_boxBrush; //boxBrush游戏对象
    public GameObject m_sphereBrush; //sphereBrush游戏对象
	
	public EEditMode m_eEditMode = EEditMode.eEditNormSpec;
    public EBrushType m_eBuildBrush = EBrushType.eTerStyleBrush;
    public EBuildType m_eBuildType = EBuildType.eBuildVoxel;

    public float m_alterRadius = 1.0f; //brush半径
	public float m_alterPower = 1.0f;
	public float m_alterDstH = 100.0f;
	public float m_GrowNoiseCooldown = 0.3f;
	public int m_voxelFilterSize = 2; //voxel平滑半径
	public Texture2D FilterMap;
	public float shootingOffset = 10;
	public int PlantFilterWeightDivisor = 50;
	public int EdgeWeight = 50;	

	public int m_buildStartIdx = 0; //刷植被开始的索引 
	public int m_buildRange = 5;	//刷植被范围 
	public int m_buildDensity = 100;	//植被刷密度 
	public float m_baseWidthScale = 0.5f; //植被随机宽度起始scale 
	public float m_baseHeightScale = 0.5f; //植被随机高度起始scale 
	public Color m_baseColor = new Color(0.9f, 0.9f, 0.9f); //植被随机颜色起始颜色 
	public float m_maxBuildAngle = 40.0f; //植被最大种植角度，超过该角度则不再种植植被 
	
	// Ter Style Brush
	public Projector terBrushProjector = null;
	public List<Texture2D> terBrushTextures = new List<Texture2D>();
	public int terBrushType = 0;
	
	public List<String> SceneryObjectNames = null;
	public VoxelPaintXMLParser vxMatXmlParser = null;
	static int ms_iResSeed = 98765;

	HashSet<VFVoxelChunkData> dirtyChunkList = new HashSet<VFVoxelChunkData>();
	private HashSet<VFVoxelChunkData> m_rebuildChunkList = new HashSet<VFVoxelChunkData>(); //用于过滤重复的rebuildChunk
	void SetChunkDirty(List<VFVoxelChunkData> vcList)
	{
		int n = vcList.Count;
		for(int i = 0; i < n; i++)
			m_rebuildChunkList.Add(vcList[i]);	//just for save
	}
	

#if UNITY_EDITOR	
	void OnGUI () 
	{
		if ( GameConfig.IsInVCE )
			return;
//		if(Application.isEditor && !GameConfig.IsInVCE)
//		{
//			if(SceneryObjectNames == null)
//			{
//				SceneryObjectNames = new List<string>();
//			}
//			
//			GUI.Box (new Rect (10,10,150,140), "Commands");
//
//			if (GUI.Button (new Rect (20,40,130,40), "Detach Camera\n From Player")) 
//			{
//				GameObject go = GameObject.Find("Player");
//				if(go != null )
//				{
//					go.SetActive(false);
//				}
//				
//				m_voxelTerrain.saveTerrain = true;
//				PECameraMan.Instance.EnterFreeLook();
//				//go = GameObject.Find("GameMainCamera");
//				//go.GetComponent<FreeCamera>().enabled = true;
//				//go.GetComponent<CameraController>().enabled = false;
////				go.GetComponent<GameMainGUI>().enabled = false;
//
//				this.m_eEditMode = EEditMode.eEditVoxel;
//				this.m_eBuildType = EBuildType.eBuildVoxel;
//				this.m_drawGizmo = true;
//				
//				GameObject astar = GameObject.Find("A*");
//				GameObject aiManager = GameObject.Find("AiManager");
//				
//				if(astar != null && astar.activeSelf)
//					astar.SetActive(false);
//				
//				if(aiManager != null && aiManager.activeSelf)
//					aiManager.SetActive(false);
//				
//				if(null != TownEditor.Instance)
//					TownEditor.Instance.ActiveEditor();
//			} 
//			if (GUI.Button (new Rect (20,90,130,40), "Save Scenery Objects"))
//			{
//				SaveSceneryObjects();
//			}
//		}
	}
#endif
	
	//更新brush
    void UpdateBrush()
    {
        //根据鼠标滚轮设置修改半径
        float _scrollValue = Input.GetAxis("Mouse ScrollWheel") * 5.0f;
        if (Input.GetKey(KeyCode.LeftAlt) && _scrollValue != 0.0f)
        {
            m_alterRadius += _scrollValue;
            m_alterRadius = Mathf.Clamp(m_alterRadius, 0.0f, 256.0f);
        }

        //根据射线查询位置更新brush位置
        m_rayHit = false;
        RaycastHit _raycastHit;
        Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int vfterrainLayer = Pathea.Layer.VFVoxelTerrain;
		float _fScale = m_alterRadius * 2.0f;

        if (Physics.Raycast(_ray, out _raycastHit, Mathf.Infinity, 1 << vfterrainLayer))
        {
			m_rayDir = _ray.direction;
            m_raycastHit = _raycastHit.point;
			m_hitInfo = _raycastHit;
            m_rayHit = true;

            if (m_drawGizmo)
            {
				if(m_boxBrush == null)
				{
				    m_boxBrush = (GameObject)GameObject.Instantiate(m_boxBrushPrefab, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
		        	m_boxBrush.layer = 2;
		        	m_boxBrush.SetActive(false);
				}
				if(m_sphereBrush == null)
				{
			        m_sphereBrush = (GameObject)GameObject.Instantiate(m_sphereBrushPrefab, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
			        m_sphereBrush.layer = 2;
			        m_sphereBrush.SetActive(false);
				}				
				switch(m_eBuildBrush)
				{
				case EBrushType.eBoxBrush:
                    m_boxBrush.SetActive(true);
                    m_sphereBrush.SetActive(false);
					if(terBrushProjector!=null)	terBrushProjector.enabled = false;
                    m_boxBrush.transform.localScale = new Vector3(_fScale, _fScale, _fScale);
                    m_boxBrush.transform.position = _raycastHit.point;
					break;
				case EBrushType.eSphereBrush:
                    m_sphereBrush.SetActive(true);
                    m_boxBrush.SetActive(false);
					if(terBrushProjector!=null)	terBrushProjector.enabled = false;
                    m_sphereBrush.transform.localScale = new Vector3(_fScale, _fScale, _fScale);
                    m_sphereBrush.transform.position = _raycastHit.point;
                    break;
				case EBrushType.eTerStyleBrush:
                    m_sphereBrush.SetActive(false);
                    m_boxBrush.SetActive(false);
					if(terBrushProjector!=null){
						terBrushProjector.enabled = true;
						terBrushProjector.transform.position = new Vector3(_raycastHit.point.x, _raycastHit.point.y+500,_raycastHit.point.z);
						terBrushProjector.orthographicSize = _fScale/2;
					}
					break;
                }
            }
            else
            {
                if(m_boxBrush != null)	m_boxBrush.SetActive(false);
                if(m_sphereBrush != null)	m_sphereBrush.SetActive(false);
				if(terBrushProjector!=null)	terBrushProjector.enabled = false;
            }
        }
    }

	//更新build操作
    void UpdateBuildOp()
    {
        if (m_rebuildChunkList.Count != 0)//proceed rebuildChunk
		{
            foreach (VFVoxelChunkData _chunk in m_rebuildChunkList)
            {
                m_voxelTerrain.SaveLoad.AddChunkToSaveList(_chunk);
            }
            m_rebuildChunkList.Clear();
        }

        UpdateBrush();

        if (m_rayHit == false)
            return;

//        int _row = (int)(m_raycastHit.z / SubTerrain.s_subTerSize.z);
//        int _col = (int)(m_raycastHit.x / SubTerrain.s_subTerSize.x);
//        int _idx = (_row << 16) + _col;
//        if (m_mapSubTer.TryGetValue(_idx, out m_curEditSubTer) == false)
            //return;
		if(Application.isEditor)
		{
			if(m_eEditMode == EEditMode.eEditVegetables)
			{
				int bvgtmode = 0;
	            //Add vegetations
	            if (Input.GetKeyDown(KeyCode.Mouse0) && (Input.GetKey(KeyCode.LeftAlt)))
	            {
					bvgtmode = 1;
	            }
				else  //remove vegetations
	            if (Input.GetKeyDown(KeyCode.Mouse0) && (Input.GetKey(KeyCode.H)))
	            {
					bvgtmode = 2;
	            }
				
				if(bvgtmode != 0)
				{
					switch(m_eBuildBrush)
					{
					case EBrushType.eBoxBrush:
	                    //ModifyBoxVegetation(bvgtmode==1);
						break;
					case EBrushType.eSphereBrush:
						//ModifySphereVegetation(bvgtmode==1);
						break;
					case EBrushType.eTerStyleBrush:
						//ModifyTerrainVegetation(bvgtmode==1);
						break;
					}
	                return;
				}
			}
			else if(m_eEditMode == EEditMode.eEditVoxel)
			{
		        if (m_eBuildType == EBuildType.eBuildVoxel)  //如果创建类型是编辑voxel
		        {
					int bvxlmode = 0;
					Vector3 _hitVoxelPos = m_raycastHit - m_voxelTerrain.transform.position;
					float _fModVol = 0f;
					byte newType = 0;
		            // Dig mode
		            if (Input.GetKey(KeyCode.Mouse0) && (Input.GetKey(KeyCode.H)))
		            {
						bvxlmode = 1;
		                _fModVol = -m_alterPower * Time.deltaTime * 1000.0f;
						newType = 2;
		            }
					else   //Build mode
		            if (Input.GetKey(KeyCode.Mouse0) && (Input.GetKey(KeyCode.LeftAlt)))
		            {
						bvxlmode = 2;
		                _fModVol = m_alterPower * Time.deltaTime * 1000.0f;
						newType = (byte)((int)m_eVoxelTerrainSet + (int)m_eVoxelTerrainSubSet);
		            }
					
					if(bvxlmode != 0)
					{
						bool bRemoveVegetation = true;
						if(m_bPaintVoxelTypeMode)
						{
							bvxlmode = 2;
							_fModVol = 0f;
							newType = (byte)((int)m_eVoxelTerrainSet + (int)m_eVoxelTerrainSubSet);
							bRemoveVegetation = false;
						}
						switch(m_eBuildBrush)
						{
						case EBrushType.eBoxBrush:
							ModifyBoxVoxel(_hitVoxelPos, _fModVol, bvxlmode==2, newType, bRemoveVegetation);
							break;
						case EBrushType.eSphereBrush:
							_hitVoxelPos += m_rayDir*Mathf.Clamp01(1-m_alterPower*0.5f)*m_alterRadius;
							ModifySphereVoxel(_hitVoxelPos, _fModVol, bvxlmode==2, newType, bRemoveVegetation);
							break;
						case EBrushType.eTerStyleBrush:
							//ModifyTerrainVoxel(_hitVoxelPos, _fModVol, bvxlmode==2, newType, true);
							ModifyTerrainHeight(_hitVoxelPos, _fModVol, bvxlmode==2, newType, bRemoveVegetation);
							break;
						}
		                return;
					}
		        }
				else if (m_eBuildType == EBuildType.eFlatten)
				{
					Vector3 _hitVoxelPos = m_raycastHit - m_voxelTerrain.transform.position;
					byte newType = 2;
					if (Input.GetKey(KeyCode.Mouse0) && (Input.GetKey(KeyCode.H)))
					{
						FlattenTerrain(_hitVoxelPos, newType, true, m_eBuildBrush!=EBrushType.eBoxBrush);
					}
					else
		            if (Input.GetKey(KeyCode.Mouse0) && (Input.GetKey(KeyCode.LeftAlt)))
					{
						FlattenTerrain(_hitVoxelPos, newType, true, m_eBuildBrush!=EBrushType.eBoxBrush);
					}
					return;
				}
		        else if (m_eBuildType == EBuildType.eSmoothVoxel) //如果搭建模式是平滑voxel
		        {
		            Vector3 _hitVoxelPos = m_raycastHit - m_voxelTerrain.transform.position;
		
		            if (Input.GetKey(KeyCode.Mouse0) && (Input.GetKey(KeyCode.LeftAlt)))
		            {
		                if (m_eBuildBrush == EBrushType.eBoxBrush)
		                    SmoothFilter(_hitVoxelPos, m_voxelFilterSize, false);
		                else if (m_eBuildBrush == EBrushType.eSphereBrush)
		                    SmoothFilter(_hitVoxelPos, m_voxelFilterSize, true);
						else if (m_eBuildBrush == EBrushType.eTerStyleBrush)
							SmoothFilter(_hitVoxelPos, m_voxelFilterSize, true);
		
		                return;
		            }
		        }
				else if (m_eBuildType == EBuildType.eGrowNoise) // paint material on existing terrain
		        {
		            if (Input.GetKey(KeyCode.Mouse0) && (Input.GetKey(KeyCode.LeftAlt)) && Time.time - lastOp > m_GrowNoiseCooldown)
		            {
						lastOp = Time.time;
		                Vector3 _hitVoxelPos = m_raycastHit - m_voxelTerrain.transform.position;
						byte newType = (byte)((int)m_eVoxelTerrainSet + (int)m_eVoxelTerrainSubSet);
		                growNoise(_hitVoxelPos, 1, true, newType);
		
		                return;
		            }
					if (Input.GetKey(KeyCode.Mouse0) && (Input.GetKey(KeyCode.H)) && Time.time - lastOp > m_GrowNoiseCooldown)
		            {
						lastOp = Time.time;
		                Vector3 _hitVoxelPos = m_raycastHit - m_voxelTerrain.transform.position;
						//byte newType = (byte)((int)m_eVoxelTerrainSet + (int)m_eVoxelTerrainSubSet);
		                growNoise(_hitVoxelPos, -1, false, 0);
		                return;
		            }
		        }
				else if (m_eBuildType == EBuildType.ePlantFilter) // paint material on existing terrain
		        {
		            if (Input.GetKey(KeyCode.Mouse0) && (Input.GetKey(KeyCode.LeftAlt)) && Time.time - lastOp > m_GrowNoiseCooldown)
		            {
						lastOp = Time.time;
		                //Vector3 _hitVoxelPos = m_raycastHit - m_voxelTerrain.transform.position;
						byte newType = (byte)((int)m_eVoxelTerrainSet + (int)m_eVoxelTerrainSubSet);
		                PlantFilter(m_hitInfo, 1, true, newType);
		                return;
		            }
					if (Input.GetKey(KeyCode.Mouse0) && (Input.GetKey(KeyCode.H)) && Time.time - lastOp > m_GrowNoiseCooldown)
		            {
						lastOp = Time.time;
		                //Vector3 _hitVoxelPos = m_raycastHit - m_voxelTerrain.transform.position;
						//byte newType = (byte)((int)m_eVoxelTerrainSet + (int)m_eVoxelTerrainSubSet);
		                PlantFilter(m_hitInfo, -1, false, 0);
		                return;
		            }
		        }
			}
		}
    //添加指定位置的tree
    }	
	
	void FindAllSceneryAssetNames()
	{
		// populate SceneryObjectNames from the assetbundle folder.
		SceneryObjectNames = new List<String> ();
		DirectoryInfo dir = new DirectoryInfo(GameConfig.AssetBundlePath+GameConfig.AssetsManifest_Item+"/");
		FileInfo[] info = dir.GetFiles("*.*");
		//foreach (FileInfo f in info)
		for(int i = 0; i < info.Length; i++)	
		{
			String[] subNames = info[i].Name.Split('.');
 			if(subNames != null && subNames[0] != null && subNames[1] == "unity3d" && subNames.Length == 2)
				SceneryObjectNames.Add(subNames[0]);
			
		}
		
//		Dictionary<string, AssetBundle> cacheMap = AssetBundlesMan.Instance.GetCacheMap();
//		Dictionary<GameObject, AssetBundle> map = AssetBundlesMan.Instance.GetMap();
//		Dictionary<string, AssetBundle>.KeyCollection cacheKey = cacheMap.Keys;
//		Dictionary<GameObject, AssetBundle>.KeyCollection mapKey = map.Keys;
//		foreach( string s in cacheKey )
//		{
//			print("cached : " + s);
//		}
//		
//		foreach( GameObject s in mapKey )
//		{
//			print("map    : " + s.name);
//		}
	}

    string GetFilePath()
    {
        if (Pathea.PeGameMgr.IsSingleStory)
        {
            return @"Assets/Resources/DynamicAssets.xml";
        }
        else if (Pathea.PeGameMgr.IsMultiTowerDefense)
        {
            return @"Assets/Resources/NetworkDynamicAssets/towerdefense.xml";
        }

        return null;
    }

	void WriteXMLAssetList(List<AssetBundleDesc> assetsList_Dynamic)
	{
		XmlSerializer serializer = new XmlSerializer(typeof(List<AssetBundleDesc>));
		
		// Write new manifest
		TextWriter writer = null;
		try{
            writer = new StreamWriter(GetFilePath());            

			serializer.Serialize(writer,assetsList_Dynamic);
			writer.Close();
		}
		catch(Exception ex)
		{
			if(writer != null )
				writer.Close();
			Debug.Log(ex.ToString());
		}
	}
	
	List<AssetBundleDesc> ReadXMLAssetList()
	{
		List<AssetBundleDesc> assetsList;
		XmlSerializer serializer = new XmlSerializer(typeof(List<AssetBundleDesc>));
		
		// Read old manifest 
		
		TextReader reader = null;
		try{
            reader = new StreamReader(GetFilePath());

			assetsList = (List<AssetBundleDesc>)serializer.Deserialize(reader);
			reader.Close();
		}
		catch(Exception ex)
		{
			Debug.Log(ex.ToString());
			if(reader != null )
				reader.Close();
			assetsList = new List<AssetBundleDesc>();
		}
		return assetsList;
	}

	void SaveSceneryObjects()
	{
		FindAllSceneryAssetNames ();
		GameObject[] gos = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
		List<GameObject> goToSave = new List<GameObject>();
		// filter the current GOs and list all the scenery objects.
		for(int i = 0; i < gos.Length; i++ )
		{
			for(int j = 0; j < SceneryObjectNames.Count; j++ )
			{
				if(gos[i].name == SceneryObjectNames[j]
					&& gos[i].transform.root.name != "EditBuilding") //If item is in EditBuilding don't add.
				{
					goToSave.Add(gos[i]);
					print("added scenery object : " + gos[i].name);
				}
			}
		}
		List<AssetBundleDesc> res = ReadXMLAssetList();
		for(int i = 0; i < goToSave.Count; i++ )
		{
			bool found = false;
			for(int j = 0; j < res.Count; j++ )
			{
				if("Item/" + goToSave[i].name == res[j].pathName)
				{
					// the bool var 'createdPos' is used to indicate if an instance of the SO is already at the given exact location
					
					bool createdPos = false;
					AssetPRS addPos = new AssetPRS(goToSave[i].transform.position, goToSave[i].transform.rotation,goToSave[i].transform.localScale);
					for(int k = 0; k < res[j].pos.Length; k++ )
					{
						
						if(res[j].pos[k].Equals(addPos))
						{
							print("same scenery object found at the location");
							found = true;
							createdPos = true;
							break;
						}
							
					}
					// no matching SO is found at this pos. append this pos to the list.
					if(createdPos == false )
					{
						AssetPRS[] newPosList = new AssetPRS[res[j].pos.Length + 1];
						Array.Copy(res[j].pos, newPosList, res[j].pos.Length);
						newPosList[res[j].pos.Length] = addPos;
						res[j].pos = newPosList;
						found = true;
					}
				}
			}
			// if such SO is not even created in the xml, create a new entry with the SO's name.
			// put the pos into its list.
			if(found == false)
			{
				AssetBundleDesc desc = new AssetBundleDesc();
				// take out the (Clone) in the name.
				//desc.pathName =  deductString("DynamicAssets/" + goToSave[i].name);
				desc.pathName =  "Item/" + goToSave[i].name;
				
				desc.pos = new AssetPRS[1];
				desc.pos[0] = new AssetPRS(goToSave[i].transform.position, goToSave[i].transform.rotation, goToSave[i].transform.localScale);
				res.Add(desc);
			}
		}
		//write the new list to dynamicassets.xml
		WriteXMLAssetList(res);
		
		TownEditor.Instance.Save();
	}
	String deductString(String original)
	{
		String[] splitted = original.Split('(');
		return splitted[0];
	}
    //移除指定位置的tree实例
    public void RemoveTree(IntVector3 ipos)
    {
//        SubTerrain _subTer = QuerySubTerrain(_treePos);
//        if (_subTer == null)
//            return false;
//        if(_subTer.DeleteTreeInfo(_treePos, m_buildStartIdx, m_buildStartIdx + m_buildRange - 1, m_treePrototypeName, deltree))
//		{
//        	SubTerrain.AddUserDataMask(_subTer);
//		}
//		return false;

		LSubTerrainMgr.DeleteTreesAtPos(ipos);

		// TODO: GRASSEDITOR
//		if (GrassEditor.Instence != null)
//		{
//			GrassEditor.Instence.DeleteGrass(ipos);
//		}
    }
	//替换指定编号的voxel
	void ReplaceBeg()
	{
		dirtyChunkList.Clear();
	}
	void ReplaceOneVoxel(int _vx,int _vy,int _vz, VFVoxel _newVoxel)
	{
		IVxDataSource _vds = m_voxelTerrain.Voxels;
		List<IntVector3> chunkPosList = VFVoxelChunkData.GetDirtyChunkPosList(_vx, _vy, _vz);
		int nChunkPosList = chunkPosList.Count;
		for(int n = 0; n < nChunkPosList; n++)
		{
			IntVector3 chunkPos = chunkPosList[n];
			VFVoxelChunkData data = _vds.readChunk(chunkPos.x, chunkPos.y, chunkPos.z);
			data.WriteVoxelAtIdxNoReq(_vx-(chunkPos.x<<VoxelTerrainConstants._shift),
			                          _vy-(chunkPos.y<<VoxelTerrainConstants._shift),
			                          _vz-(chunkPos.z<<VoxelTerrainConstants._shift),
			                          _newVoxel);
			dirtyChunkList.Add(data);
			m_rebuildChunkList.Add(data);	//just for save
		}
	}
	void ReplaceEnd()
	{
		foreach(VFVoxelChunkData data in dirtyChunkList)
		{
			data.AddToBuildList();
		}
	}
	CVFVoxel SafeReplaceOneVoxel(int _vx, int _vy, int _vz, VFVoxel _newVoxel )
	{
		try
		{
			IVxDataSource _vds = m_voxelTerrain.Voxels;
			List<IntVector3> chunkPosList = VFVoxelChunkData.GetDirtyChunkPosList(_vx, _vy, _vz);
			int nChunkPosList = chunkPosList.Count;
			VFVoxel oldVoxel = _vds.Read(_vx, _vy, _vz);
			for(int n = 0; n < nChunkPosList; n++)
			{
				IntVector3 chunkPos = chunkPosList[n];
				VFVoxelChunkData data = _vds.readChunk(chunkPos.x, chunkPos.y, chunkPos.z);
				if(!data.WriteVoxelAtIdxNoReq(	_vx-(chunkPos.x<<VoxelTerrainConstants._shift),
												_vy-(chunkPos.y<<VoxelTerrainConstants._shift),
												_vz-(chunkPos.z<<VoxelTerrainConstants._shift),
												_newVoxel))
				{

					for(int m = 0; m < n; m++)
					{
						chunkPos = chunkPosList[m];
						data = _vds.readChunk(chunkPos.x, chunkPos.y, chunkPos.z);
						data.WriteVoxelAtIdxNoReq(_vx-(chunkPos.x<<VoxelTerrainConstants._shift),
						                          _vy-(chunkPos.y<<VoxelTerrainConstants._shift),
						                          _vz-(chunkPos.z<<VoxelTerrainConstants._shift),
						                          oldVoxel);
					}
					return null;
				}
			}
			for(int n = 0; n < nChunkPosList; n++)
			{
				IntVector3 chunkPos = chunkPosList[n];
				VFVoxelChunkData data = _vds.readChunk(chunkPos.x, chunkPos.y, chunkPos.z);
				dirtyChunkList.Add(data);
				m_rebuildChunkList.Add(data);	//just for save
			}
			return new CVFVoxel(oldVoxel);
		}
		catch{
			//this chunkdata not loaded
			return null;
		}
	}
	void FlattenTerrain(Vector3 _hitVoxelPos, byte _type, bool removeVegetation, bool isRounded)
	{
        //读取平滑半径范围内的所有voxel
        IntVector3 _hitVoxelIdx = new IntVector3(_hitVoxelPos);
#if false		//VOXEL_OFFSET
        _hitVoxelIdx.x += 1;
        _hitVoxelIdx.y += 1;
        _hitVoxelIdx.z += 1;
#endif
		int _alterRadius = (int)m_alterRadius;
		float _sqAlterRadius = _alterRadius*_alterRadius;
		float fSrcH = Camera.main.transform.position.y;
		float fDstH = m_alterDstH;
		if(fDstH > fSrcH-1) fDstH = fSrcH - 1;
		int iSrcH = (int)(fSrcH + 0.5f);
		int iDstH = (int)(fDstH + 0.5f);
		ReplaceBeg();
		//VFVoxel curVoxel = m_voxelTerrain.Voxels.Read(_hitVoxelIdx.x, _hitVoxelIdx.y, _hitVoxelIdx.z);
		for(int vx = _hitVoxelIdx.x - _alterRadius + 1; vx < _hitVoxelIdx.x + _alterRadius; vx++)
		{
			for(int vz = _hitVoxelIdx.z - _alterRadius + 1; vz < _hitVoxelIdx.z + _alterRadius; vz++)
			{
				int deltaX = vx - _hitVoxelIdx.x;
				int deltaZ = vz - _hitVoxelIdx.z;
				if(isRounded && deltaX*deltaX+deltaZ*deltaZ > _sqAlterRadius)
					continue;
				
				int vy;
				if(removeVegetation)
				{
					for(vy = iSrcH-1; vy < iDstH; vy++)
					{
		            	RemoveTree(new IntVector3(vx, vy, vz));
					}
//					GrassMgr.RefreshDirtyChunks();
				}
				
				for(vy = iSrcH-1; vy > iDstH; vy--)
				{
					if(null == SafeReplaceOneVoxel(vx,vy,vz,new VFVoxel(0)))
						break;
				}
				if(vy > iDstH)						continue;
				
				float _fDec = fDstH - (int)fDstH;
				if(_fDec < 0.5f)
				{
					if(null == SafeReplaceOneVoxel(vx,iDstH,vz,new VFVoxel((byte)(255.999f*0.5/(1-_fDec)), _type)))
						continue;
				}
				else
				{
					if(null == SafeReplaceOneVoxel(vx,iDstH,vz,new VFVoxel((byte)(255.999f*(1-0.5/_fDec)), _type)))
						continue;
				}
				
				CVFVoxel oldCVoxel;
				for(vy = iDstH-1; vy > 1; vy--)
				{
					oldCVoxel = SafeReplaceOneVoxel(vx,vy,vz,new VFVoxel(255, _type));
					if(null == oldCVoxel || oldCVoxel._value.Volume >= 0x80)
						break;
				}
			}
		}
		ReplaceEnd();
		if(removeVegetation)
		{
//			GrassMgr.RefreshDirtyChunks();
		}
	}
	
    //平滑指定的voxel数组
    VFVoxel[, ,] SmoothVoxels(VFVoxel[, ,] _voxels, int _alterRadius, int _filterSize)
    {
        int _size = _alterRadius * 2 + 1;
        VFVoxel[, ,] _newVoxels = new VFVoxel[_size, _size, _size];
        int _startIdx = _filterSize;
        int _endIdx = _filterSize + _size;

        for (int _ix = _startIdx; _ix != _endIdx; ++_ix)
        {
            for (int _iy = _startIdx; _iy != _endIdx; ++_iy)
            {
                for (int _iz = _startIdx; _iz != _endIdx; ++_iz)
                {
                    if (_voxels[_ix, _iy, _iz].Type == 0)
                        continue;

                    VFVoxel _dstVoxel = new VFVoxel();
                    FilterVoxel(ref _dstVoxel, _ix, _iy, _iz, _filterSize, _voxels);
                    _newVoxels[_ix - _startIdx, _iy - _startIdx, _iz - _startIdx] = _dstVoxel;
                }
            }
        }

        return _newVoxels;
    }

    //过滤指定的单个voxel
    void FilterVoxel(ref VFVoxel _dstVoxel, int _ix, int _iy, int _iz, int _filterSize, VFVoxel[, ,] _voxels)
    {
        float _vol = 0.0f;
        int _idx = 0;
        for (int _i = -_filterSize; _i <= _filterSize; ++_i)
        {
            for (int _j = -_filterSize; _j <= _filterSize; ++_j)
            {
                for (int _k = -_filterSize; _k <= _filterSize; ++_k)
                {
                    int _x = _ix + _i;
                    int _y = _iy + _j;
                    int _z = _iz + _k;
                    VFVoxel _voxel = _voxels[_x, _y, _z];
                    _vol += _voxel.Volume;
                    ++_idx;
                }
            }
        }

         _vol /= _idx;
        _dstVoxel.Volume = (byte)_vol;
        _dstVoxel.Type = _voxels[_ix, _iy, _iz].Type;
    }

    //平滑voxel
    void SmoothFilter(Vector3 _hitVoxelPos, int _filterSize, bool isRounded)
    { 
        //读取平滑半径范围内的所有voxel
        IntVector3 _hitVoxelIdx = new IntVector3(_hitVoxelPos);
#if false		//VOXEL_OFFSET
        _hitVoxelIdx.x += 1;
        _hitVoxelIdx.y += 1;
        _hitVoxelIdx.z += 1;
#endif

        //smooth voxel's volume
        int _alterRadius = (int)m_alterRadius;
        VFVoxel[, ,] _voxels = ReadVoxels(_hitVoxelIdx, _alterRadius, _filterSize);
        VFVoxel[, ,] _newVoxels = SmoothVoxels(_voxels, _alterRadius, _filterSize);
		ReplaceBeg();
        for (int _ix = -_alterRadius; _ix <= _alterRadius; ++_ix)
        {
            for (int _iy = -_alterRadius; _iy <= _alterRadius; ++_iy)
            {
                for (int _iz = -_alterRadius; _iz <= _alterRadius; ++_iz)
                {
					if(isRounded && Mathf.Sqrt(_ix * _ix + _iy * _iy + _iz * _iz) > _filterSize * _filterSize)
					{
						continue;
					}
					ReplaceOneVoxel(_ix + _hitVoxelIdx.x,
					                _iy + _hitVoxelIdx.y,
					                _iz + _hitVoxelIdx.z,
					                _newVoxels[_ix + _alterRadius, _iy + _alterRadius, _iz + _alterRadius]);
                }
            }
        }
		ReplaceEnd();
    }

    //修改box范围内的植被
//    void ModifyBoxVegetation(bool _bAdd)
//    {
//        if (_bAdd)
//        {
//            int _minX = (int)(m_raycastHit.x - m_alterRadius), _maxX = (int)(m_raycastHit.x + m_alterRadius);
//            int _minZ = (int)(m_raycastHit.z - m_alterRadius), _maxZ = (int)(m_raycastHit.z + m_alterRadius);
//            Vector3 _buildPos = new Vector3();
//            int _buildNum = (int)(4 * m_alterRadius * m_alterRadius * m_buildDensity * 0.01f);
//            for (int _i = 0; _i != _buildNum; ++_i)
//            {
//                _buildPos.x = (float)UnityEngine.Random.Range(_minX, _maxX);
//                _buildPos.z = (float)UnityEngine.Random.Range(_minZ, _maxZ);
//                _buildPos.y = m_raycastHit.y + (float)m_alterRadius;
//
//                RaycastHit _raycastHit;
//                Ray _ray = new Ray(_buildPos, new Vector3(0.0f, -1.0f, 0.0f));
//                if (Physics.Raycast(_ray, out _raycastHit, m_alterRadius * 2.0f))
//                    AddTree(_raycastHit.point);
//            }
//        }
//        else
//        {
//            IntVector3 _hitVoxelIdx = new IntVector3(m_raycastHit);
//#if false			//VOXEL_OFFSET
//            _hitVoxelIdx.x += 1;
//            _hitVoxelIdx.y += 1;
//            _hitVoxelIdx.z += 1;
//#endif
//
//            int _alterRadius = (int)m_alterRadius;
//            for (int _ix = -_alterRadius; _ix <= _alterRadius; ++_ix)
//            {
//                for (int _iy = -_alterRadius; _iy <= _alterRadius; ++_iy)
//                {
//                    for (int _iz = -_alterRadius; _iz <= _alterRadius; ++_iz)
//                    {
//                        int _vx = _hitVoxelIdx.x + _ix;
//                        int _vy = _hitVoxelIdx.y + _iy;
//                        int _vz = _hitVoxelIdx.z + _iz;
//
//                        if (_vx < 0 || _vy < 0 || _vz < 0)
//                            continue;
//
//                        RemoveTree(new Vector3(_vx, _vy, _vz), false);
//                    }
//                }
//            }
//        }
//    }
//
//    //修改sphere范围内的植被
//    void ModifySphereVegetation(bool _bAdd)
//    {
//        if (_bAdd)
//        {
//            int _minX = (int)(m_raycastHit.x - m_alterRadius), _maxX = (int)(m_raycastHit.x + m_alterRadius);
//            int _minZ = (int)(m_raycastHit.z - m_alterRadius), _maxZ = (int)(m_raycastHit.z + m_alterRadius);
//            Vector3 _buildPos = new Vector3();
//            int _buildNum = (int)(Mathf.PI * m_alterRadius * m_alterRadius * m_buildDensity * 0.01f);
//            for (int _i = 0; _i != _buildNum; ++_i)
//            {
//                _buildPos.x = (float)UnityEngine.Random.Range(_minX, _maxX);
//                _buildPos.z = (float)UnityEngine.Random.Range(_minZ, _maxZ);
//
//                Vector2 _dist = new Vector2(_buildPos.x, _buildPos.z) - new Vector2(m_raycastHit.x, m_raycastHit.z);
//                if (_dist.sqrMagnitude > m_alterRadius * m_alterRadius)
//                    continue;
//
//                _buildPos.y = m_raycastHit.y + (float)m_alterRadius;
//
//                RaycastHit _raycastHit;
//                Ray _ray = new Ray(_buildPos, new Vector3(0.0f, -1.0f, 0.0f));
//				float prototypeSize = 4.0f;
//				
//                if (Physics.Raycast(_ray, out _raycastHit, m_alterRadius * 2.0f))
//				{
//					AddTree(_raycastHit.point);
//				}
//            }
//        }
//        else
//        {
//            IntVector3 _hitVoxelIdx = new IntVector3(m_raycastHit);
//#if false			//VOXEL_OFFSET
//            _hitVoxelIdx.x += 1;
//            _hitVoxelIdx.y += 1;
//            _hitVoxelIdx.z += 1;
//#endif
//
//            int _alterRadius = (int)m_alterRadius;
//            for (int _ix = -_alterRadius; _ix <= _alterRadius; ++_ix)
//            {
//                for (int _iy = -_alterRadius; _iy <= _alterRadius; ++_iy)
//                {
//                    for (int _iz = -_alterRadius; _iz <= _alterRadius; ++_iz)
//                    {
//                        int _vx = _hitVoxelIdx.x + _ix;
//                        int _vy = _hitVoxelIdx.y + _iy;
//                        int _vz = _hitVoxelIdx.z + _iz;
//
//                        if (_vx < 0 || _vy < 0 || _vz < 0)
//                            continue;
//
//                        Vector3 _dist = new Vector3(_vx, _vy, _vz) - new Vector3(_hitVoxelIdx.x, _hitVoxelIdx.y, _hitVoxelIdx.z);
//                        float _distSq = _dist.sqrMagnitude;
//                        if (_distSq > m_alterRadius * m_alterRadius)
//                            continue;
//
//                        RemoveTree(new Vector3(_vx, _vy, _vz), false);
//                    }
//                }
//            }
//        }
//    }
//    void ModifyTerrainVegetation(bool _bAdd)
//    {
//        if (_bAdd)
//        {
//            int _minX = (int)(m_raycastHit.x - m_alterRadius), _maxX = (int)(m_raycastHit.x + m_alterRadius);
//            int _minZ = (int)(m_raycastHit.z - m_alterRadius), _maxZ = (int)(m_raycastHit.z + m_alterRadius);
//            Vector3 _buildPos = new Vector3();
//            int _buildNum = (int)(Mathf.PI * m_alterRadius * m_alterRadius * m_buildDensity * 0.01f);
//            for (int _i = 0; _i != _buildNum; ++_i)
//            {
//                _buildPos.x = (float)UnityEngine.Random.Range(_minX, _maxX);
//                _buildPos.z = (float)UnityEngine.Random.Range(_minZ, _maxZ);
//
//                Vector2 _dist = new Vector2(_buildPos.x, _buildPos.z) - new Vector2(m_raycastHit.x, m_raycastHit.z);
//                if (_dist.sqrMagnitude > m_alterRadius * m_alterRadius)
//                    continue;
//
//                _buildPos.y = m_raycastHit.y + (float)m_alterRadius;
//
//                RaycastHit _raycastHit;
//                Ray _ray = new Ray(_buildPos, new Vector3(0.0f, -1.0f, 0.0f));
//				float prototypeSize = 4.0f;
//				
//                if (Physics.Raycast(_ray, out _raycastHit, m_alterRadius * 2.0f))
//				{
//					AddTree(_raycastHit.point);
//				}
//            }
//        }
//        else
//        {
//            IntVector3 _hitVoxelIdx = new IntVector3(m_raycastHit);
//#if false			//VOXEL_OFFSET
//            _hitVoxelIdx.x += 1;
//            _hitVoxelIdx.y += 1;
//            _hitVoxelIdx.z += 1;
//#endif
//
//            int _alterRadius = (int)m_alterRadius;
//            for (int _ix = -_alterRadius; _ix <= _alterRadius; ++_ix)
//            {
//                for (int _iy = -_alterRadius; _iy <= _alterRadius; ++_iy)
//                {
//                    for (int _iz = -_alterRadius; _iz <= _alterRadius; ++_iz)
//                    {
//                        int _vx = _hitVoxelIdx.x + _ix;
//                        int _vy = _hitVoxelIdx.y + _iy;
//                        int _vz = _hitVoxelIdx.z + _iz;
//
//                        if (_vx < 0 || _vy < 0 || _vz < 0)
//                            continue;
//
//                        Vector3 _dist = new Vector3(_vx, _vy, _vz) - new Vector3(_hitVoxelIdx.x, _hitVoxelIdx.y, _hitVoxelIdx.z);
//                        float _distSq = _dist.sqrMagnitude;
//                        if (_distSq > m_alterRadius * m_alterRadius)
//                            continue;
//
//                        RemoveTree(new Vector3(_vx, _vy, _vz), false);
//                    }
//                }
//            }
//        }
//    }

	byte[,,] noiseVol;
	void genNoiseCube(int cx, int cy, int cz, int radius, float opacity)
	{
		UnityEngine.Random.seed = ms_iResSeed;
		if(noiseVol == null )
		{
		}
		else
		{
			noiseVol = null;
		}
		noiseVol = new byte[radius * 2 + 1, radius * 2 + 1, radius * 2 + 1];
		int maxX = cx + radius;
		int maxY = cy + radius;
		int maxZ = cz + radius;
		
		int minX = (cx - radius) < 0 ? 0: cx - radius;
		int minY = (cy - radius) < 0 ? 0: cy - radius;
		int minZ = (cz - radius) < 0 ? 0: cz - radius;
		//m_NoiseStrength = 2.0f;
		
		for(int x = minX; x <= maxX; x++)
		{
			for(int y = minY; y <= maxY; y++)
			{
				for(int z = minZ; z <= maxZ; z++)
				{
					
					float dist = Mathf.Sqrt(Mathf.Pow((x - cx), 2) + Mathf.Pow((y - cy), 2) + Mathf.Pow((z - cz), 2));
					float vol;
					float ratio = radius / dist;
					if(ratio <= 1 )
					{
						ratio = Mathf.Pow(ratio, m_NoiseStrength) * 255.0f;
						ratio = Mathf.Clamp((int)ratio, 0, 255);
						vol = UnityEngine.Random.value * ratio;
					}
					else
					{
						ratio *= 127.0f;
						ratio = Mathf.Clamp((int)ratio, 0, 127);
						vol = 128 + UnityEngine.Random.value * ratio;
					}
					int intVol = Mathf.RoundToInt(vol);
					if(intVol < 128)
						intVol = 0;
				
					noiseVol[x - minX, y - minY, z - minZ] = (byte)intVol;
				}
			}
		}
	}
	void growNoise(Vector3 _hitVoxelPos, int _fGrowth, bool _bChangeType, byte _type)
	{
//		IntVector3 _chunkIdx = new IntVector3();
        IntVector3 _hitVoxelIdx = new IntVector3(_hitVoxelPos);
#if false        //VOXEL_OFFSET
        _hitVoxelIdx.x += 1;
        _hitVoxelIdx.y += 1;
        _hitVoxelIdx.z += 1;
#endif

        int _alterRadius = (int)m_alterRadius;
		genNoiseCube(_alterRadius, _alterRadius, _alterRadius, _alterRadius, 0.5f);
		ReplaceBeg();
        for (int _ix = -_alterRadius; _ix <= _alterRadius; ++_ix)
        {
            for (int _iy = -_alterRadius; _iy <= _alterRadius; ++_iy)
            {
                for (int _iz = -_alterRadius; _iz <= _alterRadius; ++_iz)
                {
                    int _vx = _hitVoxelIdx.x + _ix;
                    int _vy = _hitVoxelIdx.y + _iy;
                    int _vz = _hitVoxelIdx.z + _iz;

                    if (_vx < 0 || _vy < 0 || _vz < 0)
                        continue;

                    //修改地形数据
                    //RemoveTree(new Vector3(_vx, _vy, _vz), false);
					VFVoxel _srcVoxel = m_voxelTerrain.Voxels.SafeRead(_vx, _vy, _vz);
					if( (int)_srcVoxel.Type == constVoxelType )
						continue;
                    
//					if(_srcVoxel.Volume < 127)
//							_srcVoxel.Volume = (byte)(Mathf.RoundToInt(UnityEngine.Random.value * 127.0f));
//					
//					if(_srcVoxel.Volume > 128 && _srcVoxel.Volume < 255) 
//							_srcVoxel.Volume = (byte)(128 + Mathf.RoundToInt(UnityEngine.Random.value * 127.0f));
					int afterVol = _srcVoxel.Volume + _fGrowth * noiseVol[_ix + _alterRadius, _iy + _alterRadius, _iz + _alterRadius] /10;
					if (_bChangeType )
                    {
						if(_srcVoxel.Volume >= 127)
                        	_srcVoxel.Type = _type;
                    }
                    afterVol = Mathf.Clamp(afterVol, 0, 255);
                    _srcVoxel.Volume = (byte)afterVol;

					ReplaceOneVoxel(_vx, _vy, _vz, _srcVoxel);
                }
            }
        }
		ReplaceEnd();
	}
	VFVoxel getVoxelFromRayHit(Vector3 _hitVoxelPos, out int outX, out int outY, out int outZ)
	{
		IntVector3 _chunkIdx = new IntVector3();
        IntVector3 _hitVoxelIdx = new IntVector3(_hitVoxelPos);
#if false        //VOXEL_OFFSET
        _hitVoxelIdx.x += 1;
        _hitVoxelIdx.y += 1;
        _hitVoxelIdx.z += 1;
#endif
		Vector3 upvec = _hitVoxelPos;
		_hitVoxelPos.y += 0.5f;
		Debug.DrawLine(_hitVoxelPos, upvec);
		int _vx = _hitVoxelIdx.x + 0;
        int _vy = _hitVoxelIdx.y + 0;
        int _vz = _hitVoxelIdx.z + 0;

        _chunkIdx.x = _vx >> m_shift;
        _chunkIdx.y = _vy >> m_shift;
        _chunkIdx.z = _vz >> m_shift;

        VFVoxelChunkData _vc = m_voxelTerrain.Voxels.readChunk(_chunkIdx.x, _chunkIdx.y, _chunkIdx.z);
		outX = _vx;
		outY = _vy;
		outZ = _vz;
		
		_vx = (_vx & m_voxelNumPerChunkMask);
        _vy = (_vy & m_voxelNumPerChunkMask);
        _vz = (_vz & m_voxelNumPerChunkMask);

        VFVoxel _srcVoxel = _vc.ReadVoxelAtIdx(_vx, _vy, _vz);
		return _srcVoxel;
	}
	void PlantFilter(RaycastHit _hitInfo, int _fGrowth, bool _bChangeType, byte _type)
	{
		UnityEngine.Random.seed = ms_iResSeed;
		if(FilterMap == null)
		{
			Debug.Log("No filter map specified.");
			return;
		}
		
		IVxDataSource _vds = m_voxelTerrain.Voxels;
		IntVector3 _chunkIdx = new IntVector3(0,0,0);
        IntVector3 _hitVoxelIdx = new IntVector3(_hitInfo.point);
        //voxel编号加偏移位置1
		
		Vector3 upvec = new Vector3(0,0,0);
		upvec.Set(_hitVoxelIdx.x, _hitVoxelIdx.y + 1.5f, _hitVoxelIdx.z);
		Debug.DrawLine(_hitVoxelIdx, upvec);
		
		int _vx = _hitVoxelIdx.x + 0;
        int _vy = _hitVoxelIdx.y + 0;
        int _vz = _hitVoxelIdx.z + 0;
		
		_chunkIdx.x = _vx >> m_shift;
        _chunkIdx.y = _vy >> m_shift;
        _chunkIdx.z = _vz >> m_shift;

		Color[] filterColorArray;
		filterColorArray = FilterMap.GetPixels();
		int fmSize = FilterMap.width;
		Vector3 origin = _hitVoxelIdx;
		
		origin.y += shootingOffset;
		
		float smRed = 65535;
		float lgRed = 0;
		ReplaceBeg();
		for (int _iy = -fmSize / 2; _iy < fmSize / 2; ++_iy)
		{
            for (int _ix = -fmSize / 2; _ix < fmSize / 2; ++_ix)
			{
				Vector3 thisOrigin = origin;
				thisOrigin.x += _ix;
				thisOrigin.z += _iy;
				Ray ray = new Ray(thisOrigin, new Vector3(0, -1, 0));
				RaycastHit hitInfo;
				// get the gradient
				int colorX;
				int colorY;
				colorX = _ix + fmSize / 2;
				colorY = _iy + fmSize / 2;
				Color c = filterColorArray[colorY * fmSize + colorX];
				if(c.r == 0) continue;
				
				if(Physics.Raycast(ray, out hitInfo, shootingOffset * 2) )
				{
					VFVoxel vox;
					IntVector3 outXYZ;
					vox = m_voxelTerrain.GetRaycastHitVoxel(hitInfo, out outXYZ);
					//vox = getVoxelFromRayHit(hitInfo.point, out outX, out outY, out outZ );
					
					int amount = Mathf.RoundToInt( c.r * 255 ) / PlantFilterWeightDivisor;
					
					if(c.r < smRed)
						smRed = c.r;
					if(c.r > lgRed)
						lgRed = c.r;
					
					if( (int)vox.Volume + amount > 255 )
						vox.Volume = 255;
					else
						vox.Volume += (byte)amount;
					vox.Type = _type;

					ReplaceOneVoxel(outXYZ.x, outXYZ.y, outXYZ.z, vox);
					
					for(int zz = -1; zz < 2; zz++ )
					{
						for(int yy = -1; yy < 2; yy++ )
						{
							for(int xx = -1; xx < 2; xx++ )
							{
								int neighbourVoxelX = (outXYZ.x + xx);// & m_voxelNumPerChunkMask;
								int neighbourVoxelY = (outXYZ.y + yy);// & m_voxelNumPerChunkMask;
								int neighbourVoxelZ = (outXYZ.z + zz);// & m_voxelNumPerChunkMask;
								
								VFVoxel nVox = _vds.SafeRead(neighbourVoxelX, neighbourVoxelY, neighbourVoxelZ);
									
								int manhattanDistance = Math.Abs(xx) + Math.Abs(yy) + Math.Abs(zz);
								if(manhattanDistance == 0)
									continue;
								int amm;
								amm = amount * Mathf.RoundToInt((UnityEngine.Random.value) / (float)manhattanDistance);
								amm *= EdgeWeight;
								//print(manhattanDistance + " : " + amm);
								if( (int)nVox.Volume + amm > 255 )
									nVox.Volume = 255;
								else
									nVox.Volume += (byte)amm;
								nVox.Type = _type;
								
								ReplaceOneVoxel(outXYZ.x + xx, outXYZ.y + yy, outXYZ.z + zz, nVox);
							}
						}
					}
				}
			}
		}
		ReplaceEnd();
		print("smRed : " + smRed );
		print("lgRed : " + lgRed );
	}

    //修改box范围内的voxel
    public void ModifyBoxVoxel(Vector3 _hitVoxelPos, float _fAlterVol, bool _bChangeType, byte _type, bool removeVegetation)
    {
//        IntVector3 _chunkIdx = new IntVector3();
        IntVector3 _hitVoxelIdx = new IntVector3(_hitVoxelPos);
#if false        //VOXEL_OFFSET
        _hitVoxelIdx.x += 1;
        _hitVoxelIdx.y += 1;
        _hitVoxelIdx.z += 1;
#endif
		IVxDataSource _vds = m_voxelTerrain.Voxels;
        int _alterRadius = (int)m_alterRadius;
		ReplaceBeg();
        for (int _ix = -_alterRadius; _ix <= _alterRadius; ++_ix)
        {
            for (int _iy = -_alterRadius; _iy <= _alterRadius; ++_iy)
            {
                for (int _iz = -_alterRadius; _iz <= _alterRadius; ++_iz)
                {
                    int _vx = _hitVoxelIdx.x + _ix;
                    int _vy = _hitVoxelIdx.y + _iy;
                    int _vz = _hitVoxelIdx.z + _iz;

                    if (_vx < 0 || _vy < 0 || _vz < 0)
                        continue;

                    //修改地形数据
                    if(removeVegetation)
					{
                        RemoveTree(new IntVector3(_vx, _vy, _vz));
					}

					VFVoxel _srcVoxel = _vds.SafeRead(_vx, _vy, _vz);
					if( (int)_srcVoxel.Type == constVoxelType )
						continue;
                    if (_bChangeType )
                    {
                        _srcVoxel.Type = _type;
                    }
                    float _vol = (float)_srcVoxel.Volume + _fAlterVol;
                    _vol = Mathf.Clamp(_vol, 0.0f, 255.0f);
                    _srcVoxel.Volume = (byte)_vol;
					ReplaceOneVoxel(_vx, _vy, _vz, _srcVoxel);
				}
            }
        }
		ReplaceEnd();
		if(removeVegetation)
		{
//			GrassMgr.RefreshDirtyChunks();
		}
    }

    //修改sphere范围内的voxel
    public void ModifySphereVoxel(Vector3 _hitVoxelPos, float _fAlterVol, bool _bChangeType, byte _type, bool removeVegetation)
    {
//        IntVector3 _chunkIdx = new IntVector3();

		IntVector3 _hitVoxelIdx = new IntVector3(_hitVoxelPos);
#if false        //VOXEL_OFFSET
        _hitVoxelIdx.x += 1;
        _hitVoxelIdx.y += 1;
        _hitVoxelIdx.z += 1;
#endif

		IVxDataSource _vds = m_voxelTerrain.Voxels;
		float _sqRadius = m_alterRadius * m_alterRadius;
        int _alterRadius = (int)m_alterRadius;
		ReplaceBeg();
        for (int _ix = -_alterRadius; _ix <= _alterRadius; ++_ix)
        {
            for (int _iy = -_alterRadius; _iy <= _alterRadius; ++_iy)
            {
                for (int _iz = -_alterRadius; _iz <= _alterRadius; ++_iz)
                {
                    int _vx = _hitVoxelIdx.x + _ix;
                    int _vy = _hitVoxelIdx.y + _iy;
                    int _vz = _hitVoxelIdx.z + _iz;

                    if (_vx < 0 || _vy < 0 || _vz < 0)
                        continue;

                    //return if out of sphere scope
                    Vector3 _dist = new Vector3(_vx, _vy, _vz) - new Vector3(_hitVoxelIdx.x, _hitVoxelIdx.y, _hitVoxelIdx.z);
                    float _distSq = _dist.sqrMagnitude;
                    if (_distSq > _sqRadius)
                        continue;

                    //修改地形数据
					if(removeVegetation)
					{
                        RemoveTree(new IntVector3(_vx, _vy, _vz));
					}

					VFVoxel _srcVoxel = _vds.SafeRead(_vx, _vy, _vz);
					if( (int)_srcVoxel.Type == constVoxelType )
						continue;
					
                    if (_bChangeType )
                    {
                        _srcVoxel.Type = _type;
                    }
                    float _sqDist = _ix * _ix + _iy * _iy + _iz * _iz;
                    float _ratio = (1.0f - _sqDist / _sqRadius);
                    float _vol = (float)_srcVoxel.Volume + _fAlterVol * _ratio;
                    _vol = Mathf.Clamp(_vol, 0.0f, 255.0f);
                    _srcVoxel.Volume = (byte)_vol;

					ReplaceOneVoxel(_vx, _vy, _vz, _srcVoxel);
                }
            }
        }
		ReplaceEnd();
		if(removeVegetation)
		{
//			GrassMgr.RefreshDirtyChunks();
		}
    }
	
	//修改Terrain Mask范围内的voxel volume
    public void ModifyTerrainVoxel(Vector3 _hitVoxelPos, float _fAlterVol, bool _bChangeType, byte _type, bool removeVegetation)
    {
		IntVector3 _hitVoxelIdx = new IntVector3(_hitVoxelPos);
#if false        //VOXEL_OFFSET
        _hitVoxelIdx.x += 1;
        _hitVoxelIdx.y += 1;
        _hitVoxelIdx.z += 1;
#endif
		
        int _alterRadius = (int)m_alterRadius;
		if(terBrushTextures[terBrushType] == null){
			Debug.LogError("No ter brush filter texture specified.");
			return;
		}
		
		if(removeVegetation)
		{
	        for (int _ix = -_alterRadius; _ix <= _alterRadius; ++_ix)
	        {
	            for (int _iy = -_alterRadius; _iy <= _alterRadius; ++_iy)
	            {
	                for (int _iz = -_alterRadius; _iz <= _alterRadius; ++_iz)
	                {
	                    int _vx = _hitVoxelIdx.x + _ix;
	                    int _vy = _hitVoxelIdx.y + _iy;
	                    int _vz = _hitVoxelIdx.z + _iz;
	                    RemoveTree(new IntVector3(_vx, _vy, _vz));
					}
				}
			}
//			GrassMgr.RefreshDirtyChunks();
		}
		
		Vector3 rayStart;
		Vector3 rayDir = ((Vector3)_hitVoxelIdx) - Camera.main.transform.position;
		float rayLen = rayDir.magnitude;	// so can not use this in cave
		rayDir /= rayLen;
		List<VFVoxelChunkGo> chunksToAlter = new List<VFVoxelChunkGo>();
		RaycastHit hitInfo;
		ReplaceBeg();
		for(int ix = -_alterRadius; ix <= _alterRadius; ix ++)
		{
			for(int iz = -_alterRadius; iz <= _alterRadius; iz ++)
			{
				rayStart = Camera.main.transform.position + Camera.main.transform.right*ix + Camera.main.transform.up*iz;
				if(Physics.Raycast(rayStart, rayDir, out hitInfo, rayLen*2, 1<<Pathea.Layer.VFVoxelTerrain))
				{
					VFVoxelChunkGo chunk = hitInfo.transform.gameObject.GetComponent<VFVoxelChunkGo>();
					bool bInAltering = chunksToAlter.Contains(chunk);
					if(!bInAltering /* && VFVoxelChunkGoCreator.IsChunkInBuild(chunk._data)*/)
						return;
					if(!bInAltering)	chunksToAlter.Add(chunk);

					//byte voxelType = _bChangeType ? _type : ;
					int iu = _alterRadius + ix;
					int iv = _alterRadius + iz;
					float u = (iu) / (float)(_alterRadius+_alterRadius);
					float v = (iv) / (float)(_alterRadius+_alterRadius);
					int altPower = (int)(_fAlterVol*terBrushTextures[terBrushType].GetPixelBilinear(u,v).a);
					
					int nVoxel = 0;
					IntVector3 vpos;
					VFVoxel voxel;

					if(altPower > 0)
					{
						while(altPower!=0)
						{
							vpos = hitInfo.point - nVoxel*rayDir;
							nVoxel++;
							voxel = m_voxelTerrain.Voxels.Read(vpos.x, vpos.y, vpos.z);
							if(voxel.Volume + altPower > 255)
							{
								altPower -= 255-voxel.Volume;
								voxel.Volume = 255;
								voxel.Type = 3;
							}
							else
							{
								voxel.Volume = (byte)(voxel.Volume + altPower);
								voxel.Type = 3;
								altPower = 0;
							}
							ReplaceOneVoxel(vpos.x, vpos.y, vpos.z, voxel);
						}
					}
					else if(altPower < 0)
					{
						while(altPower!=0)
						{
							vpos = hitInfo.point + nVoxel*rayDir;
							nVoxel++;
							voxel = m_voxelTerrain.Voxels.Read(vpos.x, vpos.y, vpos.z);
							if(voxel.Volume + altPower < 0)
							{
								altPower += voxel.Volume;
								voxel.Volume = 0;
								voxel.Type = 0;
							}
							else
							{
								voxel.Volume = (byte)(voxel.Volume + altPower);
								altPower = 0;
							}
							ReplaceOneVoxel(vpos.x, vpos.y, vpos.z, voxel);
						}						
					}
				}
				else
				{
					Debug.LogWarning("[VoxelEditor]:Raycast voxelTerrain failed at "+rayStart);
					continue;
				}
			}
		}
		ReplaceEnd();
	}
	//修改Terrain Mask范围内的 terrain height
    public void ModifyTerrainHeight(Vector3 _hitVoxelPos, float _fAlterVol, bool _bChangeType, byte _type, bool removeVegetation)
    {
		IntVector3 _hitVoxelIdx = new IntVector3(_hitVoxelPos);
#if false        //VOXEL_OFFSET
        _hitVoxelIdx.x += 1;
        _hitVoxelIdx.y += 1;
        _hitVoxelIdx.z += 1;
#endif
		_bChangeType = false;
		if(vxMatXmlParser == null)
		{
			vxMatXmlParser = new VoxelPaintXMLParser();
			vxMatXmlParser.LoadXMLAtPath();
		}	
		
        int _alterRadius = (int)m_alterRadius;
		if(terBrushTextures[terBrushType] == null){
			Debug.LogError("No ter brush filter texture specified.");
			return;
		}
		
		if(removeVegetation)
		{
	        for (int _ix = -_alterRadius; _ix <= _alterRadius; ++_ix)
	        {
	            for (int _iy = -_alterRadius; _iy <= _alterRadius; ++_iy)
	            {
	                for (int _iz = -_alterRadius; _iz <= _alterRadius; ++_iz)
	                {
	                    int _vx = _hitVoxelIdx.x + _ix;
	                    int _vy = _hitVoxelIdx.y + _iy;
	                    int _vz = _hitVoxelIdx.z + _iz;
						RemoveTree(new IntVector3(_vx, _vy, _vz));
					}
				}
			}
//			GrassMgr.RefreshDirtyChunks();
		}
		
		float[,] srcHeight = new float[(2*_alterRadius+1),(2*_alterRadius+1)];
		float[,] dstHeight = new float[(2*_alterRadius+1),(2*_alterRadius+1)];
		Vector3 rayStart;
		RaycastHit hitInfo;
		float rayLen = 1000;	// so can not use this in cave
		for(int ix = -_alterRadius; ix <= _alterRadius; ix ++)
		{
			for(int iz = -_alterRadius; iz <= _alterRadius; iz ++)
			{
				rayStart = new Vector3(_hitVoxelIdx.x + ix+0.01f, _hitVoxelIdx.y + rayLen, _hitVoxelIdx.z + iz+0.01f);
				if(Physics.Raycast(rayStart, Vector3.down, out hitInfo, rayLen*2, 1<<Pathea.Layer.VFVoxelTerrain))
				{
					int iu = _alterRadius + ix;
					int iv = _alterRadius + iz;
					float u = (iu) / (float)(_alterRadius+_alterRadius);
					float v = (iv) / (float)(_alterRadius+_alterRadius);
					int altPower = (int)(_fAlterVol*terBrushTextures[terBrushType].GetPixelBilinear(u,v).a);
					srcHeight[_alterRadius + ix, _alterRadius + iz] = hitInfo.point.y;
					dstHeight[_alterRadius + ix, _alterRadius + iz] = hitInfo.point.y + altPower/128.0f;
				}
				else
				{
					Debug.LogWarning("[VoxelEditor]:Raycast voxelTerrain failed.");
					return;
				}
			}
		}
		
		//VFVoxel voxelSolid = new VFVoxel(255, _type);
		VFVoxel voxelHollow = new VFVoxel(0);
		ReplaceBeg();
		for(int vx = _hitVoxelIdx.x - _alterRadius + 1; vx < _hitVoxelIdx.x + _alterRadius; vx++)
		{
			for(int vz = _hitVoxelIdx.z - _alterRadius + 1; vz < _hitVoxelIdx.z + _alterRadius; vz++)
			{
				int hx = vx-(_hitVoxelIdx.x - _alterRadius);
				int hz = vz-(_hitVoxelIdx.z - _alterRadius);
				float fSrcH = srcHeight[hx,hz];
				float fDstH = dstHeight[hx,hz];
				int iSrcH = (int)(fSrcH + 0.5f);
				int iDstH = (int)(fDstH + 0.5f);
				byte voxelType = _bChangeType ? _type : vxMatXmlParser.GetVxMatByGradient(fDstH,
													dstHeight[hx-1,hz], dstHeight[hx+1,hz],
													dstHeight[hx,hz-1], dstHeight[hx,hz+1],
													vx, vz);
				float _fDec = fDstH - (int)fDstH;
				if(_fDec < 0.5f)
				{
					ReplaceOneVoxel(vx,iDstH,vz,new VFVoxel((byte)(255.999f*0.5/(1-_fDec)), voxelType));
					ReplaceOneVoxel(vx,iDstH-1,vz,new VFVoxel(255, voxelType));
				}
				else
				{
					ReplaceOneVoxel(vx,iDstH,vz,new VFVoxel((byte)(255.999f*(1-0.5/_fDec)), voxelType));
					ReplaceOneVoxel(vx,iDstH-1,vz,new VFVoxel(255, voxelType));
				}
				int iH;
				for(iH = iSrcH; iH < iDstH; iH++)
				{
					ReplaceOneVoxel(vx,iH,vz,new VFVoxel(255, voxelType));
				}
				for(iH = iSrcH; iH > iDstH; iH--)
				{
					ReplaceOneVoxel(vx,iH,vz,voxelHollow);
				}
			}
		}
		ReplaceEnd();
    }
	
	static float lastOp = 0.0f;
	float isSurfaceFlat(Vector3 startPos, float prototypeSize) // flat not in the absolute sense
	{
		// four other rays
		RaycastHit[] _surroundingRaycastHit;
		_surroundingRaycastHit = new RaycastHit[4];
        Ray[] _surroundingRays = new Ray[4];
		
		_surroundingRays[0] = new Ray(startPos + new Vector3(-1.0f, 3.0f, -1.0f) * prototypeSize, new Vector3(0.0f, -1.0f, 0.0f));
		_surroundingRays[1] = new Ray(startPos + new Vector3(1.0f, 3.0f, -1.0f) * prototypeSize, new Vector3(0.0f, -1.0f, 0.0f));
		_surroundingRays[2] = new Ray(startPos + new Vector3(1.0f, 3.0f, 1.0f) * prototypeSize, new Vector3(0.0f, -1.0f, 0.0f));
		_surroundingRays[3] = new Ray(startPos + new Vector3(-1.0f, 3.0f, 1.0f) * prototypeSize, new Vector3(0.0f, -1.0f, 0.0f));
		
		
		// calculate the tangents/slopes
		float baseHeight = startPos.y;
		float[] tangents = new float[4];
		float tangentSum = 0.0f;
		float heightSum = 0.0f;
		//float lowest = 65535.0f;
		for(int i = 0; i < 4; i++ )
		{
			
			Physics.Raycast(_surroundingRays[i], out _surroundingRaycastHit[i], prototypeSize * 8.0f);
			
			tangents[i] = (_surroundingRaycastHit[i].point.y - baseHeight ) / prototypeSize;
			tangentSum += Mathf.Abs( tangents[i] );
			heightSum += _surroundingRaycastHit[i].point.y;
		}
		if(tangentSum < 2.4f)
		{
			print("slope ok " + tangentSum);
			return (heightSum + startPos.y) / 5.0f;
		}
		print("slope reached for " + tangentSum);
		return -10.0f;
	}	
#endif
	//自动填充vegetation
    public void AutoFillVegetation()
    {
#if false	//w 
        //得到当前voxelTerrain加载进来的地形范围
		Vector3 vecMin = m_voxelTerrain.ViewBounds.min;
		Vector3 vecMax = m_voxelTerrain.ViewBounds.max;
    
        int _iCnt = (int)(SubTerrain.s_subTerSize.x / m_SubTerCellSize);
		int _iMinX = (int)vecMin.x;
		int _iMinZ = (int)vecMin.z;
		int _iMaxX = (int)vecMax.x;
        int _iMaxZ = (int)vecMax.z;
        int _buildNum = (int)(m_SubTerCellSize * m_SubTerCellSize * m_buildDensity * 0.01f);
        for (int _iZ = _iMinZ; _iZ != _iMaxZ; ++_iZ)
        {
            for (int _iX = _iMinX; _iX != _iMaxX; ++_iX)
            {
                int _iSubTerRow = _iZ / _iCnt;
                int _iSubTerCol = _iX / _iCnt;

                int _subTerIdx = (_iSubTerRow << 16) + _iSubTerCol;
                SubTerrain _subTer = null;
                if (m_mapSubTer.TryGetValue(_subTerIdx, out _subTer))
                {
                    int _iCellRow = _iZ % _iCnt;
                    int _iCellCol = _iX % _iCnt;

                    //只有当子地形中的_iCellRow和_iCellCol对应填充标志不存在时才启用植被种植功能
                    if (_subTer.GetCellFillFlag(_iCellRow, _iCellCol) == 0)
                    {
                        for (int _i = 0; _i != _buildNum; ++_i)
                        {
                            float _minX = _iX * m_SubTerCellSize;
                            float _maxX = _minX + m_SubTerCellSize;
                            float _minZ = _iZ * m_SubTerCellSize;
                            float _maxZ = _minZ + m_SubTerCellSize;

                            Vector3 _pos = new Vector3();
                            _pos.x = UnityEngine.Random.Range(_minX, _maxX);
                            _pos.z = UnityEngine.Random.Range(_minZ, _maxZ);
                            _pos.y = SubTerrain.s_subTerSize.y;

                            //通过射线查询的方式添加植被
                            Ray _ray = new Ray(_pos, new Vector3(0.0f, -1.0f, 0.0f));
                            RaycastHit _raycastHit;
                            if (Physics.Raycast(_ray, out _raycastHit, SubTerrain.s_subTerSize.y))
                            {
                                float _angle = Vector3.Angle(_raycastHit.normal, new Vector3(0.0f, 1.0f, 0.0f));
                                if (_angle < m_maxBuildAngle)
                                    AddTree(_raycastHit.point);
                            }
                        }
                    }
                }
            }
        }
#endif
    }

}

