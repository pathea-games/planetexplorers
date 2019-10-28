using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("KILL", true)]
    public class KillAction : ScenarioRTL.Action
    {
		// 在此列举参数
		OBJECT obj;	// OBJECT
		RANGE range;

		// 在此初始化参数
		protected override void OnCreate()
		{
			obj = Utility.ToObject(parameters["object"]);
			range = Utility.ToRange(missionVars, parameters["range"]);
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
                    BufferHelper.Serialize(w, obj);
                    BufferHelper.Serialize(w, range);
                });

                PlayerNetwork.RequestServer(EPacketType.PT_Custom_Kill, data);
            }
            else
            {
                if (obj.isPrototype)
                {
                    if (obj.type == OBJECT.OBJECTTYPE.MonsterProto)
                    {
                        foreach (var item in Pathea.EntityMgr.Instance.All)
                        {
                            if (item.commonCmpt.entityProto.proto != Pathea.EEntityProto.Monster)
                                continue;
                            if (!PeScenarioUtility.IsObjectContainEntity(obj, item))
                                continue;
                            if (!range.Contains(item.position))
                                continue;
                            item.SetAttribute(Pathea.AttribType.Hp, 0, false);
                        }
                    }
                }
                else
                {
                    Pathea.PeEntity entity = PeScenarioUtility.GetEntity(obj);
                    if (null != entity && range.Contains(entity.position))
                    {
                        entity.SetAttribute(Pathea.AttribType.Hp, 0, false);
                    }
                }
            }
            return true;
        }
    }
}
