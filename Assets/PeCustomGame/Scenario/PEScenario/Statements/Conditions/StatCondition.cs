using UnityEngine;
using ScenarioRTL;
using Pathea;

namespace PeCustom
{
    [Statement("STAT")]
    public class StatCondition : ScenarioRTL.Condition
    {
        // 在此列举参数
        OBJECT obj;  // ENTITY
        AttribType stat;
        ECompare comp;
        float amt;

        // 在此初始化参数
        protected override void OnCreate()
        {
            obj = Utility.ToObject(parameters["object"]);
            stat = (AttribType)Utility.ToEnumInt(parameters["stat"]);
            comp = Utility.ToCompare(parameters["compare"]);
            amt = Utility.ToSingle(missionVars, parameters["amount"]);
        }

        // 判断条件
        public override bool? Check()
        {
            PeEntity entity = PeScenarioUtility.GetEntity(obj);
            if (entity != null && entity.skEntity != null)
            {
                if (Utility.Compare(entity.skEntity.GetAttribute((int)stat), amt, comp))
                    return true;
            }
            return false;
        }

        protected override void SendReq()
        {
            byte[] data = BufferHelper.Export(w =>
            {
                w.Write(reqId);
                BufferHelper.Serialize(w, obj);
                w.Write(amt);
                w.Write((byte)stat);
                w.Write((byte)comp);
            });

            PlayerNetwork.RequestServer(EPacketType.PT_Custom_CheckAttribute, data);
        }
    }
}
