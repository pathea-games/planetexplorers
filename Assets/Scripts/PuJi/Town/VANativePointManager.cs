using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using TownData;

public class VANativePointManager : MonoBehaviour
{
    static VANativePointManager mInstance;

    public static VANativePointManager Instance
    {
        get
        {
            return mInstance;
        }
    }

    public Dictionary<IntVector2, NativePointInfo> nativePointInfoMap;
    public Dictionary<IntVector2, int> createdPosList;
    void Awake()
    {
        mInstance = this;
        nativePointInfoMap = new Dictionary<IntVector2, NativePointInfo>();
        createdPosList = new Dictionary<IntVector2, int>();
    }
    public void RenderNative(IntVector2 nativeXZ)
    {
        //if (!npcInfoMap.ContainsKey(posXZ))
        //{
        //    return;
        //}
        //if(createdPosList.ContainsKey(nativeXZ)){
        //    return;
        //}
        NativePointInfo nativePointInfo = nativePointInfoMap[nativeXZ];
        if (nativePointInfo.PosY == -1)
        {
            return;
        }

        RenderNative(nativePointInfo);
    }

    public void RenderNative(NativePointInfo nativePointInfo)
    {
        int id = nativePointInfo.ID;
        Vector3 pos = nativePointInfo.position;
//        int townId = nativePointInfo.townId;
        //if (Pathea.PeGameMgr.IsSingleAdventure)
        //{
        //    Debug.Log("SPPoint.InstantiateSPPoint<SPPoint>: " + pos);
        //    SPPoint point = SPPoint.InstantiateSPPoint<SPPoint>(pos,
        //                                                        Quaternion.identity,
        //                                                        IntVector4.Zero,
        //                                                        SPTerrainEvent.instance.transform,
        //                                                        0,
        //                                                        id,
        //                                                        true,
        //                                                        true,
        //                                                        false,
        //                                                        false);

        //    SPTerrainEvent.instance.RegisterSPPoint(point, true);
        //}
        //else if (GameConfig.IsMultiMode)
        //{
        //    SPTerrainEvent.instance.CreateMultiNativeStatic(pos, id, townId);
        //}

//		if(Pathea.PeGameMgr.IsSingleAdventure)
//		{
		int allyId = VArtifactTownManager.Instance.GetTownByID(nativePointInfo.townId).AllyId;
		int playerId = VATownGenerator.Instance.GetPlayerId(allyId);
		int allyColor = VATownGenerator.Instance.GetAllyColor(allyId);
		SceneEntityPosAgent agent = MonsterEntityCreator.CreateAdAgent(pos, id,allyColor,playerId);
		SceneMan.AddSceneObj(agent);
		VArtifactTownManager.Instance.AddMonsterPointAgent(nativePointInfo.townId,agent);
//		}
	}
	
	
    internal NativePointInfo GetNativePointByPosXZ(IntVector2 nativePointXZ)
    {
        if (!nativePointInfoMap.ContainsKey(nativePointXZ))
        {
            return null;
        }
        return nativePointInfoMap[nativePointXZ];
    }

    internal void AddNative(NativePointInfo nativePointInfo)
    {
        nativePointInfoMap[nativePointInfo.index] = nativePointInfo;
    }

    //internal bool IsCreated(IntVector2 nativePosXZ)
    //{
    //    return createdPosList.Contains(nativePosXZ);
    //}
}