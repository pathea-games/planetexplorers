using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WhiteCat;

public class SPTerrainRect : MonoBehaviour
{
	int mCount;
	IntVector4 mPosition;
	List<SPPoint> mPoints;
	List<IntVector2> mMeshNodes;
	List<IntVector2> mCaveNodes;
	List<Vector3> mEventPosition;

    SimplexNoise mNoise;

	public IntVector4 position
	{
		get{return mPosition;}
	}

	public List<SPPoint> points
	{
		get{return mPoints;}
	}

	public List<IntVector2> meshNodes
	{
		get{return mMeshNodes;}
	}

    public IntVector4 nextIndex
    {
        get { return new IntVector4(mPosition.x, mPosition.z, mCount++, 0); }
    }

    public IntVector4 currentIndex
    {
        get { return new IntVector4(mPosition.x, mPosition.z, mCount + 1, 0); }
    }

    public static SPTerrainRect InstantiateSPTerrainRect(IntVector4 pos, int minCount, int maxCount, Transform parent, SimplexNoise noise)
    {
        GameObject obj = new GameObject(pos.x + " , " + pos.z);
        obj.transform.parent = parent;
        obj.transform.position = pos.ToVector3();

        SPTerrainRect rect = obj.AddComponent<SPTerrainRect>();
        rect.Init(pos, minCount, maxCount, noise);

        return rect;
    }

    public void Init(IntVector4 pos, int minCount, int maxCount, SimplexNoise noise)
    {
        mCount = 0;
        mNoise = noise;
        mPosition = pos;
        mMeshNodes = new List<IntVector2>();
        mCaveNodes = new List<IntVector2>();
        mPoints = new List<SPPoint>();
        Spawn(pos, minCount, maxCount);
    }

    public void Destroy()
    {
        Cleanup();
        GameObject.Destroy(gameObject);
    }

	public void Cleanup()
	{
		foreach (SPPoint point in mPoints) 
		{
            if(point != null)
            {
                point.Activate(false);
                //GameObject.Destroy(point.gameObject);
            }
		}

		mPoints.Clear();
	}

	public bool IsMarkExistCave(IntVector2 mark)
	{
		return mCaveNodes.Contains(mark);
	}

	public void RegisterCavMark(IntVector2 mark)
	{
		if(!mCaveNodes.Contains(mark))
		{
			mCaveNodes.Add(mark);
		}
	}

	public void RegisterSPPoint(SPPoint point)
	{
        if (point == null)
            return;

        if(!mPoints.Contains(point))
		    mPoints.Add(point);
	}

	public void RegisterMeshNode(IntVector4 node)
	{
		IntVector2 mark = AiUtil.ConvertToIntVector2FormLodLevel(
			node, LODOctreeMan._maxLod);
                                                                                                 
		if(!mMeshNodes.Contains(mark))
			mMeshNodes.Add(mark);
	}

	public void RemoveMeshNode(IntVector4 node)
	{
		IntVector2 mark = AiUtil.ConvertToIntVector2FormLodLevel(
			node, LODOctreeMan._maxLod);

		if(mMeshNodes.Contains(mark))
			mMeshNodes.Remove(mark);
	}

	bool Match(SPPoint point, IntVector4 node)
	{
		if(point == null)
			return false;

		Rect rect = new Rect(node.x, node.z, 
		                     VoxelTerrainConstants._numVoxelsPerAxis << node.w, 
		                     VoxelTerrainConstants._numVoxelsPerAxis << node.w);
		Vector2 mark = new Vector2(point.position.x, point.position.z);
		return rect.Contains(mark);
	}

    void LoadNoisePoints(IntVector4 node, int minCount, int maxCount)
    {
        int nx = node.x >> VoxelTerrainConstants._shift >> node.w;
        int nz = node.z >> VoxelTerrainConstants._shift >> node.w;

        float noise = (float)mNoise.Noise(nx, nz, nx + nz);
        int randomCount = Mathf.FloorToInt((maxCount - minCount) * noise);
        int count = Mathf.Clamp(minCount + randomCount, minCount, maxCount);
        int length = VoxelTerrainConstants._numVoxelsPerAxis << node.w;

        for (int i = 0; i < count; i++)
        {
            float ox = (float)mNoise.Noise(nx, (nx + nz) * i) * 0.5f + 0.5f;
            float oz = (float)mNoise.Noise(nz, (nx - nz) * i) * 0.5f + 0.5f;

            Vector3 pos = new Vector3(node.x + ox * length, node.y, node.z + oz * length);
            Quaternion rot = Quaternion.Euler(0.0f, UnityEngine.Random.Range(0, 360), 0.0f);
            SPPoint point = SPPoint.InstantiateSPPoint<SPPoint>(pos, rot, nextIndex, transform, 0, 0, true, true, false, true, true, mNoise);
            point.name = "Noise : " + point.name;
            RegisterSPPoint(point);

            //Debug.LogError("Noise normal ai point : " + pos);
        }
    }

