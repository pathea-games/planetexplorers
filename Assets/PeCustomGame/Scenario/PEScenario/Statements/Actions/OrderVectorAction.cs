using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("ORDER VECTOR", true)]
    public class OrderVectorAction : ScenarioRTL.Action
    {
        // 在此列举参数
        OBJECT obj;	// NPOBJECT
        ECommand cmd;
        EFunc func;
        Vector3 pos;	// NPOBJECT

        // 在此初始化参数
        protected override void OnCreate()
        {
            obj = Utility.ToObject(parameters["object"]);
            cmd = (ECommand)Utility.ToEnumInt(parameters["command"]);
            func = Utility.ToFunc(parameters["func"]);
            pos = Utility.ToVector(missionVars, parameters["vector"]);
        }

        // 执行动作
        // 若为瞬间动作，返回true；
        // 若为持续动作，该函数会每帧被调用，直到返回true
        public override bool Logic()
        {
            Pathea.PeEntity entity = PeScenarioUtility.GetEntity(obj);
            if (entity != null)
            {
                if ((entity.proto == Pathea.EEntityProto.Npc ||
                    entity.proto == Pathea.EEntityProto.RandomNpc ||
                    entity.proto == Pathea.EEntityProto.Monster) &&
                    entity.requestCmpt != null)
                {
                    if (cmd == ECommand.MoveTo)
                        entity.requestCmpt.Register(Pathea.EReqType.MoveToPoint, LocalToWorld(entity.peTrans, pos, func), 1f, true, Pathea.SpeedState.Run);
                    else if (cmd == ECommand.FaceAt)
                        entity.requestCmpt.Register(Pathea.EReqType.Dialogue, "", LocalToWorld(entity.peTrans, pos, func));
                }
            }
            return true;
        }

        const int radius = 5;   //若点不是合法的站立点，将在一定区域内搜索合法站立点，这是此区域的半径
        private Vector3 LocalToWorld(Pathea.PeTrans trans, Vector3 local, EFunc func)
        {
            Vector3 world = Vector3.Cross(Vector3.up, trans.forward) * pos.x + Vector3.up * pos.y + trans.forward * pos.z;
            if (func == EFunc.Plus)
                world = PETools.PEUtil.GetRandomPositionOnGround(trans.position + world, 0, radius);
            else if (func == EFunc.Minus)
                world = PETools.PEUtil.GetRandomPositionOnGround(trans.position - world, 0, radius);
            else if (func == EFunc.SetTo)
                world = local;
            return world;
        }
    }
}
