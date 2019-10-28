using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;


public partial class PlayerNetwork 
{



	#region Action Callback APIs
    void RPC_S2C_GenRandomItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        Vector3 pos = stream.Read<Vector3>();
        Quaternion rot = stream.Read<Quaternion>();
        int templateId = stream.Read<int>();
        int[] itemIdCount = stream.Read<int[]>();

		RandomItem.RandomItemBoxInfo boxInfo = RandomItem.RandomItemBoxInfo.GetBoxInfoById(templateId);
		if(boxInfo!=null)
			RandomItemMgr.Instance.AddItmeResult(pos,rot, templateId, itemIdCount, boxInfo.boxModelPath);
    }
	void RPC_S2C_GenRandomItemRare(uLink.BitStream stream, uLink.NetworkMessageInfo info){
		stream.Read<Vector3>();

		//RandomItemObj rio = RandomItemMgr.Instance.GetRandomItemObj(pos);
		//--to do:
		//rio.AddRareProto(DunItemId.UNFINISHED_ISO,1);

	}
	void RPC_S2C_RandomItemRareAry(uLink.BitStream stream, uLink.NetworkMessageInfo info){
		List<Vector3> posList = stream.Read<Vector3[]>().ToList();
		List<Quaternion> rotList = stream.Read<Quaternion[]>().ToList();
		List<int> idList = stream.Read<int[]>().ToList();
		List<int> itemLength = stream.Read<int[]>().ToList();
		List<int> allItems = stream.Read<int[]>().ToList();

		for(int i=0;i<posList.Count;i++){
			Vector3 pos = posList[i];
			Quaternion rot = rotList[i];
			int boxId = idList[i];
			int iCount = itemLength[i];
			int[] itemIdCount = new int[iCount];
			Array.Copy(allItems.ToArray(),itemIdCount,iCount);
			allItems.RemoveRange(0,iCount);
			RandomItem.RandomItemBoxInfo boxInfo = RandomItem.RandomItemBoxInfo.GetBoxInfoById(boxId);
			if(boxInfo!=null)
				RandomItemMgr.Instance.AddRareItmeResult(pos,rot, boxId, itemIdCount, boxInfo.boxModelPath);
		}
	}


	void RPC_S2C_GetRandomIsoCode(uLink.BitStream stream,uLink.NetworkMessageInfo info){
		//--to do: get iso code
		//Vector3 pos = stream.Read<Vector3>();
	}

    void RPC_S2C_RandomItemFetch(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        Vector3 pos = stream.Read<Vector3>();
        int index = stream.Read<int>();
        int protoid = stream.Read<int>();
        int count = stream.Read<int>();

        RandomItemObj riObj = RandomItemMgr.Instance.GetRandomItemObj(pos);
        if (riObj == null)
            return;
        riObj.TryFetch(index,protoid,count);
        if(GameUI.Instance.mItemGet!=null)
			GameUI.Instance.mItemGet.Reflash();
    }

    void RPC_S2C_RandomItemFetchAll(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        Vector3 pos = stream.Read<Vector3>();
        RandomItemObj riObj = RandomItemMgr.Instance.GetRandomItemObj(pos);
        if (riObj == null)
            return;
        riObj.TryFetchAll();
        if (GameUI.Instance.mItemGet != null)
			GameUI.Instance.mItemGet.Reflash();
    }
    void RPC_S2C_GenRandomFeces(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        Vector3 pos = stream.Read<Vector3>();
        Quaternion rot = stream.Read<Quaternion>();
        int[] itemIdCount = stream.Read<int[]>();


        RandomItemMgr.Instance.AddFecesResult(pos, rot, itemIdCount);
    }


    //new fetch
    void RPC_S2C_RandomItemClicked(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        Vector3 pos = stream.Read<Vector3>();
        int[] itemIdCount = stream.Read<int[]>(); 
        RandomItemObj riObj = RandomItemMgr.Instance.GetRandomItemObj(pos);
        if (riObj == null)
            return;
        riObj.ClickedInMultiMode(itemIdCount);

    }
    void RPC_S2C_RandomItemDestroy(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        Vector3 pos = stream.Read<Vector3>(); 
        RandomItemObj riObj = RandomItemMgr.Instance.GetRandomItemObj(pos);
        if (riObj == null)
            return;
        riObj.DestroySelf();
    }

	void RPC_S2C_RandomItemDestroyList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3[] posList = stream.Read<Vector3[]>(); 
		foreach(Vector3 pos in posList){
			RandomItemObj riObj = RandomItemMgr.Instance.GetRandomItemObj(pos);
			if (riObj == null)
				return;
			riObj.DestroySelf();
		}
	}

	#endregion
}