    void LoadNoiseBossPoints(IntVector4 node)
    {
        int nx = node.x >> VoxelTerrainConstants._shift >> node.w;
        int nz = node.z >> VoxelTerrainConstants._shift >> node.w;

        float value = (float)mNoise.Noise(nx, nz);
        if (value < -0.5f)
        {

            int length = VoxelTerrainConstants._numVoxelsPerAxis << node.w;

            float ox = (float)mNoise.Noise(nx, nz, nx + nz) * 0.5f + 0.5f;
            float oz = (float)mNoise.Noise(nz, nx, nx - nz) * 0.5f + 0.5f;

            Vector3 pos = new Vector3(node.x + ox * length, node.y, node.z + oz * length);

            if (AIErodeMap.IsInErodeArea(pos) == null)
            {
                Quaternion rot = Quaternion.Euler(0.0f, Random.Range(0, 360), 0.0f);
                SPPoint point = SPPoint.InstantiateSPPoint<SPPointBoss>(pos, rot, nextIndex, transform, 0, 0, true, true, true, true, true, mNoise);
                point.name = "Noise boss : " + point.name;
                RegisterSPPoint(point);
            }

            //Debug.LogError("Noise boss ai point : " + pos);
        }
    }

    void LoadDynamicPoints(IntVector4 node, int min, int max)
    {
        int count = Random.Range(min, max);
        for (int i = 0; i < count; i++)
        {
            Vector3 position = node.ToVector3();
            position += new Vector3(Random.Range(0.0f, VoxelTerrainConstants._numVoxelsPerAxis << node.w),
                        0.0f,
                        Random.Range(0.0f, VoxelTerrainConstants._numVoxelsPerAxis << node.w));

            Quaternion rot = Quaternion.Euler(0.0f, Random.Range(0, 360), 0.0f);
            SPPoint point = SPPoint.InstantiateSPPoint<SPPoint>(position, rot, nextIndex, transform);
            point.name = "Dynamic : " + point.name;
            RegisterSPPoint(point);
        }
    }

    void LoadUpperAirPoints(IntVector4 node, int min, int max)
    {
        GameObject carrier = null;// PlayerFactory.mMainPlayer.Carrier;
        if (carrier == null)
            return;

        HelicopterController hel = carrier.GetComponent<HelicopterController>();
        if (hel == null)
            return;

		/*
        VCPVtolCockpitFunc vtol = hel.m_Cockpit as VCPVtolCockpitFunc;
        if (vtol == null)
            return;

        if (vtol.FlyingHeight < 50.0f)
            return;
		 * */

        //if (Random.value > AiManager.Manager.upperPoint)
        //    return;

        Vector3 position = node.ToVector3();
        position += new Vector3(Random.Range(0.0f, VoxelTerrainConstants._numVoxelsPerAxis << node.w),
                                0.0f,
                                Random.Range(0.0f, VoxelTerrainConstants._numVoxelsPerAxis << node.w));

        Quaternion rot = Quaternion.Euler(0.0f, Random.Range(0, 360), 0.0f);
        SPPoint point = SPPoint.InstantiateSPPoint<SPPoint>(position, rot, nextIndex, transform, 0, 63);
        point.name = "Upper Air : " + point.name;
        RegisterSPPoint(point);
    }

    void LoadStaticPoints(IntVector4 node)
    {
        List<SPPoint> spawnPoints = AISpawnPoint.GetPoints(node);
        foreach (SPPoint point in spawnPoints)
        {
            point.index = nextIndex;
            RegisterSPPoint(point);
        }
    }

    void Spawn(IntVector4 node, int min, int max)
    {
        //if (Application.loadedLevelName.Equals(GameConfig.MainSceneName))
        //{
        //    LoadDynamicPoints(node, min, max);
        //    LoadStaticPoints(node);
        //    //LoadUpperAirPoints(node, 1, 3);
        //}

        //if (Application.loadedLevelName.Equals(GameConfig.AdventureSceneName))
        //{
        //    LoadDynamicPoints(node, min, max);
        //    LoadNoiseBossPoints(node);
        //}

        //if (Application.loadedLevelName.Equals(GameConfig.ClientSceneName))
        //{
        //    LoadNoisePoints(node, min, max);
        //    LoadNoiseBossPoints(node);
        //}
    }

    public static void WriteSPTerrainRect(uLink.BitStream stream, object obj, params object[] codecOptions)
    {
        SPTerrainRect point = obj as SPTerrainRect;
        stream.Write<int>(point.mCount);
        stream.Write<IntVector4>(point.mPosition);
        stream.Write<SPPoint[]>(point.mPoints.ToArray());
    }

    public static object ReadSPTerrainRect(uLink.BitStream stream, params object[] codecOptions)
    {
        SPTerrainRect point = new SPTerrainRect();
        point.mCount = stream.Read<int>();
        point.mPosition = stream.Read<IntVector4>();
        point.mPoints = new List<SPPoint>();
        SPPoint[] points = stream.Read<SPPoint[]>();
        point.mPoints.AddRange(points);
        return point;
    }
}
