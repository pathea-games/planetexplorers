using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SkillAsset;
using Pathea.Effect;
using Pathea;

public class BGEffect : MonoBehaviour 
{
    static BGEffect mInstance;
    public static BGEffect Instance { get { return mInstance; } }

    public float audioRate;
    public float effectRate;

    GameObject mAudioPoint;
    GameObject mEffectPoint;

	List<AudioController> envSounds = new List<AudioController>();
    List<GameObject> envEffects = new List<GameObject>();
	List<IntVector2> exists = new List<IntVector2>();

    internal virtual int GetMapID(Vector3 position) { return -1; }

	IEnumerator Start()
	{
		mInstance = this;
        while (LODOctreeMan.self == null) { yield return null; }
		LODOctreeMan.self.AttachEvents(null, OnTerrainColliderDestroy, OnTerrainColliderCreated, null);

        if (mEffectPoint == null)
        {
            mEffectPoint = new GameObject("Effect");
            mEffectPoint.transform.parent = transform;
            mEffectPoint.transform.localPosition = Vector3.zero;
        }

        if (mAudioPoint == null)
        {
            mAudioPoint = new GameObject("Audio");
            mAudioPoint.transform.parent = transform;
            mAudioPoint.transform.localPosition = Vector3.zero;
        }
    }

    void Update()
    {
        PeEntity player = PeCreature.Instance.mainPlayer;
        if (player != null)
        {
            for (int i = envSounds.Count - 1; i >=0 ; i--)
            {
                AudioController audioCtrl = envSounds[i];
                if (audioCtrl == null)
                    continue;

                float sqrSDis = PETools.PEUtil.SqrMagnitude(player.position, audioCtrl.transform.position);
                float dis = audioCtrl.mAudio.maxDistance * 0.97f;
                if (sqrSDis < dis * dis)
                {
                    if (!audioCtrl.mAudio.loop)
                    {
                        audioCtrl.autoDel = true;
                        envSounds.RemoveAt(i);
                    }

                    audioCtrl.PlayAudio(1f);
                }
            }
        }
    }

	void OnDestroy()
	{
        if(LODOctreeMan.self != null)
		    LODOctreeMan.self.DetachEvents(null, OnTerrainColliderDestroy, OnTerrainColliderCreated, null);
	}

	void OnTerrainColliderCreated(IntVector4 node)
	{
		if(node.w == 0)
		{
			IntVector2 mark = new IntVector2(node.x, node.z);
			if(!exists.Contains(mark))
			{
				SetupEnvironmentAudio(node);

                SetupEnvironmentEffect(node);
				
				exists.Add(mark);
			}
		}
	}
	
	void OnTerrainColliderDestroy(IntVector4 node)
	{
		if(node.w == 0)
		{
			IntVector2 mark = new IntVector2(node.x, node.z);
			if(exists.Contains(mark))
			{
				List<AudioController> acList = envSounds.FindAll(ret => Match(ret, node));
				foreach (AudioController acCtrl in acList) 
				{
					envSounds.Remove(acCtrl);
					acCtrl.Delete();
				}

                List<GameObject> envEffList = envEffects.FindAll(ret => MatchEffect(ret, node));

                foreach (GameObject ite in envEffList)
                {
                    envEffects.Remove(ite);
                    GameObject.Destroy(ite);
                }
				
				exists.Remove(mark);
			}
		}
	}

    void SetupEnvironmentEffect(IntVector4 node)
    {
        if (Random.value < effectRate)
        {
            SetupEffect(node);
        }
    }

    void OnEffectSpawned(GameObject effect)
    {
        if (effect != null && !envEffects.Contains(effect))
            envEffects.Add(effect);
    }

    int CalculateType(Vector3 pos)
    {
        return AiUtil.CheckPositionInCave(pos, 128.0f, AiUtil.groundedLayer) ? 2 : (GameConfig.IsNight ? 0 : 1);
    }

