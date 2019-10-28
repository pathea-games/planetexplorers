using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using Object = UnityEngine.Object;


public class AssetPRS
{
	public static AssetPRS Zero = new AssetPRS (Vector3.zero, Quaternion.identity, Vector3.one);

    [XmlAttribute("X")]
    public float x { get; set; }
    [XmlAttribute("Y")]
    public float y { get; set; }
    [XmlAttribute("Z")]
    public float z { get; set; }

    [XmlAttribute("RotX")]
    public float rx { get; set; }
    [XmlAttribute("RotY")]
    public float ry { get; set; }
    [XmlAttribute("RotZ")]
    public float rz { get; set; }
    [XmlAttribute("RotW")]
    public float rw { get; set; }

    [XmlAttribute("ScaleX")]
    public float sx { get; set; }
    [XmlAttribute("ScaleY")]
    public float sy { get; set; }
    [XmlAttribute("ScaleZ")]
    public float sz { get; set; }

    public Vector3 Position()
    {
        return new Vector3(x, y, z);
    }
    public Quaternion Rotation()
    {
        return new Quaternion(rx, ry, rz, rw);
    }
    public Vector3 Scale()
    {
        return new Vector3(sx, sy, sz);
    }
    public bool Equals(AssetPRS obj)
    {
		return 	Math.Abs( x-obj.x )<PETools.PEMath.Epsilon && Math.Abs( y-obj.y )<PETools.PEMath.Epsilon && Math.Abs( z-obj.z )<PETools.PEMath.Epsilon &&
				Math.Abs(sx-obj.sx)<PETools.PEMath.Epsilon && Math.Abs(sy-obj.sy)<PETools.PEMath.Epsilon && Math.Abs(sz-obj.sz)<PETools.PEMath.Epsilon &&
				Math.Abs(rx-obj.rx)<PETools.PEMath.Epsilon && Math.Abs(rz-obj.rz)<PETools.PEMath.Epsilon && Math.Abs(ry-obj.ry)<PETools.PEMath.Epsilon && Math.Abs(rw-obj.rw)<PETools.PEMath.Epsilon;
    }
    public AssetPRS() { }
    public AssetPRS(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        x = position.x; y = position.y; z = position.z;
        sx = scale.x; sy = scale.y; sz = scale.z;
        rx = rotation.x; ry = rotation.y; rz = rotation.z; rw = rotation.w;
    }
}

public class AssetReq
{
	public enum ProcLvl
	{
		DoNothing,
		DoLoading,
		DoInstantiation,
	}
    public AssetPRS Prs;    
	public string	PathName;
	public ProcLvl	ProcLevel;
	public bool		NeedCaching;
	public bool   isActive;

    public AssetReq() { }
	public AssetReq(string assetPathName, AssetPRS assetPos, ProcLvl procLvl = ProcLvl.DoInstantiation, bool bCaching = true)
    {
        PathName = assetPathName;
        Prs = assetPos;
		ProcLevel = procLvl;
		NeedCaching = bCaching;
    }
	public AssetReq(string assetPathName, ProcLvl procLvl = ProcLvl.DoLoading, bool bCaching = true)
	{
		PathName = assetPathName;
		Prs = AssetPRS.Zero;
		ProcLevel = procLvl;
		NeedCaching = bCaching;
	}
	public void Deactivate()
	{
		ProcLevel = ProcLvl.DoNothing;
	}

    public override bool Equals(object obj)
    {
        AssetReq other = obj as AssetReq;
        if (other == null)
            return false;

        return PathName.Equals(other.PathName) && Prs.Equals(other.Prs);
    }

    public delegate void ReqFinishDelegate(GameObject go);
    public event ReqFinishDelegate ReqFinishHandler;

    internal void OnFinish(GameObject go)
    {
        if (ReqFinishHandler != null)
        {
            ReqFinishHandler(go);
        }		
    }

