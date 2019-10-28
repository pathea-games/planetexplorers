using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("MODIFY PACKAGE", true)]
    public class ModifyPackageAction : ScenarioRTL.Action
    {
		// 在此列举参数
		OBJECT item;   // ITEM
		EFunc func;
		int count;
		OBJECT player;  // PLAYER

		// 在此初始化参数
		protected override void OnCreate()
		{
			item = Utility.ToObject(parameters["item"]);
			func = Utility.ToFunc(parameters["func"]);
			count = Utility.ToInt(missionVars, parameters["count"]);
			player = Utility.ToObject(parameters["player"]);
		}
        
        // 执行动作
        // 若为瞬间动作，返回true；
        // 若为持续动作，该函数会每帧被调用，直到返回true
        public override bool Logic()
        {
            if (Pathea.PeGameMgr.IsMulti)
            {
                byte[] data = PETools.Serialize.Export(w =>
                {
                    BufferHelper.Serialize(w, player);
                    BufferHelper.Serialize(w, item);
                    w.Write(count);
                    w.Write((byte)func);
                });

                PlayerNetwork.RequestServer(EPacketType.PT_Custom_ModifyPackage, data);
            }
            else
            {
                if (player.type == OBJECT.OBJECTTYPE.Player && item.type == OBJECT.OBJECTTYPE.ItemProto && item.isSpecificPrototype)
                {
                    Pathea.PeEntity entity = PeScenarioUtility.GetEntity(player);
                    if (entity != null && entity.packageCmpt != null)
                    {
                        int num = entity.packageCmpt.GetItemCount(item.Id);
                        int result = Utility.Function(num, count, func);
                        if (result > 0)
                            entity.packageCmpt.Set(item.Id, result);
                        else
                        {
                            entity.packageCmpt.Destory(item.Id, num);
                            if (result < 0)
                                Debug.LogWarning(string.Format("Items whose protoID is {0} are not enough.",item.Id));
                        }

                        if (result - num > 0)
                        {
                            ItemAsset.ItemProto proto = ItemAsset.ItemProto.Mgr.Instance.Get(item.Id);
                            if (proto == null)
                                return true;

                            string msg = proto.GetName() + " X " + (result - num).ToString();
                            new PeTipMsg(msg, proto.icon[0], PeTipMsg.EMsgLevel.Norm, PeTipMsg.EMsgType.Misc);
                        }
                    }
                }
            }
            return true;
        }
    }
}
