using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using CustomData;
using SkillAsset;
using ItemAsset;
public partial class PlayerNetwork
{


	void RPC_S2C_Plant_GetBack(uLink.BitStream stream,uLink.NetworkMessageInfo info)
	{
		int objId = stream.Read<int> ();
        FarmManager.Instance.RemovePlant(objId);
        DragArticleAgent.Destory(objId);
        //ItemMgr.Instance.DestroyItem(objId);

	}
    
    void RPC_S2C_Plant_PutOut(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        Vector3 pos = stream.Read<Vector3>();
        Quaternion rot = stream.Read<Quaternion>();
        Vector3 scale = stream.Read<Vector3>();
        int instanceId = stream.Read<int>();

        ItemObject itemobj = ItemMgr.Instance.Get(instanceId);

        DragArticleAgent dragItem = DragArticleAgent.Create(itemobj.GetCmpt<Drag>(), pos, scale, rot, instanceId);
        
        FarmPlantLogic plant = dragItem.itemLogic as FarmPlantLogic;
        plant.InitInMultiMode();
        stream.Read<FarmPlantLogic>();
        plant.UpdateInMultiMode();
        
        //DragItem item = new DragItem(objID);

        //item.position = plantPos;
        //item.rotation = transform.rotation;
        //item.itemScript = null;
        ////item.DependType = dependType;
        //item.network = this;
        //DragItem.Mgr.Instance.Add(item);


    }

	void RPC_S2C_Plant_VFTerrainTarget(uLink.BitStream stream,uLink.NetworkMessageInfo info)
	{
		byte[] data = stream.Read<byte[]> ();
		/*int viewId = */stream.Read<int> ();
        //if ( PlayerFactory.mMainPlayer.OwnerView.viewID.id == viewId )
        //    return;
		MemoryStream ms = new MemoryStream(data);
		BinaryReader _out = new BinaryReader(ms);
		
		Dictionary<IntVector3, VFVoxel> voxels = new Dictionary<IntVector3, VFVoxel> ();
		int Count = _out.ReadInt32 ();
		for (int i = 0; i < Count; i++) 
		{
			IntVector3 index = new IntVector3(_out.ReadInt32 (), _out.ReadInt32 (), _out.ReadInt32 ());
			voxels [index]  = new VFVoxel(_out.ReadByte(), _out.ReadByte());
			VFVoxelTerrain.self.AlterVoxelInBuild(index.ToVector3(), voxels [index] );
        }
	}

    //[Obsolete]
	void RPC_S2C_Plant_FarmInfo(uLink.BitStream stream,uLink.NetworkMessageInfo info)
	{
		byte[] data = stream.Read<byte[]> ();
		List<FarmPlantInitData> initList = FarmManager.Instance.ImportPlantData (data);

        foreach (FarmPlantInitData plantData in initList)
        {
            ItemObject itemObj = ItemMgr.Instance.Get(plantData.mPlantInstanceId);
            DragArticleAgent dragItem = DragArticleAgent.Create(itemObj.GetCmpt<Drag>(), plantData.mPos, Vector3.one, plantData.mRot, plantData.mPlantInstanceId);
            
            FarmPlantLogic plant = dragItem.itemLogic as FarmPlantLogic;
            plant.InitDataFromPlant(plantData);
            FarmManager.Instance.AddPlant(plant);
            plant.UpdateInMultiMode();
        }
	}

    //[Obsolete]
	void RPC_S2C_Plant_Water(uLink.BitStream stream,uLink.NetworkMessageInfo info)
	{
		int objId = stream.Read<int> ();
		double water = stream.Read<double> ();
		FarmPlantLogic plant = FarmManager.Instance.GetPlantByItemObjID (objId);
        if (plant != null)
        {
            plant.mWater = water;
            plant.UpdateInMultiMode();
        }
	}

   // [Obsolete]
	void RPC_S2C_Plant_Clean(uLink.BitStream stream,uLink.NetworkMessageInfo info)
	{
		int objId = stream.Read<int> ();
		double clean = stream.Read<double> ();
		FarmPlantLogic plant = FarmManager.Instance.GetPlantByItemObjID (objId);
        if (plant != null)
        {
            plant.mClean = clean;
            plant.UpdateInMultiMode();
        }
	}

	void RPC_S2C_Plant_Clear(uLink.BitStream stream,uLink.NetworkMessageInfo info)
	{
		int objId = stream.Read<int> ();
		FarmPlantLogic plant = FarmManager.Instance.GetPlantByItemObjID (objId);
		if (plant != null)
		{
			FarmManager.Instance.RemovePlant(objId);
            DragArticleAgent.Destory(objId);
			//ItemMgr.Instance.DestroyItem(objId);
		}

    }

	
}