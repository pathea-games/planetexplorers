using UnityEngine;
using ScenarioRTL;
using Pathea;
using ItemAsset;
using System.Collections.Generic;

namespace PeCustom
{
    [Statement("EQUIPMENT")]
    public class EquipmentCondition : ScenarioRTL.Condition
    {
        // 在此列举参数
        OBJECT player;  // PLAYER
        OBJECT equip;   // ITEM
        
        // 在此初始化参数
        protected override void OnCreate()
        {
            player = Utility.ToObject(parameters["player"]);
            equip = Utility.ToObject(parameters["equipment"]);
        }
        
        // 判断条件
        public override bool? Check()
        {
			if (player.type == OBJECT.OBJECTTYPE.Player && equip.type == OBJECT.OBJECTTYPE.ItemProto)
			{
				Pathea.PeEntity entity = PeScenarioUtility.GetEntity(player);
				if (entity != null && entity.equipmentCmpt != null)
				{
					List<ItemObject> list = entity.equipmentCmpt._ItemList;
					for (int i = 0; i < list.Count; ++i)
					{
						if (equip.isAnyPrototype)
							return true;
						if (equip.isAnyPrototypeInCategory && list[i].protoData.editorTypeId == equip.Group)
							return true;
						if (list[i].protoId == equip.Id)
							return true;
					}
				}
			}
			return false;
        }

        protected override void SendReq()
        {
            // 向服务器端发请求
            byte[] data = BufferHelper.Export(w =>
            {
                w.Write(reqId);

                BufferHelper.Serialize(w, player);
                BufferHelper.Serialize(w, equip);
            });

            PlayerNetwork.RequestServer(EPacketType.PT_Custom_CheckEquip, data);
        }
    }
}
