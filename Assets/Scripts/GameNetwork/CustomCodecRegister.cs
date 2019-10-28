using UnityEngine;
using System.Collections;
using ItemAsset;
using Jboy;
using CustomData;
using TownData;

[AddComponentMenu("Pathea Network/CustomCodecRegister")]

public class CustomCodecRegister : MonoBehaviour
{
	void Awake()
	{
        uLink.BitStreamCodec.AddAndMakeArray<IntVector3>(IntVector3.DeserializeItem, IntVector3.SerializeItem);
        uLink.BitStreamCodec.AddAndMakeArray<IntVector2>(IntVector2.DeserializeItem, IntVector2.SerializeItem);
		uLink.BitStreamCodec.AddAndMakeArray<TMsgInfo>(TMsgInfo.ReadMsg, TMsgInfo.WriteMsg);
		uLink.BitStreamCodec.AddAndMakeArray<RoleInfo>(RoleInfo.ReadRoleInfo, RoleInfo.WriteRoleInfo);
        uLink.BitStreamCodec.AddAndMakeArray<RoleInfoProxy>(RoleInfoProxy.ReadRoleInfoProxy, RoleInfoProxy.WriteRoleInfoProxy);
        uLink.BitStreamCodec.AddAndMakeArray<ItemObjectData>(ItemObjectData.ReadItem, ItemObjectData.WriteItem);
        //uLink.BitStreamCodec.AddAndMakeArray<AISpawnPointNetwork>(AISpawnPointNetwork.ReadSpawnPoint, AISpawnPointNetwork.WriteSpawnPoint);
        //uLink.BitStreamCodec.AddAndMakeArray<AISpawnChunkNetwork>(AISpawnChunkNetwork.ReadSpawnChunk, AISpawnChunkNetwork.WriteSpawnChunk);
        uLink.BitStreamCodec.AddAndMakeArray<SPTerrainRect>(SPTerrainRect.ReadSPTerrainRect, SPTerrainRect.WriteSPTerrainRect);
        uLink.BitStreamCodec.AddAndMakeArray<SPPoint>(SPPoint.ReadSPPoint, SPPoint.WriteSPPoint);
		//uLink.BitStreamCodec.AddAndMakeArray<MapArea>(MapArea.DeserializeMapArea, MapArea.SerializeMapArea);
		uLink.BitStreamCodec.AddAndMakeArray<IntVector4>(IntVector4.DeserializeItem, IntVector4.SerializeItem);
		//uLink.BitStreamCodec.AddAndMakeArray<BattleArea>(BattleArea.DeserializeBattleArea, BattleArea.SerializeBattleArea);
		uLink.BitStreamCodec.AddAndMakeArray<ItemObject>(ItemObject.Deserialize, ItemObject.Serialize);
		uLink.BitStreamCodec.AddAndMakeArray<BattleInfo>(BattleInfo.Deserialize, BattleInfo.Serialize);
		//uLink.BitStreamCodec.AddAndMakeArray<NpcDataInfo>(NpcDataInfo.Deserialize, NpcDataInfo.Serialize);
		uLink.BitStreamCodec.AddAndMakeArray<NpcMissionData>(NpcMissionData.Deserialize, NpcMissionData.Serialize);
		uLink.BitStreamCodec.AddAndMakeArray<CreationOriginData>(CreationOriginData.Deserialize, CreationOriginData.Serialize);
		uLink.BitStreamCodec.AddAndMakeArray<PlayerBattleInfo>(PlayerBattleInfo.Deserialize, PlayerBattleInfo.Serialize);
		uLink.BitStreamCodec.AddAndMakeArray<ItemSample>(ItemSample.Deserialize, ItemSample.Serialize);
		//uLink.BitStreamCodec.AddAndMakeArray<MaskArea>(MaskArea.Deserialize, MaskArea.Serialize);
		uLink.BitStreamCodec.AddAndMakeArray<RegisteredISO>(RegisteredISO.Deserialize, RegisteredISO.Serialize);
		uLink.BitStreamCodec.AddAndMakeArray<MapObj>(MapObj.Deserialize, MapObj.Serialize);
		uLink.BitStreamCodec.AddAndMakeArray<HistoryStruct>(HistoryStruct.Deserialize, HistoryStruct.Serialize);
		uLink.BitStreamCodec.AddAndMakeArray<CompoudItem>(CompoudItem.Deserialize, CompoudItem.Serialize);
		uLink.BitStreamCodec.AddAndMakeArray<LobbyShopData>(LobbyShopData.Deserialize, LobbyShopData.Serialize);
        //randomTown
        uLink.BitStreamCodec.AddAndMakeArray<CreatItemInfo>(CreatItemInfo.DeserializeItemInfo, CreatItemInfo.SerializeItemInfo);
        //uLink.BitStreamCodec.AddAndMakeArray<TownInfo>(TownInfo.DeserializeInfo, TownInfo.SerializeInfo);
        //uLink.BitStreamCodec.AddAndMakeArray<BuildingInfo>(BuildingInfo.DeserializeInfo, BuildingInfo.SerializeInfo);
        uLink.BitStreamCodec.AddAndMakeArray<VATownNpcInfo>(VATownNpcInfo.DeserializeInfo, VATownNpcInfo.SerializeInfo);
        uLink.BitStreamCodec.AddAndMakeArray<BuildingID>(BuildingID.Deserialize, BuildingID.Serialize);
		uLink.BitStreamCodec.AddAndMakeArray<SceneObject>(SceneObject.Deserialize, SceneObject.Serialize);
        //FarmPlant
        uLink.BitStreamCodec.AddAndMakeArray<FarmPlantLogic>(FarmPlantLogic.Deserialize, FarmPlantLogic.Serialize);
        //colony
        uLink.BitStreamCodec.AddAndMakeArray<TradeObj>(TradeObj.Deserialize, TradeObj.Serialize);
        uLink.BitStreamCodec.AddAndMakeArray<TownTradeItemInfo>(TownTradeItemInfo.Deserialize, TownTradeItemInfo.Serialize);
        uLink.BitStreamCodec.AddAndMakeArray<ItemIdCount>(ItemIdCount.Deserialize, ItemIdCount.Serialize);
        uLink.BitStreamCodec.AddAndMakeArray<CSTreatment>(CSTreatment.Deserialize, CSTreatment.Serialize);
    }
}
