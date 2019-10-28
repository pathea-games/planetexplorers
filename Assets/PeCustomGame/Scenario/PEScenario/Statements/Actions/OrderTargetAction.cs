using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("ORDER TARGET", true)]
    public class OrderTargetAction : ScenarioRTL.Action
    {
        // 在此列举参数
        OBJECT obj;	// NPOBJECT
        ECommand cmd;
        OBJECT tar;	// ENTITY

        // 在此初始化参数
        protected override void OnCreate()
        {
            obj = Utility.ToObject(parameters["object"]);
            cmd = (ECommand)Utility.ToEnumInt(parameters["command"]);
            tar = Utility.ToObject(parameters["target"]);
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
                    BufferHelper.Serialize(w, tar);
                    w.Write((byte)cmd);
                });

                PlayerNetwork.RequestServer(EPacketType.PT_Custom_OrderTarget, data);
            }
            else
            {
                Pathea.PeEntity entity = PeScenarioUtility.GetEntity(obj);
                Pathea.PeEntity target = PeScenarioUtility.GetEntity(tar);
                if (entity != null && target != null)
                {
                    if ((entity.proto == Pathea.EEntityProto.Npc ||
                        entity.proto == Pathea.EEntityProto.RandomNpc ||
                        entity.proto == Pathea.EEntityProto.Monster) &&
                        entity.requestCmpt != null)
                    {
                        if (cmd == ECommand.MoveTo)
                            entity.requestCmpt.Register(Pathea.EReqType.FollowTarget, target.Id);
                        else if (cmd == ECommand.FaceAt)
                            entity.requestCmpt.Register(Pathea.EReqType.Dialogue, "", target.peTrans);
                    }
                }
            }

            return true;
        }
    }
}
