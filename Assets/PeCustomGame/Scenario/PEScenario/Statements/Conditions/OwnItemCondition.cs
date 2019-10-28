using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("OWN ITEM")]
    public class OwnItemCondition : ScenarioRTL.Condition
    {
        // 在此列举参数
        OBJECT player;  // PLAYER
        ECompare comp;
        int count;
        OBJECT item;   // ITEM

        // 在此初始化参数
        protected override void OnCreate()
        {
            player = Utility.ToObject(parameters["player"]);
            comp = Utility.ToCompare(parameters["compare"]);
            count = Utility.ToInt(missionVars, parameters["count"]);
            item = Utility.ToObject(parameters["item"]);
        }

        // 判断条件
        public override bool? Check()
        {
            if (player.type == OBJECT.OBJECTTYPE.Player && item.type == OBJECT.OBJECTTYPE.ItemProto)
            {
                Pathea.PeEntity entity = PeScenarioUtility.GetEntity(player);
                if (entity != null && entity.packageCmpt != null)
                {
                    int num = 0;
                    if (item.isSpecificPrototype)
                        num = entity.packageCmpt.GetItemCount(item.Id);             //某种item
                    else if (item.isAnyPrototypeInCategory)
                        num = entity.packageCmpt.GetCountByEditorType(item.Group);  //某大类item
                    else if (item.isAnyPrototype)
                        num = entity.packageCmpt.GetAllItemsCount();                //全部item

                    if (Utility.Compare(num, count, comp))
                        return true;
                }
            }
            return false;
        }

        protected override void SendReq()
        {
            byte[] data = BufferHelper.Export(w =>
            {
                w.Write(reqId);
                BufferHelper.Serialize(w, player);
                BufferHelper.Serialize(w, item);
                w.Write(count);
                w.Write((byte)comp);
            });

            PlayerNetwork.RequestServer(EPacketType.PT_Custom_CheckOwerItem, data);
        }
    }
}