    public static List<AssetReq> ConvertToReq(List<AssetBundleDesc> descList)
    {
        List<AssetReq> reqList = new List<AssetReq>();
        foreach (AssetBundleDesc desc in descList)
        {
            int len = desc.pos.Length;
            for (int i = 0; i < len; i++)
            {
                reqList.Add(new AssetReq(desc.pathName, desc.pos[i]));
            }
        }
        return reqList;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

public static class AssetsPool
{
    public static readonly int s_Version = 108;	//beta 1.0.7
	static readonly string[] s_PooledPrefabs = new string[]{
		"Prefabs/CursorState",
		"Prefab/Particle/dengshuangzhu/Male_RunSwordAttack",
		"Prefab/Particle/dengshuangzhu/Female_RunSwordAttack",
		"Prefab/Particle/fx_grasslandAmbient01",
		"Prefab/Particle/FX_insects_01",
		"Prefab/Particle/FX_walkingOnDirt",
		"Prefab/Particle/FX_walkingOnGrass",
		"Prefab/Particle/FX_enemyFall",
		"Prefab/Particle/FX_enemyFall_large",
		"Prefab/Particle/FX_enemyHit_critical",
		Pathea.PeEntityCreator.PlayerPrefabPath,
		Pathea.PeEntityCreator.NpcPrefabPath,
		Pathea.PeEntityCreator.NpcPrefabNativePath,
		Pathea.PeEntityCreator.TowerPrefabPath,
		Pathea.PeEntityCreator.DoodadPrefabPath,
		Pathea.PeEntityCreator.MonsterPrefabPath,
		Pathea.PeEntityCreator.GroupPrefabPath,
		Pathea.EntityInfoCmpt.OverHeadPrefabPath,
		Pathea.Motion_Equip.GlovesPrefabPath,
		FecesModelPath.PATH_01,
		FecesModelPath.PATH_02,
		FecesModelPath.PATH_03,
	};

	//static Object[] s_preloadAnimClips = null;
	//static Object[] s_preloadMaterials = null;
    static Dictionary<string, Object> s_CacheMap;
    static AssetsPool()
    {
        s_CacheMap = new Dictionary<string, Object>();
    }

	public static void PreLoad()
	{
		if (s_CacheMap.Count == 0) {
			int n = s_PooledPrefabs.Length;
			for (int i = 0; i < n; i++) {
				s_CacheMap [s_PooledPrefabs [i]] = Resources.Load (s_PooledPrefabs [i]);
			}
			// Cache Player/NPC model&prefab
			Pathea.AvatarCmpt.CachePrefab();
			Pathea.NpcProtoDb.CachePrefab();
			CustomCharactor.CustomMetaData.CachePrefab();
			RandomItem.RandomItemBoxInfo.CachePrefab();
			//s_preloadAnimClips = Resources.LoadAll<AnimationClip>("Model/PlayerModel");
			//s_preloadMaterials = Resources.LoadAll("Model/PlayerModel/Materials");
		}
	}
    public static void RegisterAsset(string pathName, Object obj)
    {
		if (!s_CacheMap.ContainsKey(pathName)){
#if Win32Ver
			s_CacheMap.Clear();
#endif
            s_CacheMap.Add(pathName, obj);
        }
    }
	public static bool TryGetAsset(string pathName, out Object asset)
	{
		return (s_CacheMap.TryGetValue (pathName, out asset));
	}
    public static void Clear()
    {
        //foreach (KeyValuePair<string, Object> kvp in s_CacheMap)
        //{
        //    GameObject.Destroy(kvp.Value);
        //}
        s_CacheMap.Clear();
		//s_preloadAnimClips = null;
		//s_preloadMaterials = null;
    }	
	public static IEnumerator Cleanup()
	{
		while (true)
		{
			// TODO : clean up
			yield return new WaitForSeconds(60.0f * 3);
		}
	}
}

// This manager support only one scene now
public class AssetsLoader : MonoBehaviour
{
	const int MinValidPathLen = 2;	// use this to determine if path is valid.
	public const string InvalidAssetPath = "0";	// Empty would cause load all.
	public const string PrefabExtension = ".prefab";
	public const string AssetBundleExtension = ".unity3d";

    static AssetsLoader self;
    public static AssetsLoader Instance
    {
        get
		{ 
			if (self == null)
			{
				GameObject go = new GameObject("AssetsLoader");
				self = go.AddComponent<AssetsLoader>();
				Debug.Log("AssetsLoader Awake end");
			}
			return self; 
		}
    }

	Stack<AssetReq> assetStack_Request = new Stack<AssetReq>();	
	// Use this for initialization
	void Start()
	{
#if Win32Ver
		//win32 not preload to reduce mem consume
#else
		AssetsPool.PreLoad();
#endif
		
		//Shader.WarmupAllShaders(); // This statement take 25s in windows, up to 170s in mac/linux, so comment it.
		StartCoroutine(ProcessReqs(assetStack_Request, GameConfig.AssetBundlePath));
		//StartCoroutine(AssetsPool.Cleanup());
	}
	void OnDisable()
	{
		StopAllCoroutines();
		assetStack_Request.Clear();
		AssetsPool.Clear();
	}

	//System.Diagnostics.Stopwatch _sw = new System.Diagnostics.Stopwatch();
    IEnumerator ProcessReqs(Stack<AssetReq> assetReqList, string assetBundlePath)
    {
        while(true)
        {
            if (assetReqList.Count > 0)
            {
                AssetReq req = assetReqList.Pop();
				if(req.ProcLevel <= AssetReq.ProcLvl.DoNothing)		continue;

				/* test code for LoadAssetImm
				GameObject goo = LoadAssetImm(req.pathName, req.pos.Position(), req.pos.Rotation(), req.pos.Scale());
				if(goo != null){	req.OnFinish(goo);	}
				yield return 0;
				continue;
				*/
				//_sw.Reset();
				//_sw.Start();
                Object asset = null;                
				string assetPathName = req.PathName;
				string assetPathNameExt = Path.GetExtension(req.PathName);
				bool bAssetBundle = assetPathNameExt.Equals(AssetBundleExtension);
				if(!bAssetBundle && !String.IsNullOrEmpty(assetPathNameExt)){
					assetPathName = (Path.GetDirectoryName(req.PathName) + "/" + Path.GetFileNameWithoutExtension(req.PathName));
				}
				if (!AssetsPool.TryGetAsset(assetPathName, out asset))
				{
					if (bAssetBundle)
					{
						WWW www = WWW.LoadFromCacheOrDownload("file://" + assetBundlePath + assetPathName, AssetsPool.s_Version);
						yield return www;
	                    if (www.error != null)
						{
	                        Debug.LogError(www.error);
						}
	                    else
	                    {
	                        AssetBundle assetBundle = www.assetBundle;
							AssetBundleRequest request = assetBundle.LoadAssetAsync(Path.GetFileNameWithoutExtension(assetPathName), typeof(GameObject));
							yield return request;

	                        if (request != null){
								asset = request.asset;
							}
	                        request = null;
	                        assetBundle.Unload(false);
	                    }
	                    if (www != null)
	                    {
	                        www.Dispose();
	                        www = null;
	                    }
					}
					else // Treat as prefab
					{
#if UNITY_EDITOR
						if (assetPathName.StartsWith("../"))
						{
							asset = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/" + req.PathName.Substring(3), typeof(GameObject));
						}else
#endif
						{
#if UNITY_EDITOR
							//Debug.LogError("Load uncached prefab :"+assetPathName);
#endif
							ResourceRequest resReq = Resources.LoadAsync(assetPathName);
							while (!resReq.isDone) {
								yield return null;
							}
							asset = resReq.asset;
						}
					}

					if (asset != null && req.NeedCaching)
					{
						AssetsPool.RegisterAsset(assetPathName, asset);
					}
				}
				//_sw.Stop();
				//Debug.LogError("Load "+assetPathName+":"+_sw.ElapsedMilliseconds);
				if(asset != null && req.ProcLevel >= AssetReq.ProcLvl.DoInstantiation)
				{
	                GameObject go = Instantiate(asset, req.Prs.Position(), req.Prs.Rotation()) as GameObject;
	                if (go != null)
	                {
						Profiler.BeginSample("AssetsLoader:Instantiate "+assetPathName);
	                    go.transform.localScale = req.Prs.Scale();
	                    req.OnFinish(go);
	                    Profiler.EndSample();
	                }
				}
            }
            yield return null;
        }
    }

	public void AddReq(AssetReq request)
	{
		if (request.PathName.Length < MinValidPathLen) 
			return;

        assetStack_Request.Push(request);
	}
    public AssetReq AddReq(string pathName, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        AssetReq req = new AssetReq(pathName, new AssetPRS(position, rotation, scale));
		AddReq (req);
        return req;
    }
	public Object LoadPrefabImm(string prefabPathName, bool bIntoCache = false)
	{
		Object asset = null;    
		if (prefabPathName.Length < MinValidPathLen) 
			return asset;

		string assetPathName = prefabPathName;
		string assetPathNameExt = Path.GetExtension(prefabPathName);
		if(!String.IsNullOrEmpty(assetPathNameExt)){
			assetPathName = (Path.GetDirectoryName(prefabPathName) + "/" + Path.GetFileNameWithoutExtension(prefabPathName));
		}
		if (!AssetsPool.TryGetAsset (assetPathName, out asset)) {
			try{
#if UNITY_EDITOR
				//Debug.LogError("Load uncached prefab Imm :"+assetPathName);
#endif
				asset = Resources.Load(assetPathName);
			}catch(Exception e){
				Debug.LogError("[AssetsLoader]Failed to load "+assetPathName+" with error: "+e.ToString());
			}
			if(bIntoCache && asset != null){
				AssetsPool.RegisterAsset(assetPathName, asset);
			}
		}
		return asset;
	}
	public GameObject InstantiateAssetImm(string pathName, Vector3 position, Quaternion rotation, Vector3 scale, bool bIntoCache = false)
	{
		GameObject retGo = null;
		if (pathName.Length < MinValidPathLen) 
			return retGo;

		try{
			Object asset = null;   
			string assetPathName = pathName;
			string assetPathNameExt = Path.GetExtension(pathName);
			bool bAssetBundle = assetPathNameExt.Equals(AssetBundleExtension);
			if(!bAssetBundle && !String.IsNullOrEmpty(assetPathNameExt)){
				assetPathName = (Path.GetDirectoryName(pathName) + "/" + Path.GetFileNameWithoutExtension(pathName));
			}
			if (!AssetsPool.TryGetAsset(assetPathName, out asset))
			{
				if (bAssetBundle)
				{
					FileStream fs = new FileStream( GameConfig.AssetBundlePath + assetPathName, FileMode.Open, FileAccess.Read, FileShare.Read );
					BinaryReader r = new BinaryReader(fs);
					byte[] raw_data = r.ReadBytes((int)(fs.Length));
					r.Close();
					fs.Close();

#if UNITY_5_3 || UNITY_5_4
					AssetBundle bundle = AssetBundle.LoadFromMemory(raw_data);
#else
					AssetBundle bundle = AssetBundle.CreateFromMemoryImmediate (raw_data);
#endif				
					asset = bundle.LoadAsset<GameObject>(Path.GetFileNameWithoutExtension(assetPathName));
					bundle.Unload(false);
				}			
				else
				{
#if UNITY_EDITOR
					if (assetPathName.StartsWith("../"))
					{
						asset = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/" + assetPathName.Substring(3), typeof(GameObject));
					}else
#endif
					{
						asset = Resources.Load(assetPathName);
					}
				}
				if (asset != null && bIntoCache)
				{
					AssetsPool.RegisterAsset(assetPathName, asset);
				}
			}
			if(asset != null){
				retGo = Instantiate(asset, position, rotation) as GameObject;
				retGo.transform.localScale = scale;
			}
		} catch(Exception){
			Debug.Log("Error: Faile to LoadAssetImm "+pathName);
		}
		return retGo;
	}
}