    void SetupEffect(IntVector4 node)
    {
        Vector3 position = node.ToVector3();
        position += new Vector3(Random.Range(0.0f, VoxelTerrainConstants._numVoxelsPerAxis << node.w),
                                0.0f,
                                Random.Range(0.0f, VoxelTerrainConstants._numVoxelsPerAxis << node.w));

        int height = VoxelTerrainConstants._numVoxelsPerAxis << node.w;

        RaycastHit hitInfo;
        if (Physics.Raycast(position + Vector3.up * height, Vector3.down, out hitInfo, height, GameConfig.GroundLayer))
        {
            float waterHeight;
            int type1 = -1;
            int type2 = -1;
            if (PETools.PEUtil.GetWaterSurfaceHeight(hitInfo.point, out waterHeight))
            {
                if (Random.value < 0.3f)
                {
                    type1 = 0;
                    position = new Vector3(hitInfo.point.x, waterHeight, hitInfo.point.z);
                }
                else
                {
                    type1 = 1;
                    position = hitInfo.point + Vector3.up * Random.Range(0.0f, waterHeight - hitInfo.point.y);
                }
            }
            else
            {
                type1 = 2;
                position = hitInfo.point + Vector3.up * Random.Range(0.5f, 5.0f);
            }

            type2 = CalculateType(position);

            int effectID = AISpawnDataStory.GetEnvEffectID(GetMapID(position), type1, type2);
            Transform parent = mEffectPoint != null ? mEffectPoint.transform : null;
            EffectBuilder.EffectRequest req = EffectBuilder.Instance.Register(effectID, null, position, Quaternion.identity, parent);
            req.SpawnEvent += OnEffectSpawned;
        }
    }

	void SetupEnvironmentAudio(IntVector4 node)
	{
        if (Random.value < audioRate)
        {
            SetupAudioController(node);
        }
	}

	void SetupAudioController(IntVector4 node)
	{
		Vector3 position = node.ToVector3();
		position += new Vector3(Random.Range(0.0f, VoxelTerrainConstants._numVoxelsPerAxis << node.w),
		                        0.0f,
		                        Random.Range(0.0f, VoxelTerrainConstants._numVoxelsPerAxis << node.w));
		
		int height =  VoxelTerrainConstants._numVoxelsPerAxis << node.w;
		RaycastHit hitInfo;
        if (Physics.Raycast(position + Vector3.up * height, Vector3.down, out hitInfo, height, GameConfig.GroundLayer))
		{
            float waterHeight;
            if (PETools.PEUtil.GetWaterSurfaceHeight(hitInfo.point, out waterHeight))
                position = hitInfo.point + Vector3.up * Random.Range(0.0f, waterHeight - hitInfo.point.y);
            else
                position = hitInfo.point + Vector3.up * Random.Range(0.5f, 5.0f);

            int sid = AISpawnDataStory.GetEnvMusicID(GetMapID(position));
            Transform parent = mAudioPoint != null ? mAudioPoint.transform : null;
            AudioController ctrl = AudioManager.instance.Create(position, sid, parent, false, false);
            envSounds.Add(ctrl);
		}
	}
	
	bool Match(AudioController ac, IntVector4 node)
	{
		if(ac == null)
			return false;
		
		float dx = ac.transform.position.x - node.x;
		float dz = ac.transform.position.z - node.z;
		
		return dx >= PETools.PEMath.Epsilon && dx <= VoxelTerrainConstants._numVoxelsPerAxis << node.w 
			    && dz >= PETools.PEMath.Epsilon && dz <= VoxelTerrainConstants._numVoxelsPerAxis << node.w 
				&& ac.transform.position.y >= node.y;
	}

    bool MatchEffect(GameObject ac, IntVector4 node)
    {
        if (ac == null)
            return false;

        float dx = ac.transform.position.x - node.x;
        float dz = ac.transform.position.z - node.z;

        return dx >= PETools.PEMath.Epsilon && dx <= VoxelTerrainConstants._numVoxelsPerAxis << node.w
                && dz >= PETools.PEMath.Epsilon && dz <= VoxelTerrainConstants._numVoxelsPerAxis << node.w
                && ac.transform.position.y >= node.y;
    }
}
